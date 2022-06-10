using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sasaki.Unity
{
	public class PixelFinderTester : MonoBehaviour
	{

		[SerializeField] FinderSystemType systemType;

		[SerializeField] GameObject frontObj, leftObj, rightObj, backObj;

		[SerializeField] RawImage frontImage, leftImage, rightImage, backImage;
		[SerializeField] RawImage frontImageJob, leftImageJob, rightImageJob, backImageJob;

		List<PixelFinderGPU> pixelFinder;
		List<PixelFinderJob> pixelFinderJobs;
		[SerializeField] SealedFinder _sealedFinder;

		float m_UpdateTime = -1;

		static int DiffuseColor
		{
			get => Shader.PropertyToID("_diffuseColor");
		}

		void Start()
		{
			Test_UseSeparateColorsForEachShader();
			Test_UseBurst();
			_sealedFinder.Init(frontObj.GetComponent<MeshRenderer>().material.GetColor(DiffuseColor));
		}

		void Do()
		{
			var t0 = Time.realtimeSinceStartup;

			switch (systemType)
			{
				case FinderSystemType.ComputeShader:
					foreach (var finder in pixelFinder)
						StartCoroutine(finder.Run());
					break;
				case FinderSystemType.Burst:
					foreach (var j in pixelFinderJobs)
						j.RenderBurst();
					break;
				case FinderSystemType.BurstParallel:
					foreach (var j in pixelFinderJobs)
						j.RenderBurstParallel();
					break;
				case FinderSystemType.GPU:
					StartCoroutine(_sealedFinder.Run());
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			var t1 = Time.realtimeSinceStartup;
			var dt = t1 - t0;

			m_UpdateTime = m_UpdateTime < 0 ? dt : Mathf.Lerp(m_UpdateTime, dt, 0.3f);

			Debug.Log($"Complete {systemType}: {m_UpdateTime * 1000.0f:F2}ms");
		}

		void OnGUI()
		{
			if (GUI.Button(new Rect(10, 10, 50, 15), "Run"))
				Do();
		}

		void Test_UseBurst()
		{
			var parent = new GameObject("Jobs");

			var front = new GameObject("Front-PixelFinderJob").AddComponent<PixelFinderJob>();
			front.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
			front.transform.SetParent(parent.transform);

			front.Init(frontObj.GetComponent<MeshRenderer>().material.GetColor(DiffuseColor));
			frontImageJob.texture = front.texture;

			var left = new GameObject("Left-PixelFinderJob").AddComponent<PixelFinderJob>();
			left.transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));
			left.transform.SetParent(parent.transform);

			left.Init(leftObj.GetComponent<MeshRenderer>().material.GetColor(DiffuseColor));
			leftImageJob.texture = left.texture;

			var right = new GameObject("Right-PixelFinderJob").AddComponent<PixelFinderJob>();
			right.transform.localRotation = Quaternion.Euler(new Vector3(0, -90, 0));
			right.transform.SetParent(parent.transform);

			right.Init(rightObj.GetComponent<MeshRenderer>().material.GetColor(DiffuseColor));
			rightImageJob.texture = right.texture;

			var back = new GameObject("Back-PixelFinderJob").AddComponent<PixelFinderJob>();
			back.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
			back.transform.SetParent(parent.transform);

			back.Init(backObj.GetComponent<MeshRenderer>().material.GetColor(DiffuseColor));
			backImageJob.texture = back.texture;

			pixelFinderJobs = new List<PixelFinderJob>()
			{
				front, left, right, back
			};
		}

		void Test_UseSeparateColorsForEachShader()
		{
			var parent = new GameObject("Shader");

			var front = new GameObject("Front-PixelFinder").AddComponent<PixelFinderGPU>();
			front.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
			front.transform.SetParent(parent.transform);

			front.Init(frontObj.GetComponent<MeshRenderer>().material.GetColor(DiffuseColor));
			frontImage.texture = front.texture;

			var left = new GameObject("Left-PixelFinder").AddComponent<PixelFinderGPU>();
			left.transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));
			left.transform.SetParent(parent.transform);

			left.Init(leftObj.GetComponent<MeshRenderer>().material.GetColor(DiffuseColor));
			leftImage.texture = left.texture;

			var right = new GameObject("Right-PixelFinder").AddComponent<PixelFinderGPU>();
			right.transform.localRotation = Quaternion.Euler(new Vector3(0, -90, 0));
			right.transform.SetParent(parent.transform);

			right.Init(rightObj.GetComponent<MeshRenderer>().material.GetColor(DiffuseColor));
			rightImage.texture = right.texture;

			var back = new GameObject("Back-PixelFinder").AddComponent<PixelFinderGPU>();
			back.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
			back.transform.SetParent(parent.transform);

			back.Init(backObj.GetComponent<MeshRenderer>().material.GetColor(DiffuseColor));
			backImage.texture = back.texture;

			pixelFinder = new List<PixelFinderGPU>
			{
				front, left, right, back
			};
		}
	}
}