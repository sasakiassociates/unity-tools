using System.Threading.Tasks;
using UnityEngine;

namespace Sasaki.Unity
{

	public class PixelFinderSystemTester : MonoBehaviour
	{
		[SerializeField] private int pointCount;

		[SerializeField] GameObject frontObj, leftObj, rightObj, backObj;
		
		public APixelFinderSystem system;

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
				Run();
			
		}

		public void Run()
		{
			points = new Vector3[pointCount];

			for (int i = 0; i < pointCount; i++)
				points[i] = transform.position;

			system.Init(points, colors);

			system.onSystemRunComplete += arg0 => Debug.Log("Reporting back");
			
			system.Run();
		}

	}
}