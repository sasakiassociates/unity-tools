using System;
using System.Collections;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Sasaki.Unity
{
	public abstract class APixelFinder : MonoBehaviour, IPixelFinder
	{
		[SerializeField] Texture2D _colorStrip;
		[SerializeField] Camera _camera;

		[SerializeField] [Range(1, 16)]
		protected int _depthBuffer = 16;

		[SerializeField, HideInInspector]
		protected int _index;

		[SerializeField, HideInInspector]
		Color32[] _colors;

		(bool done, bool ready, bool running) _is;

		(int color, int camera) _counts;

		NativeArray<Color32> _buffer, _tempBuffer;

		(RenderTexture main, RenderTexture temp) _rt;

		public PixelDataContainer data { get; protected set; }

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

		public bool isRunning
		{
			get => _is.running;
			protected set => _is.running = value;
		}

		public bool isDone
		{
			get => _is.done;
			protected set => _is.done = value;
		}

		public bool isReady
		{
			get => _is.ready;
			protected set => _is.ready = value;
		}

		public int cameraCount
		{
			get => _counts.camera;
			protected set => _counts.camera = value;
		}

		/// <summary>
		/// Reports the total collection size 
		/// </summary>
		public int collectionCount
		{
			get => data.Data?.Length ?? 0;
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
		protected virtual void OnEnable()
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

			_tempBuffer = new NativeArray<Color32>(size * size,
			                                       Allocator.Persistent,
			                                       NativeArrayOptions.UninitializedMemory);
		}

		protected virtual void OnDestroy()
		{
			SafeClean();
		}

		protected virtual void OnDisable()
		{
			SafeClean();
		}

		protected virtual void SafeClean()
		{
			AsyncGPUReadback.WaitAllRequests();

			if (_rt.main != null) _rt.main.Release();
			if (_rt.temp != null) _rt.temp.Release();

			if (_buffer != default && _buffer.Any()) _buffer.Dispose();
			if (_tempBuffer != default && _tempBuffer.Any()) _tempBuffer.Dispose();
		}
		#endregion

		public void Init(Color32 color, Action onDone, int collectionSize = 1, int cameraTotal = 6)
		{
			Init(new[] { color }, onDone, collectionSize);
		}

		public const uint MAX_VALUE = 16384;
		public const uint MAX_PIXELS_IN_VIEW = 2223114636;

		Action OnDone;

		public virtual void Init(Color32[] inputColors, Action onDone, int collectionSize = 1, int cameraTotal = 6)
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

			OnDone = onDone;
		}

		/// <summary>
		/// Render the camera and store the data into the container
		/// </summary>
		/// <param name="dataIndex">The index of where this data should be stored, Defaults to 0</param>
		public IEnumerator Run(int dataIndex = 0)
		{
			_index = dataIndex;
			isDone = false;

			// NOTE: Important to wait for end of frame to get a fresh view
			yield return new WaitForEndOfFrame();

			// NOTE: The async callback will dispose of all collections when using StartCoroutine. 
			Render();

			yield return new WaitUntil(() => isDone);
		}

		/// <summary>
		/// Render the camera and store the data into the container
		/// </summary>
		void Render()
		{
			Graphics.Blit(_rt.main, _rt.temp);

			AsyncGPUReadback.RequestIntoNativeArray
			(ref _buffer, _rt.temp, 0, request =>
			{
				if (request.hasError) throw new Exception("AsyncGPUReadback.RequestIntoNativeArray");

				OnCompleteReadback(request);
				isDone = true;
				OnDone?.Invoke();

				// _tempBuffer.Dispose();
				// _tempBuffer = new NativeArray<Color32>(request.GetData<Color32>(), Allocator.Persistent);
			});
		}

		protected abstract void OnCompleteReadback(AsyncGPUReadbackRequest request);
	}
}