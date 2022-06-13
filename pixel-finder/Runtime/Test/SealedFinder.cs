using System;
using System.Collections;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Sasaki.Unity
{
	public sealed class SealedFinder : MonoBehaviour
	{
		[SerializeField] Texture2D _colorStrip;
		[SerializeField] Camera _camera;

		[SerializeField] [Range(1, 16)]
		int _depthBuffer = 16;

		[SerializeField, HideInInspector]
		int _index;

		(bool done, bool ready, bool running) _is;

		(int color, int camera) _counts;

		ComputeBuffer _computeBuffer;
		NativeArray<Color32> _buffer, _tempBuffer;

		(RenderTexture main, RenderTexture temp) _rt;

		public PixelDataContainer data { get; protected set; }

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

		public int colorCount
		{
			get => _counts.color;
			protected set => _counts.color = value;
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
		void Start()
		{
			SafeClean();

			_computeBuffer = new ComputeBuffer(size * size, sizeof(uint));
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

			_computeBuffer?.Dispose();

			if (_rt.main != null) _rt.main.Release();
			if (_rt.temp != null) _rt.temp.Release();

			if (_buffer != default && _buffer.Any()) _buffer.Dispose();
			if (_tempBuffer != default && _tempBuffer.Any()) _tempBuffer.Dispose();
		}
		#endregion

		public void Init(Color32 color, int collectionSize = 1, int cameraTotal = 6)
		{
			Init(new[] { color }, collectionSize);
		}

		public void Init(Color32[] colors, int collectionSize = 1, int cameraTotal = 6)
		{
			if (colors == null || !colors.Any())
			{
				Debug.LogWarning("No Active Colors Ready");
				return;
			}

			colorStrip = colors.DrawPixelLine();
			_counts.color = colors.Length;

			if (colorStrip == null)
			{
				Debug.LogError("Texture did not set");
				return;
			}

			data = new PixelDataContainer(collectionSize);

			_counts.camera = cameraTotal;

			aspect = 1;
			fov = 90f;
			maxClipping = 10000;
			background = new Color32(0, 0, 0, 255);
			flags = CameraClearFlags.Color;
		}

		public IEnumerator Run(int dataIndex = 0)
		{
			_index = dataIndex;
			yield return new WaitForEndOfFrame();

			Render();
		}

		/// <summary>
		/// Render the camera and store the data into the container
		/// </summary>
		void Render()
		{
			// yield return new WaitForEndOfFrame();

			Graphics.Blit(_rt.main, _rt.temp);

			AsyncGPUReadback.RequestIntoNativeArray
				(ref _buffer, _rt.temp, 0, OnCompleteReadback);
		}

		void OnCompleteReadback(AsyncGPUReadbackRequest request)
		{
			if (request.hasError) throw new Exception("AsyncGPUReadback.RequestIntoNativeArray");

			// _buffer.Dispose();
			_tempBuffer.Dispose();

			_tempBuffer = new NativeArray<Color32>(request.GetData<Color32>(), Allocator.Persistent);

			Debug.Log(_buffer.Length);
			
		}
	}

}