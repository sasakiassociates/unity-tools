using UnityEngine;

namespace Sasaki.Unity
{
	public class PixelFinderSystemTester : MonoBehaviour
	{
		[SerializeField] int pointCount;

		[SerializeField] GameObject frontObj, leftObj, rightObj, backObj;

		public PixelLayout layout;

		Vector3[] points;

		static int DiffuseColor
		{
			get => Shader.PropertyToID("_diffuseColor");
		}

		Color32[] colors
		{
			get =>
				new Color32[]
				{
					frontObj.GetComponent<MeshRenderer>().material.GetColor(DiffuseColor),
					leftObj.GetComponent<MeshRenderer>().material.GetColor(DiffuseColor),
					rightObj.GetComponent<MeshRenderer>().material.GetColor(DiffuseColor),
					backObj.GetComponent<MeshRenderer>().material.GetColor(DiffuseColor)
				};
		}

		void OnGUI()
		{
			if (GUI.Button(new Rect(10, 10, 50, 15), "Run"))
				Run();
		}

		public void Run()
		{
			points = new Vector3[pointCount];

			for (var i = 0; i < pointCount; i++)
				points[i] = transform.position;

			layout.Init(points.Length, colors);

			layout.OnComplete += () => Debug.Log("Reporting back");

			layout.Run(0);
		}
	}
}