using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Sasaki.Unity
{
	public enum FinderSystemType
	{
		ComputeShader,
		Burst,
		BurstParallel,
	}

	[RequireComponent(typeof(Camera))]
	public class PixelFinderJob : APixelFinder
	{

		const int textureSize = 512;
		Texture2D _texture;

		static bool colorMatch(Color32 c, Color32 d)
		{
			return math.abs(c.r - d.r) < 0.01 && math.abs(c.g - d.g) < 0.01 && math.abs(c.b - d.b) < 0.01;
		}

		static double calcFrac(float pos, float r)
		{
			const double px = 1.0 / textureSize;
			double theta = math.atan2(pos + px, r) - math.atan2(pos, r);
			return theta * r / px;
		}
		static double calcPixelPos(int x, int y)
		{
			var fracX = calcFrac(x / (float)textureSize - 0.5f, 0.5f);
			var fracY = calcFrac(y / (float)textureSize - 0.5f, 0.5f);
			var overall = math.floor(MAX_VALUE * math.pow(math.abs(fracX * fracY), math.sqrt(2)));
			return overall;
		}

		[BurstCompile]
		struct SearchImageForColorsBurstJob : IJob
		{
			[ReadOnly] public NativeArray<Color32> imagePixels;

			[ReadOnly] public NativeArray<Color32> colorsToFind;

			public NativeArray<double> results;

			public void Execute()
			{
				var idx = 0;
				for (var y = 0; y < textureSize; y++)
				{
					for (var x = 0; x < textureSize; x++)
					{
						var pixel = imagePixels[idx++];
						for (int i = 0; i < colorsToFind.Length; i++)
						{
							if (!colorMatch(pixel, colorsToFind[i]))
								continue;

							results[i] += calcPixelPos(x, y);
						}
					}
				}
			}
		}

		[BurstCompile]
		struct SearchImageForColorsBurstJobParallel : IJobParallelFor
		{
			[ReadOnly] public NativeArray<Color32> imagePixels;

			[ReadOnly] public NativeArray<Color32> colorsToFind;

			// [NativeDisableParallelForRestriction]
			[NativeDisableContainerSafetyRestriction]
			public NativeArray<double> results;

			public void Execute(int y)
			{
				// Each execute is called from 0 to textureSize (512)
				for (var x = 0; x < textureSize; x++)
				{
					// Each job loops through the row of pixels 
					var pixel = imagePixels[x + y];

					for (int i = 0; i < colorsToFind.Length; i++)
					{
						if (!colorMatch(pixel, colorsToFind[i]))
							continue;

						results[i] += calcPixelPos(x, y);
					}
				}
			}
		}

		public bool usePar;

		[SerializeField] double[] results;

		protected override void OnCompleteReadback(AsyncGPUReadbackRequest request)
		{
			var rawPixelData = new NativeArray<Color32>(request.GetData<Color32>(), Allocator.TempJob);
			var countedColorResults = new NativeArray<double>(colorCount, Allocator.TempJob);
			var colorsToFind = new NativeArray<Color32>(colors, Allocator.TempJob);

			if (!usePar)
			{
				var job = new SearchImageForColorsBurstJob()
				{
					imagePixels = rawPixelData,
					colorsToFind = colorsToFind,
					results = countedColorResults
				};
				job.Schedule().Complete();
				results = new double[job.results.Length];
				job.results.CopyTo(results);
			}
			else
			{
				var job = new SearchImageForColorsBurstJobParallel()
				{
					imagePixels = rawPixelData,
					colorsToFind = colorsToFind,
					results = countedColorResults
				};
				job.Schedule(textureSize, 64).Complete();
				results = new double[job.results.Length];
				job.results.CopyTo(results);
			}

			data.Set(results, _index);

			rawPixelData.Dispose();
			countedColorResults.Dispose();
			colorsToFind.Dispose();
		}

		public override void Init(Color32[] inputColors, Action onDone, int collectionSize = 1, int cameraTotal = 6)
		{
			base.Init(inputColors, onDone, collectionSize, cameraTotal);

			_texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false, true);
			var tempColors = new List<Color32>();

			for (int x = 0; x < textureSize; x++)
			{
				for (int y = 0; y < textureSize; y++)
				{
					var r = math.lerp(0, 1, (float)x / textureSize);
					var g = math.lerp(0, 1, (float)y / textureSize);
					var b = math.lerp(0, 1, (float)(x * y) / (textureSize * textureSize));

					tempColors.Add(new Color(r, g, b, 1));
				}
			}

			_texture.SetPixels32(tempColors.ToArray());
			_texture.Apply();

			return;

			var pix = _texture.GetPixels();

			for (int x = 0; x < textureSize; x++)
			{
				for (int y = 0; y < textureSize; y++)
				{
					var index = x + y;
					if (pix.Length > index && _texture.width > x && _texture.height > y)
					{
						if (!pix[index].Equals(_texture.GetPixel(x, y)))
							Debug.Log("MisMatch");
					}
				}
			}
		}
	}
}