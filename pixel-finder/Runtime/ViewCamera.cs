using System;
using UnityEngine;

namespace Sasaki.Unity
{
	[RequireComponent(typeof(Camera))]
	public class ViewCamera : MonoBehaviour
	{

		const int ViewSize = 512;

		[SerializeField] Camera _camera;

		[SerializeField] [Range(1, 16)]
		int depthBuffer = 16;

		[SerializeField] bool debugViewer;

		Action OnProcessEvent;

		public RenderTexture RenderText { get; private set; }

		public bool isRunning { get; set; }
		public bool jobDone { get; set; }

		public Camera Camera
		{
			get
			{
				if (_camera == null)
					_camera = gameObject.GetComponent<Camera>();
				return _camera;
			}
		}

		public float far
		{
			get => _camera.farClipPlane;
			private set => _camera.farClipPlane = value;
		}

		public float near
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

		// public RigStage SetLayerMask
		// {
		//   // set => cam.cullingMask = ViewToUtils.GetCullingMask(value);
		//   set => ;
		// }

		public Color ViewerBackground
		{
			set
			{
				if (_camera != null)
					_camera.backgroundColor = value;
			}
		}

		public float OrthoSize
		{
			set
			{
				if (_camera == null) return;

				if (!_camera.orthographic)
					_camera.orthographic = true;

				_camera.orthographicSize = value;
			}
		}

		void Awake()
		{
			// var ctrl = ViewToHub.Instance;
			// if (ctrl == null)
			//   return;
			//
			// ctrl.SetCameraDebugger += value => debugViewer = value;
			// debugViewer = ctrl.ShowDebugger;
		}

		void OnDisable()
		{
			SafeClean();
		}

		void OnDestroy()
		{
			SafeClean();
		}

		// NOTE this will only call when a camera is displayed
		void OnPostRender()
		{
			if (isRunning && !jobDone && RenderText != null) OnProcessEvent?.Invoke();
		}

		void OnPreRender()
		{
			if (_camera != null) _camera.targetTexture = RenderText;
		}

		void SafeClean()
		{
			if (RenderText != null)
				RenderText.Release();
		}

		public void Init(Action onProcess = null)
		{
			_camera = gameObject.GetComponent<Camera>();
			if (_camera == null)
				_camera = gameObject.AddComponent<Camera>();

			OnProcessEvent = onProcess;

			jobDone = false;
			far = 10000;

			RenderText = RenderTexture.GetTemporary(ViewSize, ViewSize, depthBuffer);
			RenderText.name = $"{gameObject.name}-CameraTexture";
			ViewerBackground = Color.black;

			_camera.clearFlags = CameraClearFlags.Color;
			_camera.aspect = 1;
			_camera.fieldOfView = 90f;
			_camera.backgroundColor = Color.black;
		}
	}

}