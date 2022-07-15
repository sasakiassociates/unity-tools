using System;
using System.Collections;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Sasaki.Unity
{
	[RequireComponent(typeof(Camera))]
	public sealed class PixelFinder : MonoBehaviour, IPixelFinder
	{
		public const uint MAX_VALUE = 16384;

		public const uint MAX_PIXELS_IN_VIEW = 2223114636;

		[Obsolete("Old max value for possible pixels in a view. Use MAX_VALUE", true)]
		public const uint MAX_VALUE_OLD = 1395882500;

		const string PixelFinderInit = "PixelFinderInitialize";

		const string PixelFinderMain = "PixelFinderMain";

		const string InputTexture = "InputTexture";

		const string InputTextureSize = "InputTextureSize";

		const string ColorArraySize = "ColorArraySize";

		const string ColorArrayTexture = "ColorArrayTexture";

		const string PixelCountBuffer = "PixelCountBuffer";

		[SerializeField] Texture2D _colorStrip;

		[SerializeField] Camera _camera;

		[SerializeField] [Range(1, 16)] int _depthBuffer = 16;

		[SerializeField] [HideInInspector] int _index;

		[SerializeField] [HideInInspector] Color32[] _colors;

		[SerializeField] uint[] histogramData;

		[SerializeField] ComputeShader pixelShader;

		NativeArray<Color32> _buffer;

		(int color, int camera) _counts;

		ComputeBuffer _histogramBuffer;

		(bool done, bool ready) _is;

		(int Main, int Init) _kern;

		(RenderTexture main, RenderTexture temp) _rt;

		/// <summary>
		///   Container object of the data being stored
		/// </summary>
		public PixelDataContainer data { get; set; }

		/// <summary>
		///   The pixel colors to search for
		/// </summary>
		public Color32[] colors
		{
			get => _colors;
			private set
			{
				if (value == null || !value.Any())
				{
					Debug.LogWarning("No Active Colors Ready");
					return;
				}

				_colors = value;
				_counts.color = colors.Length;

				colorStrip = colors.DrawPixelLine();
			}
		}

		/// <summary>
		///   Returns true if all analysis callbacks are complete
		/// </summary>
		public bool isDone
		{
			get => _is.done;
			set => _is.done = value;
		}

		/// <summary>
		///   Checks if all buffers and data area ready for processing
		/// </summary>
		public bool isReady
		{
			get => _is.ready;
			set => _is.ready = value;
		}

		/// <summary>
		///   Gets the amount of cameras linked with this finder
		/// </summary>
		public int cameraCount
		{
			get => _counts.camera;
			set => _counts.camera = value;
		}

		/// <summary>
		///   Reports the total collection size
		/// </summary>
		public int collectionCount
		{
			get => data.data?.Length ?? 0;
		}

		/// <summary>
		///   Count of colors being searched for
		/// </summary>
		public int colorCount
		{
			get => _colors?.Length ?? 0;
		}

		/// <summary>
		///   The selected colors being passed into the gpu
		/// </summary>
		public Texture2D colorStrip
		{
			get => _colorStrip;
			private set
			{
				if (_colorStrip != null)
					Destroy(_colorStrip);

				_colorStrip = value;
			}
		}

		/// <summary>
		///   Far clipping plane for attached camera
		/// </summary>
		public float maxClipping
		{
			get => _camera.farClipPlane;
			private set => _camera.farClipPlane = value;
		}

		/// <summary>
		///   Near clipping plane for attached camera
		/// </summary>
		public float nearClipping
		{
			get => _camera.nearClipPlane;
			private set => _camera.nearClipPlane = value;
		}

		/// <summary>
		///   Aspect Ratio of the attached camera
		/// </summary>
		public float aspect
		{
			get => _camera.aspect;
			private set => _camera.aspect = value;
		}

		/// <summary>
		///   FOV for the attached camera
		/// </summary>
		public float fov
		{
			get => _camera.fieldOfView;
			set => _camera.fieldOfView = value;
		}

		/// <summary>
		///   Culling mask value for the attached camera
		/// </summary>
		public int mask
		{
			get => _camera.cullingMask;
			set => _camera.cullingMask = value;

		}

		/// <summary>
		///   Flags for the attached camera
		/// </summary>
		public CameraClearFlags flags
		{
			get => _camera.clearFlags;
			set => _camera.clearFlags = value;
		}

		/// <summary>
		///   Orthographic size for attached camera. Setting this will automatically set the camera to ortho mode
		/// </summary>
		public float orthographicSize
		{
			get => cam.orthographicSize;
			set => cam.orthographicSize = value;
		}

		void OnEnable()
		{
			SafeClean();

			// NOTE: the GC seems to dispose of the buffers or something like that
			SceneManager.sceneUnloaded += _ => SafeClean();

			_rt.main = new RenderTexture(size, size, _depthBuffer);
			_rt.main.name = $"{gameObject.name}-CameraTexture-Main";

			_rt.temp = new RenderTexture(size, size, _depthBuffer);
			_rt.temp.name = $"{gameObject.name}-CameraTexture-Temp";

			_camera = gameObject.GetComponent<Camera>();
			if (_camera == null)
				_camera = gameObject.AddComponent<Camera>();

			_camera.targetTexture = _rt.main;

			_buffer = new NativeArray<Color32>(size * size,
			                                   Allocator.Persistent,
			                                   NativeArrayOptions.UninitializedMemory);
		}

		void OnDisable()
		{
			SafeClean();
		}

		void OnDestroy()
		{
			SafeClean();
		}

		/// <summary>
		///   Background color for the attached camera
		/// </summary>
		public Color background
		{
			get => _camera.backgroundColor;
			set => _camera.backgroundColor = value;
		}

		/// <summary>
		///   Attached camera of this viewer
		/// </summary>
		public Camera cam
		{
			get => _camera;
		}

		/// <summary>
		///   Active texture being passed for analysis
		/// </summary>
		public RenderTexture texture
		{
			get => _rt.main;
		}

		/// <summary>
		///   Hard coded size for the camera. Set to 512x512
		/// </summary>
		public int size
		{
			get => 512;
		}

		/// <summary>
		///   Callback event triggered once the analysis is completed
		/// </summary>
		event Action OnDone;

		/// <summary>
		///   Initializes a new pixel finder, clearing all data and parameters
		/// </summary>
		/// <param name="color"></param>
		/// <param name="onDone"></param>
		/// <param name="collectionSize"></param>
		/// <param name="cameraTotal"></param>
		public void Init(Color32 color, Action onDone, int collectionSize = 1, int cameraTotal = 6)
		{
			Init(new[] { color }, onDone, collectionSize);
		}

		/// <summary>
		///   Initializes a new pixel finder, clearing all data and parameters
		/// </summary>
		/// <param name="inputColors"></param>
		/// <param name="onDone"></param>
		/// <param name="collectionSize"></param>
		/// <param name="cameraTotal"></param>
		public void Init(Color32[] inputColors, Action onDone, int collectionSize = 1, int cameraTotal = 6)
		{
			// draws the texture needed for analysis
			colors = inputColors;

			if (colorStrip == null)
			{
				Debug.LogError("Texture did not set");
				return;
			}

			_counts.camera = cameraTotal;

			aspect = 1;
			fov = 90f;
			maxClipping = 10000;
			background = new Color32(0, 0, 0, 255);
			flags = CameraClearFlags.Color;

			data = new PixelDataContainer(collectionSize);

			if (pixelShader == null)
			{
				#if UNITY_EDITOR
				pixelShader =
					Instantiate(AssetDatabase.LoadAssetAtPath<ComputeShader>("Packages/com.sasaki.pixelfinder/Runtime/Shader/PixelFinder.compute"));
				#endif

				if (pixelShader == null)
				{
					Debug.LogWarning("No Active Shader Found");
					return;
				}
			}

			CreateBuffers();

			_kern.Init = pixelShader.FindKernel(PixelFinderInit);
			pixelShader.SetBuffer(_kern.Init, PixelCountBuffer, _histogramBuffer);

			_kern.Main = pixelShader.FindKernel(PixelFinderMain);
			pixelShader.SetBuffer(_kern.Main, PixelCountBuffer, _histogramBuffer);
			pixelShader.SetTexture(_kern.Main, ColorArrayTexture, colorStrip);
			pixelShader.SetInt(InputTextureSize, 512);

			if (_kern.Init < 0 || _kern.Main < 0 || null == _histogramBuffer || null == histogramData)
				isReady = false;
			else
				isReady = true;

			OnDone = onDone;
		}

		/// <summary>
		///   Resets the data collection, keeping the same colors and camera total
		///   Use Init() for complete reset of finder
		/// </summary>
		public void SetNewDataCollection(int collectionSize = 1)
		{
			data = new PixelDataContainer(collectionSize);
		}

		/// <summary>
		///   Render the camera and store the data into the container
		/// </summary>
		/// <param name="dataIndex">The index of where this data should be stored, Defaults to 0</param>
		public IEnumerator Render(int dataIndex = 0)
		{
			_index = dataIndex;
			isDone = false;

			// NOTE: Important to wait for end of frame to get a fresh view
			yield return new WaitForEndOfFrame();

			// NOTE: The async callback will dispose of all collections when using StartCoroutine. 
			RenderAndCompile();

			yield return null;
		}

		/// <summary>
		///   Render the camera and store the data into the container
		/// </summary>
		void RenderAndCompile()
		{
			Graphics.Blit(_rt.main, _rt.temp);

			AsyncGPUReadback.RequestIntoNativeArray
			(ref _buffer, _rt.temp, 0, request =>
			{
				if (request.hasError) throw new Exception("AsyncGPUReadback.RequestIntoNativeArray");

				OnCompleteReadback(request);
				isDone = true;

				OnDone?.Invoke();
			});
		}

		void OnCompleteReadback(AsyncGPUReadbackRequest request)
		{
			if (_histogramBuffer.count != colorCount)
			{
				Debug.Log("Updating Buffer");
				CreateBuffers();
			}

			pixelShader.SetBuffer(_kern.Init, PixelCountBuffer, _histogramBuffer);
			pixelShader.SetBuffer(_kern.Main, PixelCountBuffer, _histogramBuffer);
			pixelShader.SetTexture(_kern.Main, InputTexture, texture);
			pixelShader.Dispatch(_kern.Init, 256 / 64, 1, 1);
			pixelShader.Dispatch(_kern.Main, (texture.width + 7) / 8, (texture.height + 7) / 8, 1);

			// NOTE: Performance impact with snagging the GPU data
			_histogramBuffer.GetData(histogramData);

			var res = new double[histogramData.Length];

			for (var i = 0; i < histogramData.Length; i++)
				res[i] = (double)histogramData[i] / MAX_PIXELS_IN_VIEW / cameraCount;

			data.Set(res, _index);
		}

		void SafeClean()
		{
			AsyncGPUReadback.WaitAllRequests();

			if (_rt.main != null) _rt.main.Release();
			if (_rt.temp != null) _rt.temp.Release();

			if (_buffer != default && _buffer.Any()) _buffer.Dispose();
			
			_histogramBuffer?.Dispose();
		}

		void CreateBuffers()
		{
			_histogramBuffer?.Dispose();

			_histogramBuffer = new ComputeBuffer(colorCount, sizeof(uint));

			histogramData = new uint[colorCount];

			pixelShader.SetInt(ColorArraySize, colorCount);
		}
	}
}