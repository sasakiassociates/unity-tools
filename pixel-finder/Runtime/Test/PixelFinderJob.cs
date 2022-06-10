using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
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
		GPU

	}

	[RequireComponent(typeof(Camera))]
	public class PixelFinderJob : MonoBehaviour
	{

		const int textureSize = 512;
		Texture2D _texture;
		Color32[] _colors;

		[SerializeField] Camera _camera;

		static bool colorMatch(Color32 c, Color32 d)
		{
			return math.abs(c.r - d.r) < 0.01 && math.abs(c.g - d.g) < 0.01 && math.abs(c.b - d.b) < 0.01;
		}

		static float calcFrac(float pos, float r)
		{
			const float px = 1f / textureSize;
			float theta = math.atan2(pos + px, r) - math.atan2(pos, r);
			return theta * r / px;
		}

		[BurstCompile]
		struct SearchImageForColorsBurstJob : IJob
		{
			public NativeArray<Color32> imagePixels;

			public NativeArray<Color32> colorsToFind;

			public NativeArray<double> results;

			public void Execute()
			{
				var idx = 0;
				for (var y = 0; y < textureSize; ++y)
				{
					for (var x = 0; x < textureSize; ++x)
					{
						var pixel = imagePixels[idx++];
						for (int i = 0; i < colorsToFind.Length; i++)
						{
							if (colorMatch(pixel, colorsToFind[i]))
							{
								results[i] += 1;
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Test Methods for trying to copy pixel data from a render texture into a texture 2d.
		/// NOTE: This does not copy the texture properly into the array 
		/// </summary>
		/// <returns></returns>
		private NativeArray<Color32> Test_LoadPixelsIntoTexture()
		{
			// Copy the texture into a dummy texture
			Graphics.CopyTexture(texture, _texture);
			_texture.Apply();
			return _texture.GetRawTextureData<Color32>();
		}

		/// <summary>
		/// Test Methods for trying to copy pixel data from a render texture into a texture 2d.
		/// NOTE: Doesn't seem to load well with multiple objects calling this 
		/// </summary>
		/// <returns></returns>
		private NativeArray<Color32> Test_GetPixelsFromGPU()
		{
			var data = new NativeArray<Color32>(textureSize *textureSize, Allocator.Persistent);
			// Async call for GPU call, but this doesn't play well with jobs
			AsyncGPUReadback.RequestIntoNativeArray(ref data, texture);
			_texture.LoadRawTextureData(data);
			_texture.Apply();
			return _texture.GetRawTextureData<Color32>();
		}

		[BurstCompile]
		struct SearchImageForColorsBurstJobParallel : IJobParallelFor
		{
			[NativeDisableParallelForRestriction]
			public NativeArray<Color32> imagePixels;

			[NativeDisableParallelForRestriction]
			public NativeArray<Color32> colorsToFind;

			[NativeDisableParallelForRestriction]
			public NativeArray<double> results;

			public void Execute(int y)
			{
				var idx = y * textureSize;
				for (var x = 0; x < textureSize; ++x)
				{
					var pixel = imagePixels[idx++];

					for (int i = 0; i < colorsToFind.Length; i++)
					{
						if (colorMatch(pixel, colorsToFind[i]))
						{
							results[i] += 1;
						}
					}

					for (var i = 0; i < colorsToFind.Length; i++)
					{
						if (!colorMatch(colorsToFind[i], pixel))
							continue;

						results[i] += 1;

						// results[idx] =
						// 	var fracX = calcFrac(idx / (textureSize - 0.5f), 0.5f);
						// var fracY = calcFrac(y / (textureSize - 0.5f), 0.5f);

						// results[idx++] = math.floor(16384 * math.pow(math.abs(fracX * fracY), math.sqrt(2))) / 2223114636 / 6;
					}
				}
			}
		}

		// NativeArray<Color32> _memoryColors;

		public void RenderBurstParallel()
		{
			_camera.Render();

			var data = Test_LoadPixelsIntoTexture();
			// var data2 = Test_GetPixelsFromGPU();

			var nativeColors = new NativeArray<Color32>(_colors, Allocator.TempJob);
			var res = new NativeArray<double>(_colors.Length, Allocator.TempJob);

			var job = new SearchImageForColorsBurstJobParallel()
			{
				imagePixels = data,
				colorsToFind = nativeColors,
				results = res
			};

			job.Schedule(textureSize, 1).Complete();

			results = new double[job.results.Length];
			job.results.CopyTo(results);

			res.Dispose();
			nativeColors.Dispose();
		}

		public void RenderBurst()
		{
			_camera.Render();

			var data = Test_LoadPixelsIntoTexture();

			// var data2 = Test_GetPixelsFromGPU();
			// Test_LoadPixelsIntoTexture();
			var nativeColors = new NativeArray<Color32>(_colors, Allocator.TempJob);
			var res = new NativeArray<double>(_colors.Length, Allocator.TempJob);

			var job = new SearchImageForColorsBurstJob()
			{
				imagePixels = data,
				colorsToFind = nativeColors,
				results = res
			};

			job.Schedule().Complete();

			results = new double[job.results.Length];
			job.results.CopyTo(results);

			res.Dispose();
			nativeColors.Dispose();
		}

		public RenderTexture texture { get; private set; }

		NativeArray<Color32> _buffer;
		[SerializeField] double[] results;

		void OnDisable()
		{
			DestroyImmediate(_texture);
			if (texture != null)
				texture.Release();
		}

		public void Init(Color32 colors)
		{
			_colors = new[] { colors };
			//
			// _buffer = new NativeArray<Color32>(textureSize * textureSize, Allocator.Persistent,
			//                                    NativeArrayOptions.UninitializedMemory);
			// _memoryColors = new NativeArray<Color32>(textureSize * textureSize, Allocator.Persistent);

			_camera = GetComponent<Camera>();
			_camera.clearFlags = CameraClearFlags.Color;
			_camera.aspect = 1;
			_camera.fieldOfView = 90f;
			_camera.farClipPlane = 10000;
			_camera.backgroundColor = new Color32(0, 0, 0, 255);

			texture = RenderTexture.GetTemporary(textureSize, textureSize, 16, RenderTextureFormat.ARGB32);
			texture.name = $"{gameObject.name}-CameraTexture";

			_camera.targetTexture = texture;

			_texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false, true);
			var tempColors = new List<Color32>();
			for (int i = 0; i < textureSize * textureSize; i++)
				tempColors.Add(new Color32(0, 0, 0, 255));

			_texture.SetPixels32(tempColors.ToArray());
			_texture.Apply();
		}
	}
}