using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sasaki.Unity
{
	public class PixelFinderSetup : MonoBehaviour
	{

		[SerializeField] FinderSystemType systemType;
		
		[SerializeField] GameObject frontObj, leftObj, rightObj, backObj;

		[SerializeField] RawImage frontImage, leftImage, rightImage, backImage;

		List<PixelFinder> pixelFinder;
		List<PixelFinderJob> pixelFinderJobs;

		float m_UpdateTime = -1;

		RenderTexture Texture { get; set; }

		static int DiffuseColor
		{
			get => Shader.PropertyToID("_diffuseColor");
		}

		void Start()
		{
			Test_UseSeparateColorsForEachShader(); 
			Test_UseBurst();
		}

		void Update()
		{
			var t0 = Time.realtimeSinceStartup;

			switch (systemType)
			{
				case FinderSystemType.ComputeShader:
					foreach (var finder in pixelFinder)
						finder.Render();
					break;
				case FinderSystemType.BurstParallel:
					foreach (var j in pixelFinderJobs)					
						j.RenderBurst();
					break;
			}

			var t1 = Time.realtimeSinceStartup;
			var dt = t1 - t0;

			// Update "time it took" UI indicator
			m_UpdateTime = m_UpdateTime < 0 ? dt : Mathf.Lerp(m_UpdateTime, dt, 0.3f);

			Debug.Log($"Complete {systemType}: {m_UpdateTime * 1000.0f:F2}ms");
		}

		[SerializeField] Color32[] frontColors;

		void Test_UseBurst()
		{
			var front = new GameObject("Front-PixelFinderJob").AddComponent<PixelFinderJob>();
			front.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
			front.transform.SetParent(transform);

			front.Init(frontObj.GetComponent<MeshRenderer>().material.GetColor(DiffuseColor));
			frontImage.texture = front.texture;

			var left = new GameObject("Left-PixelFinderJob").AddComponent<PixelFinderJob>();
			left.transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));
			left.transform.SetParent(transform);

			left.Init(leftObj.GetComponent<MeshRenderer>().material.GetColor(DiffuseColor));
			leftImage.texture = left.texture;

			var right = new GameObject("Right-PixelFinderJob").AddComponent<PixelFinderJob>();
			right.transform.localRotation = Quaternion.Euler(new Vector3(0, -90, 0));
			right.transform.SetParent(transform);

			right.Init(rightObj.GetComponent<MeshRenderer>().material.GetColor(DiffuseColor));
			rightImage.texture = right.texture;

			var back = new GameObject("Back-PixelFinderJob").AddComponent<PixelFinderJob>();
			back.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
			back.transform.SetParent(transform);

			back.Init(backObj.GetComponent<MeshRenderer>().material.GetColor(DiffuseColor));
			backImage.texture = back.texture;

			pixelFinderJobs = new List<PixelFinderJob>()
			{
				front, left, right, back
			};
		}

		void Test_UseSeparateColorsForEachShader()
		{
			var front = new GameObject("Front-PixelFinder").AddComponent<PixelFinder>();
			front.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
			front.transform.SetParent(transform);

			front.Init(frontObj.GetComponent<MeshRenderer>().material.GetColor(DiffuseColor));
			frontImage.texture = front.texture;

			var left = new GameObject("Left-PixelFinder").AddComponent<PixelFinder>();
			left.transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));
			left.transform.SetParent(transform);

			left.Init(leftObj.GetComponent<MeshRenderer>().material.GetColor(DiffuseColor));
			leftImage.texture = left.texture;

			var right = new GameObject("Right-PixelFinder").AddComponent<PixelFinder>();
			right.transform.localRotation = Quaternion.Euler(new Vector3(0, -90, 0));
			right.transform.SetParent(transform);

			right.Init(rightObj.GetComponent<MeshRenderer>().material.GetColor(DiffuseColor));
			rightImage.texture = right.texture;

			var back = new GameObject("Back-PixelFinder").AddComponent<PixelFinder>();
			back.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
			back.transform.SetParent(transform);

			back.Init(backObj.GetComponent<MeshRenderer>().material.GetColor(DiffuseColor));
			backImage.texture = back.texture;

			pixelFinder = new List<PixelFinder>
			{
				front, left, right, back
			};
		}
	}
}