using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Sasaki.Unity
{
	public enum SystemType
	{
		Coroutines,
		Task,
		Update,
		Sync
	}

	public class PixelFinderSystemTester : MonoBehaviour
	{
		[SerializeField] private int pointCount;

		// [SerializeField] bool useSync;
		[SerializeField] SystemType systemType;
		[SerializeField] PixelFinderGPUCallback _gpuCallback;

		[SerializeField] GameObject frontObj, leftObj, rightObj, backObj;

		// [SerializeField] RawImage frontImage, leftImage, rightImage, backImage;

		public PixelFinderSystem system;

		Vector3[] points;

		static int DiffuseColor
		{
			get => Shader.PropertyToID("_diffuseColor");
		}

		private Color32[] colors =>
			new Color32[]
			{
				frontObj.GetComponent<MeshRenderer>().material.GetColor(DiffuseColor),
				leftObj.GetComponent<MeshRenderer>().material.GetColor(DiffuseColor),
				rightObj.GetComponent<MeshRenderer>().material.GetColor(DiffuseColor),
				backObj.GetComponent<MeshRenderer>().material.GetColor(DiffuseColor),
			};

		void OnGUI()
		{
			if (GUI.Button(new Rect(10, 10, 50, 15), "Run"))
				Run().Forget();

			if (GUI.Button(new Rect(10, 50, 50, 15), "Render"))
			{
				StartCoroutine(_gpuCallback.Run());
			}
		}

		public async UniTaskVoid Run()
		{
			Debug.Log($"Starting System Test with {systemType}");
			points = new Vector3[pointCount];

			for (int i = 0; i < pointCount; i++)
				points[i] = transform.position;

			system.Create(points, colors);
			_gpuCallback.Init(colors, points.Length);


			system.OnSystemDataComplete += arg0 => Debug.Log("Reporting back");

			switch (systemType)
			{
				case SystemType.Coroutines:
					StartCoroutine(system.RunCoroutine());
					break;
				case SystemType.Task:
					await system.RunTask();
					break;
				case SystemType.Sync:
					system.RunSync();
					break;
				case SystemType.Update:
					break;
			}
		}

	}
}