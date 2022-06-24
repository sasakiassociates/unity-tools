using System;
using System.Collections;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Sasaki.Unity
{
	[RequireComponent(typeof(Camera))]
	public sealed class PixelFinder : MonoBehaviour, IPixelFinder
	{
		[SerializeField] Texture2D _colorStrip;
		[SerializeField] Camera _camera;

		[SerializeField] [Range(1, 16)]
		int _depthBuffer = 16;

		[SerializeField, HideInInspector]
		int _index;

		[SerializeField, HideInInspector]
		Color32[] _colors;

		(bool done, bool ready) _is;

		(int color, int camera) _counts;

		NativeArray<Color32> _buffer;

		(RenderTexture main, RenderTexture temp) _rt;

		[SerializeField] uint[] histogramData;

		[SerializeField] ComputeShader pixelShader;

		ComputeBuffer _histogramBuffer;

		public const uint MAX_VALUE = 16384;
		public const uint MAX_PIXELS_IN_VIEW = 2223114636;

		//Old Value for Max 1395882500

		#region Shader Parameters

		const string PixelFinderInit = "PixelFinderInitialize";

		const string PixelFinderMain = "PixelFinderMain";

		const string InputTexture = "InputTexture";

		const string InputTextureSize = "InputTextureSize";

		const string ColorArraySize = "ColorArraySize";

		const string ColorArrayTexture = "ColorArrayTexture";

		const string PixelCountBuffer = "PixelCountBuffer";

		int _kernMain, _kernInitialize;

		#endregion

		public PixelDataContainer data { get; set; }

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

		public bool isDone
		{
			get => _is.done;
			set => _is.done = value;
		}

		public bool isReady
		{
			get => _is.ready;
			set => _is.ready = value;
		}

		public int cameraCount
		{
			get => _counts.camera;
			set => _counts.camera = value;
		}

		/// <summary>
		/// Reports the total collection size 
		/// </summary>
		public int collectionCount
		{
			get => data.data?.Length ?? 0;
		}

		public int colorCount
		{
			get => _colors?.Length ?? 0;
		}

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

		public float maxClipping
		{
			get => _camera.farClipPlane;
			private set => _camera.farClipPlane = value;
		}

		public float nearClipping
		{
			get => _camera.nearClipPlane;
			private set => _camera.nearClipPlane = value;
		}

		public float aspect
		{
			get => _camera.aspect;
			private set => _camera.aspect = value;
		}

		public float fov
		{
			get => _camera.fieldOfView;
			set => _camera.fieldOfView = value;
		}

		public CameraClearFlags flags
		{
			get => _camera.clearFlags;
			set => _camera.clearFlags = value;
		}

		public Color background
		{
			get => _camera.backgroundColor;
			set => _camera.backgroundColor = value;
		}

		public float orthographicSize
		{
			get => cam.orthographicSize;
			set => cam.orthographicSize = value;
		}

		public Camera cam
		{
			get => _camera;
		}

		public RenderTexture texture
		{
			get => _rt.main;
		}

		public int size
		{
			get => 512;
		}

		#region Unity Functions

		void OnEnable()
		{
			SafeClean();

			_rt.main = new RenderTexture(size, size, _depthBuffer);
			_rt.main.name = $"{gameObject.name}-CameraTexture-Main";

			_rt.temp = new RenderTexture(size, size, _depthBuffer);
			// _rt.temp = RenderTexture.GetTemporary(size, size, _depthBuffer);
			_rt.temp.name = $"{gameObject.name}-CameraTexture-Temp";

			_camera = gameObject.GetComponent<Camera>();
			if (_camera == null)
				_camera = gameObject.AddComponent<Camera>();

			_camera.targetTexture = _rt.main;

			_buffer = new NativeArray<Color32>(size * size,
			                                   Allocator.Persistent,
			                                   NativeArrayOptions.UninitializedMemory);
		}

		void OnDestroy()
		{
			SafeClean();
		}

		void OnDisable()
		{
			SafeClean();
		}

		void SafeClean()
		{
			AsyncGPUReadback.WaitAllRequests();

			if (_rt.main != null) _rt.main.Release();
			if (_rt.temp != null) _rt.temp.Release();

			if (_buffer != default && _buffer.Any()) _buffer.Dispose();
		}

		#endregion

		public void Init(Color32 color, Action onDone, int collectionSize = 1, int cameraTotal = 6)
		{
			Init(new[] { color }, onDone, collectionSize);
		}

		/// <summary>
		/// Resets the data collection, keeping the same colors and camera total
		/// Use Init() for complete reset of finder
		/// </summary>
		public void SetNewDataCollection(int collectionSize = 1)
		{
			data = new PixelDataContainer(collectionSize);
		}

		Action OnDone;

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

			_kernInitialize = pixelShader.FindKernel(PixelFinderInit);
			pixelShader.SetBuffer(_kernInitialize, PixelCountBuffer, _histogramBuffer);

			_kernMain = pixelShader.FindKernel(PixelFinderMain);
			pixelShader.SetBuffer(_kernMain, PixelCountBuffer, _histogramBuffer);
			pixelShader.SetTexture(_kernMain, ColorArrayTexture, colorStrip);
			pixelShader.SetInt(InputTextureSize, 512);

			if (_kernInitialize < 0 || _kernMain < 0 || null == _histogramBuffer || null == histogramData)
				isReady = false;
			else
				isReady = true;

			OnDone = onDone;
		}

		/// <summary>
		/// Render the camera and store the data into the container
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
		/// Render the camera and store the data into the container
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

			pixelShader.SetBuffer(_kernInitialize, PixelCountBuffer, _histogramBuffer);
			pixelShader.SetBuffer(_kernMain, PixelCountBuffer, _histogramBuffer);
			pixelShader.SetTexture(_kernMain, InputTexture, texture);
			pixelShader.Dispatch(_kernInitialize, 256 / 64, 1, 1);
			pixelShader.Dispatch(_kernMain, (texture.width + 7) / 8, (texture.height + 7) / 8, 1);

			// NOTE: Performance impact with snagging the GPU data
			_histogramBuffer.GetData(histogramData);

			var res = new double[histogramData.Length];

			for (var i = 0; i < histogramData.Length; i++)
				res[i] = (double)histogramData[i] / MAX_PIXELS_IN_VIEW / cameraCount;

			data.Set(res, _index);
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