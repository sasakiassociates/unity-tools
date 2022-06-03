using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Sasaki.Unity
{

	public enum FinderSystemType
	{
		ComputeShader,
		BurstParallel

	}

	[RequireComponent(typeof(Camera))]
	public class PixelFinderJob : MonoBehaviour
	{

		int m_TextureSize = 512;
		Texture2D m_Texture;
		Color32[] m_Colors;

		[SerializeField] Camera _camera;

		static bool colorMatch(Color32 c, Color32 d)
		{
			return math.abs(c.r - d.r) < 0.01 && math.abs(c.g - d.g) < 0.01 && math.abs(c.b - d.b) < 0.01;
		}

		static float calcFrac(float pos, float r)
		{
			const float px = 1 / (float)512;
			float theta = math.atan2(pos + px, r) - math.atan2(pos, r);
			return theta * r / px;
		}

		[BurstCompile]
		struct CalcPlasmaIntoNativeArrayBurstParallel : IJobParallelFor
		{
			// Our job accesses does not just access one element of
			// this array that maps to the job index --
			// we compute whole row of pixels in one job invocation. Thus have to
			// tell the job safety system to stop checking that array
			// accesses map to job index on this array, via
			// the NativeDisableParallelForRestriction attribute.
			[NativeDisableParallelForRestriction]
			public NativeArray<Color32> textureColors;

			public int textureSize;

			[NativeDisableParallelForRestriction]
			public NativeArray<Color32> colors;

			[NativeDisableParallelForRestriction]
			public NativeArray<double> results;

			public void Execute(int y)
			{
				var idx = y * textureSize;
				for (var x = 0; x < textureSize; ++x)
				{
					var pixel = textureColors[idx];

					foreach (var c in colors)
					{
						if (!colorMatch(c, pixel))
							continue;

						var fracX = calcFrac(idx / (512 - 0.5f), 0.5f);
						var fracY = calcFrac(y / (512 - 0.5f), 0.5f);

						results[idx++] = math.floor(16384 * math.pow(math.abs(fracX * fracY), math.sqrt(2))) / 2223114636 / 6;
					}
				}
			}
		}

		public void RenderBurst()
		{
			Graphics.CopyTexture(texture, 0, 0, 0, 0, m_TextureSize, m_TextureSize, m_Texture, 0, 0, 0, 0);

			var data = m_Texture.GetPixelData<Color32>(0);
			var nativeColors = new NativeArray<Color32>(m_Colors, Allocator.TempJob);
			var res = new NativeArray<double>(data.Length, Allocator.TempJob);

			var job = new CalcPlasmaIntoNativeArrayBurstParallel()
			{
				textureColors = data,
				textureSize = m_TextureSize,
				colors = nativeColors,
				results = res
			};

			job.Schedule(m_TextureSize, 1).Complete();

			// results = new List<double>();
			// foreach (var r in res)
			// 	if (r > 0)
			// 		results.Add(r);

			res.Dispose();
			nativeColors.Dispose();

			Debug.Log("Parallel Job Complete");
		}

		public RenderTexture texture { get; private set; }

		[SerializeField] private List<double> results;

		void OnDisable()
		{
			DestroyImmediate(m_Texture);
			if (texture != null)
				texture.Release();
		}

		public void Init(Color32 getColor)
		{
			m_Colors = new[] { getColor };
			_camera = GetComponent<Camera>();

			_camera.clearFlags = CameraClearFlags.Color;

			texture = RenderTexture.GetTemporary(512, 512, 16);
			texture.name = $"{gameObject.name}-CameraTexture";

			_camera.targetTexture = texture;

			m_Texture = new Texture2D(m_TextureSize, m_TextureSize, TextureFormat.RGBA32, false);
		}
	}
}