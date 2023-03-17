using System;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;


namespace Sasaki.Unity
{

  [RequireComponent(typeof(Camera))]
  public sealed class PixelFinder : MonoBehaviour, IPixelFinder, IPixelFinderEvents
  {

    internal static class Kernel
    {
      internal const string INPUT_TEXTURE = "inputTexture";
      internal const string INPUT_TEXTURE_SIZE = "inputTextureSize";
      internal const string COLOR_ARRAY_SIZE = "colorArraySize";
      internal const string COLOR_ARRAY_TEXTURE = "colorArrayTexture";
      internal const string PIXEL_COUNT_BUFFER = "pixelCountBuffer";

      internal const string DEBUG_FLOAT_BUFFER = "debugFloatBuffer";
      internal const string DEBUG_UINT_BUFFER = "debugUintBuffer";

      internal static int PixelFinderMain { get; set; }
      internal static int PixelFinderInitialize { get; set; }
      internal static int DebugView { get; set; }
      internal static int DebugViewValues { get; set; }
      internal static int CompileDebugViews { get; set; }
    }

    [SerializeField] Texture2D colorStrip;

    [SerializeField] Camera cam;

    [SerializeField][Range(1, 16)] int depthBuffer = 16;

    [SerializeField][HideInInspector] Color32[] colors;

    [SerializeField] uint[] histogramData;

    [SerializeField] ComputeShader pixelShader;

    NativeArray<Color32> _buffer;

    ComputeBuffer _histogramBuffer;

    (RenderTexture main, RenderTexture temp) _rt;



    /// <summary>
    /// Pixel data gathered from the GPU and stored after <see cref="AsyncGPUReadbackRequest"/>
    /// </summary>
    public uint[] PixelData
    {
      get => histogramData;
    }

    /// <summary>
    ///   The pixel colors to search for
    /// </summary>
    public Color32[] Colors
    {
      get => colors;
      private set
      {
        if(value == null || !value.Any())
        {
          Debug.LogWarning("No Active Colors Ready");
          return;
        }

        colors = value;
        ColorStrip = Colors.DrawPixelLine();
      }
    }

    /// <summary>
    ///   Returns true if all analysis callbacks are complete
    /// </summary>
    public bool WorkComplete { get; private set; }

    /// <summary>
    ///   Checks if all buffers and data area ready for processing
    /// </summary>
    public bool IsReady { get; private set; }

    /// <summary>
    ///   Count of colors being searched for
    /// </summary>
    public int ColorCount => colors?.Length ?? 0;

    /// <summary>
    ///   The selected colors being passed into the gpu
    /// </summary>
    public Texture2D ColorStrip
    {
      get => colorStrip;
      private set
      {
        if(colorStrip != null)
          Destroy(colorStrip);

        colorStrip = value;
      }
    }

    /// <summary>
    ///   Far clipping plane for attached camera
    /// </summary>
    public float MaxClipping
    {
      get => cam.farClipPlane;
      set => cam.farClipPlane = value;
    }

    /// <summary>
    ///   Near clipping plane for attached camera
    /// </summary>
    public float NearClipping
    {
      get => cam.nearClipPlane;
      set => cam.nearClipPlane = value;
    }

    /// <summary>
    ///   Aspect Ratio of the attached camera
    /// </summary>
    public float Aspect
    {
      get => cam.aspect;
      set => cam.aspect = value;
    }

    /// <summary>
    ///   FOV for the attached camera
    /// </summary>
    public float Fov
    {
      get => cam.fieldOfView;
      set => cam.fieldOfView = value;
    }

    /// <summary>
    ///   Culling mask value for the attached camera
    /// </summary>
    public int Mask
    {
      get => cam.cullingMask;
      set => cam.cullingMask = value;

    }

    /// <summary>
    ///   Flags for the attached camera
    /// </summary>
    public CameraClearFlags Flags
    {
      get => cam.clearFlags;
      set => cam.clearFlags = value;
    }

    /// <summary>
    ///   Orthographic size for attached camera. Setting this will automatically set the camera to ortho mode
    /// </summary>
    public float OrthographicSize
    {
      get => Cam.orthographicSize;
      set => Cam.orthographicSize = value;
    }

    /// <summary>
    ///   Background color for the attached camera
    /// </summary>
    public Color Background
    {
      get => cam.backgroundColor;
      set => cam.backgroundColor = value;
    }

    /// <summary>
    ///   Attached camera of this viewer
    /// </summary>
    public Camera Cam => cam;

    /// <summary>
    ///   Active texture being passed for analysis
    /// </summary>
    public RenderTexture Texture => _rt.main;

    /// <summary>
    ///   Hard coded size for the camera. Set to 512x512
    /// </summary>
    public int TextureSize => 512;

    void OnEnable()
    {
      SafeClean();

      if(pixelShader == null)
      {
        pixelShader = Addressables.LoadAssetAsync<ComputeShader>("PixelFinder").WaitForCompletion();
      }

      // NOTE: the GC seems to dispose of the buffers or something like that
      SceneManager.sceneUnloaded += _ => SafeClean();

      _rt.main = new RenderTexture(TextureSize, TextureSize, depthBuffer);
      _rt.main.name = $"{gameObject.name}-CameraTexture-Main";

      _rt.temp = new RenderTexture(TextureSize, TextureSize, depthBuffer);
      _rt.temp.name = $"{gameObject.name}-CameraTexture-Temp";

      cam = gameObject.GetComponent<Camera>();
      if(cam == null) cam = gameObject.AddComponent<Camera>();

      cam.targetTexture = _rt.main;

      _buffer = new NativeArray<Color32>(TextureSize * TextureSize,
        Allocator.Persistent,
        NativeArrayOptions.UninitializedMemory);
    }

    void OnDisable() => SafeClean();

    void OnDestroy() => SafeClean();

    /// <summary>
    ///   Initializes a new pixel finder, clearing all data and parameters
    /// </summary>
    /// <param name="color"></param>
    /// <param name="onDone"></param>
    /// <param name="collectionSize"></param>
    /// <param name="cameraTotal"></param>
    public void Init(Color32 color, UnityAction onDone = null, int collectionSize = 1, int cameraTotal = 6) =>
      Init(new[] {color}, onDone, collectionSize);

    /// <summary>
    ///   Initializes a new pixel finder, clearing all data and parameters
    /// </summary>
    /// <param name="inputColors"></param>
    /// <param name="onDone"></param>
    /// <param name="collectionSize"></param>
    /// <param name="cameraTotal"></param>
    public void Init(Color32[] inputColors, UnityAction onDone = null, int collectionSize = 1, int cameraTotal = 6)
    {
      // draws the texture needed for analysis
      Colors = inputColors;

      if(ColorStrip == null)
      {
        Debug.LogError("Texture did not set");
        return;
      }

      Aspect = 1;
      Fov = 90f;
      MaxClipping = 10000;
      Background = new Color32(0, 0, 0, 255);
      Flags = CameraClearFlags.Color;



      CreateBuffers();

      Kernel.PixelFinderInitialize = pixelShader.FindKernel(nameof(Kernel.PixelFinderInitialize));
      pixelShader.SetBuffer(Kernel.PixelFinderInitialize, Kernel.PIXEL_COUNT_BUFFER, _histogramBuffer);

      Kernel.PixelFinderMain = pixelShader.FindKernel(nameof(Kernel.PixelFinderMain));

      pixelShader.SetBuffer(Kernel.PixelFinderMain, Kernel.PIXEL_COUNT_BUFFER, _histogramBuffer);
      pixelShader.SetTexture(Kernel.PixelFinderMain, Kernel.COLOR_ARRAY_TEXTURE, ColorStrip);
      pixelShader.SetInt(Kernel.INPUT_TEXTURE_SIZE, TextureSize);

      IsReady = Kernel.PixelFinderInitialize < 0 || Kernel.PixelFinderMain < 0 || null == _histogramBuffer || null == histogramData;
      WorkComplete = true;

      OnDone = onDone;
    }

    public void PreRender()
    {
      WorkComplete = false;
    }

    /// <summary>
    ///   Render the camera and store the data into the container
    /// </summary>
    public IEnumerator Render()
    {
      // _index = dataIndex;

      // NOTE: Important to wait for end of frame to get a fresh view
      yield return new WaitForEndOfFrame();

      // TODO: Fix so when the UI calls to move this it doesnt trigger a second rendering call 
      Graphics.Blit(_rt.main, _rt.temp);

      // NOTE: The async callback will dispose of all collections when using StartCoroutine. 
      AsyncGPUReadback.RequestIntoNativeArray
      (ref _buffer, _rt.temp, 0, request =>
      {
        if(request.hasError) throw new Exception("AsyncGPUReadback.RequestIntoNativeArray");

        OnCompleteReadback(request);

        WorkComplete = true;

        OnDone?.Invoke();
      });

      yield return null;
    }

    void OnCompleteReadback(AsyncGPUReadbackRequest _)
    {
      if(_histogramBuffer?.count != ColorCount)
      {
        Debug.Log("Updating Buffer");
        CreateBuffers();
      }

      pixelShader.SetBuffer(Kernel.PixelFinderInitialize, Kernel.PIXEL_COUNT_BUFFER, _histogramBuffer);
      pixelShader.SetBuffer(Kernel.PixelFinderMain, Kernel.PIXEL_COUNT_BUFFER, _histogramBuffer);
      pixelShader.SetTexture(Kernel.PixelFinderMain, Kernel.INPUT_TEXTURE, Texture);
      pixelShader.Dispatch(Kernel.PixelFinderInitialize, 256 / 64, 1, 1);
      pixelShader.Dispatch(Kernel.PixelFinderMain, (Texture.width + 7) / 8, (Texture.height + 7) / 8, 1);
      _histogramBuffer.GetData(histogramData);

      OnValueSet?.Invoke(histogramData);
    }

    void SafeClean()
    {
      AsyncGPUReadback.WaitAllRequests();

      if(_rt.main != null) _rt.main.Release();
      if(_rt.temp != null) _rt.temp.Release();

      if(_buffer.IsCreated && _buffer.Any()) _buffer.Dispose();

      _histogramBuffer?.Dispose();
    }

    void CreateBuffers()
    {
      _histogramBuffer?.Dispose();

      _histogramBuffer = new ComputeBuffer(ColorCount, sizeof(uint));

      histogramData = new uint[ColorCount];

      pixelShader.SetInt(Kernel.COLOR_ARRAY_SIZE, ColorCount);
    }

    public RenderTexture DebugView(Gradient gradient)
    {
      var debugColors = GetColors(gradient, gradient.colorKeys.Length);

      var debugColorBuffer = new ComputeBuffer(debugColors.Length, Marshal.SizeOf<float4>(), ComputeBufferType.Structured);
      debugColorBuffer.SetData(debugColors);
      var debugTexture = CreateTexture(RenderTextureFormat.ARGB64, TextureSize, "uint-texture");

      Kernel.DebugView = pixelShader.FindKernel(nameof(Kernel.DebugView));
      pixelShader.SetBuffer(Kernel.DebugView, nameof(debugColorBuffer), debugColorBuffer);
      pixelShader.SetTexture(Kernel.DebugView, "debugTexture", debugTexture);
      pixelShader.SetInt("colorBufferCount", debugColors.Length - 1);
      pixelShader.Dispatch(Kernel.DebugView, (debugTexture.width + 7) / 8, (debugTexture.height + 7) / 8, 1);

      debugColorBuffer.Dispose();
      return debugTexture;
    }

    public void DebugViewValues()
    {

      var uintText2D = CreateTexture(RenderTextureFormat.RInt, TextureSize, "uint-texture");
      var floatText2D = CreateTexture(RenderTextureFormat.RFloat, TextureSize, "float-texture");

      var uintBuffer = new ComputeBuffer(TextureSize * TextureSize, Marshal.SizeOf<uint>());
      var floatBuffer = new ComputeBuffer(TextureSize * TextureSize, Marshal.SizeOf<float>());

      var uint2dBuffer = new ComputeBuffer(TextureSize * TextureSize, Marshal.SizeOf<uint>() * 2);
      var float2dBuffer = new ComputeBuffer(TextureSize * TextureSize, Marshal.SizeOf<float>() * 2);

      Kernel.DebugViewValues = pixelShader.FindKernel(nameof(Kernel.DebugViewValues));
      pixelShader.SetTexture(Kernel.DebugViewValues, nameof(uintText2D), uintText2D);
      pixelShader.SetTexture(Kernel.DebugViewValues, nameof(floatText2D), floatText2D);


      Kernel.CompileDebugViews = pixelShader.FindKernel(nameof(Kernel.CompileDebugViews));
      pixelShader.SetTexture(Kernel.CompileDebugViews, nameof(floatText2D), floatText2D);
      pixelShader.SetTexture(Kernel.CompileDebugViews, nameof(uintText2D), uintText2D);
      pixelShader.SetBuffer(Kernel.CompileDebugViews, Kernel.DEBUG_FLOAT_BUFFER, floatBuffer);
      pixelShader.SetBuffer(Kernel.CompileDebugViews, Kernel.DEBUG_UINT_BUFFER, uintBuffer);

      pixelShader.Dispatch(Kernel.DebugViewValues, 512, 512, 1);
      pixelShader.Dispatch(Kernel.CompileDebugViews, 1, 1, 1);

      _floatValues = new float[TextureSize * TextureSize];
      floatBuffer.GetData(_floatValues);
      _uintValues = new uint[TextureSize * TextureSize];
      uintBuffer.GetData(_uintValues);

      StartCoroutine(LogValues());

      floatBuffer.Dispose();
      float2dBuffer.Dispose();
      uintBuffer.Dispose();
      uint2dBuffer.Dispose();

    }

    float[] _floatValues;
    uint[] _uintValues;

    IEnumerator LogValues()
    {

      for(int x = 0, i = 0; x < TextureSize; x++)
      {
        for(int y = 0; y < TextureSize; y++, i++)
        {
          Debug.Log($"{x},{y}= {_floatValues[i]}f : {_uintValues[i]} ");
        }
        yield return null;
      }

    }

    /// <inheritdoc />
    public event UnityAction<uint[]> OnValueSet;

    /// <inheritdoc />
    public event UnityAction<FinderCaptureArgs> OnCapture;

    /// <inheritdoc />
    public event UnityAction<FinderCaptureArgs> OnScreenCapture;

    /// <inheritdoc />
    public event UnityAction OnDone;

    public static RenderTexture CreateTexture(RenderTextureFormat format, int res, string name = "tex", FilterMode fm = FilterMode.Point, TextureWrapMode wm = TextureWrapMode.Repeat)
    {
      var texture = new RenderTexture(res, res, 1, format)
      {
        name = name,
        enableRandomWrite = true,
        filterMode = fm,
        wrapMode = wm,
        volumeDepth = 1,
        dimension = TextureDimension.Tex2D,
        autoGenerateMips = false,
        useMipMap = false
      };

      texture.Create();
      return texture;
    }

    public static float4[] GetColors(Gradient gradient, int nStates)
    {
      var colors = new float4[nStates];
      for(int i = 0; i < nStates; i++)
      {
        var time = 0.0f;
        if(i == nStates - 1)
        {
          time = 1.0f;
        }
        else if(i > 0)
        {
          time = i / (float)nStates;
        }

        var c = gradient.Evaluate(time);
        colors[i] = new float4(c.r, c.g, c.b, c.a);
      }

      return colors;

    }



  }


}
