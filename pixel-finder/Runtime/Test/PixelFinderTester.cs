using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace Sasaki.Unity
{
	public class PixelFinderTester : MonoBehaviour
	{

		[SerializeField] FinderSystemType systemType;

		[SerializeField] GameObject frontObj, leftObj, rightObj, backObj;

		[SerializeField] RawImage frontImage, leftImage, rightImage, backImage;
		[SerializeField] RawImage frontImageJob, leftImageJob, rightImageJob, backImageJob;

		List<PixelFinder> pixelFinder;

		float m_UpdateTime = -1;

		static int DiffuseColor
		{
			get => Shader.PropertyToID("_diffuseColor");
		}

		void Start()
		{
			Test_UseSeparateColorsForEachShader();
		}

		Stopwatch timer;
		void Do()
		{
			timer = new Stopwatch();
			timer.Start();

			if (systemType == FinderSystemType.ComputeShader)
				foreach (var finder in pixelFinder)
					StartCoroutine(finder.Run());
			
		}

		void OnGUI()
		{
			if (GUI.Button(new Rect(10, 10, 50, 15), "Run"))
				Do();
		}

		private void Check()
		{
			if (systemType == FinderSystemType.ComputeShader)
			{
				foreach (var finder in pixelFinder)
				{
					if (!finder.isDone)
					{
						Debug.Log($"{finder.name} is not done");
						return;
					}
				}
				
				foreach (var finder in pixelFinder)
					Debug.Log(finder.data.Data[0][0]);
			}

			timer.Stop();

			Debug.Log($"Complete {systemType}: {timer.Elapsed}");
		}
		
		void Test_UseSeparateColorsForEachShader()
		{
			var parent = new GameObject("Shader");

			var front = new GameObject("Front-PixelFinder").AddComponent<PixelFinder>();
			front.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
			front.transform.SetParent(parent.transform);

			front.Init(frontObj.GetComponent<MeshRenderer>().material.GetColor(DiffuseColor), Check);
			frontImage.texture = front.texture;

			var left = new GameObject("Left-PixelFinder").AddComponent<PixelFinder>();
			left.transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));
			left.transform.SetParent(parent.transform);

			left.Init(leftObj.GetComponent<MeshRenderer>().material.GetColor(DiffuseColor), Check);
			leftImage.texture = left.texture;

			var right = new GameObject("Right-PixelFinder").AddComponent<PixelFinder>();
			right.transform.localRotation = Quaternion.Euler(new Vector3(0, -90, 0));
			right.transform.SetParent(parent.transform);

			right.Init(rightObj.GetComponent<MeshRenderer>().material.GetColor(DiffuseColor), Check);
			rightImage.texture = right.texture;

			var back = new GameObject("Back-PixelFinder").AddComponent<PixelFinder>();
			back.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
			back.transform.SetParent(parent.transform);

			back.Init(backObj.GetComponent<MeshRenderer>().material.GetColor(DiffuseColor), Check);
			backImage.texture = back.texture;

			pixelFinder = new List<PixelFinder>
			{
				front, left, right, back
			};
		}
	}
}