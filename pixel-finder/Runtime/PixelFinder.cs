using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Sasaki.Unity
{
	[RequireComponent(typeof(Camera))]
	public class PixelFinder : MonoBehaviour
	{

		const int ViewSize = 512;

		[SerializeField] Camera _camera;

		[SerializeField] [Range(1, 16)]
		int depthBuffer = 16;

		[SerializeField] Texture2D _colorStrip;

		[SerializeField] uint[] histogramData;

		[SerializeField] bool debugViewer;

		[SerializeField] [HideInInspector]
		int colorCount;

		[SerializeField] ComputeShader pixelShader;

		PixelDataContainer _container;

		bool _counterReady;

		ComputeBuffer _histogramBuffer;

		int _kernInitialize;

		public bool jobDone { get; set; }

		public bool isRunning { get; private set; }

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

		public float orthoSize
		{
			set
			{
				if (!_camera.orthographic)
					_camera.orthographic = true;
				_camera.orthographicSize = value;
			}
		}

		public Color background
		{
			set => _camera.backgroundColor = value;
		}

		public RenderTexture texture { get; private set; }

		void SafeClean()
		{
			_histogramBuffer?.Dispose();
			if (texture != null)
				texture.Release();
		}

		void SetKernels()
		{
			if (_colorStrip == null)
			{
				Debug.LogError($"texture on {this} is not ready yet");
				return;
			}

			CreateBuffers();

			_kernInitialize = pixelShader.FindKernel(PixelFinderInit);
			pixelShader.SetBuffer(_kernInitialize, PixelCountBuffer, _histogramBuffer);

			_kernMain = pixelShader.FindKernel(PixelFinderMain);
			pixelShader.SetBuffer(_kernMain, PixelCountBuffer, _histogramBuffer);
			pixelShader.SetTexture(_kernMain, ColorArrayTexture, _colorStrip);
			pixelShader.SetInt(InputTextureSize, 512);

			if (_kernInitialize < 0 || _kernMain < 0 || null == _histogramBuffer || null == histogramData)
				_counterReady = false;
			else
				_counterReady = true;
		}

		void CreateBuffers()
		{
			_histogramBuffer?.Dispose();

			_histogramBuffer = new ComputeBuffer(colorCount, sizeof(uint));

			histogramData = new uint[colorCount];

			pixelShader.SetInt(ColorArraySize, colorCount);
		}

		public void Init(Color32 color, int size = 1)
		{
			Init(new[] { color }, size);
		}

		public void Init(Color32[] colors, int size = 1)
		{
			jobDone = false;

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

			if (colors == null || !colors.Any())
			{
				Debug.LogWarning("No Active Colors Ready");
				return;
			}

			if (_colorStrip != null)
				Destroy(_colorStrip);

			_colorStrip = colors.DrawPixelLine();

			if (_colorStrip == null)
			{
				Debug.LogError("Texture did not set");
				return;
			}

			_container = new PixelDataContainer(size);

			_camera = gameObject.GetComponent<Camera>();
			if (_camera == null)
				_camera = gameObject.AddComponent<Camera>();

			aspect = 1;
			fov = 90f;
			maxClipping = 10000;
			background = Color.black;
			flags = CameraClearFlags.Color;

			texture = RenderTexture.GetTemporary(ViewSize, ViewSize, depthBuffer);
			texture.name = $"{gameObject.name}-CameraTexture";

			_camera.targetTexture = texture;

			colorCount = colors.Length;
			SetKernels();
		}

		public void Render()
		{
			if (!_counterReady)
			{
				Debug.Log($"{name} is not ready");
				return;
			}

			_camera.Render();
			Process(_camera.targetTexture);

			Debug.Log($"{name} value = {_container.Data[0][0]}");
		}

		void Process(Texture txt)
		{
			if (_histogramBuffer.count != colorCount)
			{
				Debug.Log("Updating Buffer");
				CreateBuffers();
			}

			pixelShader.SetBuffer(_kernInitialize, PixelCountBuffer, _histogramBuffer);
			pixelShader.SetBuffer(_kernMain, PixelCountBuffer, _histogramBuffer);
			pixelShader.SetTexture(_kernMain, InputTexture, txt);
			pixelShader.Dispatch(_kernInitialize, 256 / 64, 1, 1);
			pixelShader.Dispatch(_kernMain, (txt.width + 7) / 8, (txt.height + 7) / 8, 1);

			_histogramBuffer.GetData(histogramData);
			_container.Set(histogramData, 6);
		}

		internal readonly struct PixelDataContainer
		{
			public PixelDataContainer(int size = 0) => Data = new double[size][];

			//Old Value for Max 1395882500

			/// <summary>
			///   Max Pixel Count in a single viewer as measured :D
			/// </summary>
			public const uint PIXELS_IN_VIEW = 2223114636;

			public double[][] Data { get; }

			public void Set(uint[] dataFromShader, int cameraCount, int index = 0)
			{
				var data = new double[dataFromShader.Length];

				for (var i = 0; i < dataFromShader.Length; i++)
					data[i] = (double)dataFromShader[i] / PIXELS_IN_VIEW / cameraCount;

				Data[index] = data;
			}
		}

		#region Shader Parameters
		const string PixelFinderInit = "PixelFinderInitialize";

		const string PixelFinderMain = "PixelFinderMain";

		const string InputTexture = "InputTexture";

		const string InputTextureSize = "InputTextureSize";

		const string ColorArraySize = "ColorArraySize";

		const string ColorArrayTexture = "ColorArrayTexture";

		const string PixelCountBuffer = "PixelCountBuffer";

		int _kernMain;
		#endregion

		#region Unity Functions
		void OnEnable()
		{
			SafeClean();
		}

		void OnDestroy()
		{
			SafeClean();
		}

		void OnDisable()
		{
			SafeClean();
		}
		#endregion

	}
}