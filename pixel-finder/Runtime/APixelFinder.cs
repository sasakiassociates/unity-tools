using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Sasaki.Unity
{
	public abstract class APixelFinder : MonoBehaviour, IPixelFinder
	{
		[SerializeField] Texture2D _colorStrip;
		[SerializeField] Camera _camera;

		[SerializeField] [Range(1, 16)]
		int depthBuffer = 16;

		[SerializeField] [HideInInspector]
		internal int colorCount, cameraCount;

		[SerializeField, HideInInspector]
		internal int index;

		public PixelDataContainer data { get; protected set; }

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

		public RenderTexture texture { get; private set; }

		public int size
		{
			get => 512;
		}

		#region Unity Functions
		protected virtual void OnEnable()
		{
			SafeClean();
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
			if (texture != null)
				texture.Release();
		}
		#endregion

		public void Init(Color32 color, int collectionSize = 1, int cameraTotal = 6)
		{
			Init(new[] { color }, collectionSize);
		}

		public virtual void Init(Color32[] colors, int collectionSize = 1, int cameraTotal = 6)
		{
			if (colors == null || !colors.Any())
			{
				Debug.LogWarning("No Active Colors Ready");
				return;
			}

			colorStrip = colors.DrawPixelLine();
			colorCount = colors.Length;

			if (colorStrip == null)
			{
				Debug.LogError("Texture did not set");
				return;
			}

			data = new PixelDataContainer(collectionSize);

			_camera = gameObject.GetComponent<Camera>();
			if (_camera == null)
				_camera = gameObject.AddComponent<Camera>();

			cameraCount = cameraTotal;

			aspect = 1;
			fov = 90f;
			maxClipping = 10000;
			background = new Color32(0, 0, 0, 255);
			flags = CameraClearFlags.Color;

			texture = RenderTexture.GetTemporary(size, size, depthBuffer);
			texture.name = $"{gameObject.name}-CameraTexture";

			_camera.targetTexture = texture;
		}

		/// <summary>
		/// Render the camera and store the data into the container
		/// </summary>
		/// <param name="dataIndex">The index of where this data should be stored</param>
		public void RenderAndStore(int dataIndex)
		{
			index = dataIndex;
			Render();
			Store();
		}

		public virtual void Render(int dataIndex)
		{
			index = dataIndex;
			Render();
		}

		public virtual void Store(int dataIndex)
		{
			index = dataIndex;
			Store();
		}

		public abstract void Render();

		public abstract void Store();
	}
}