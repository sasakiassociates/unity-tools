using System;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Sasaki.Unity
{

	[RequireComponent(typeof(Camera))]
	public class PixelFinderGPU : APixelFinder
	{
		[SerializeField] uint[] histogramData;

		[SerializeField] bool debugViewer;

		[SerializeField] ComputeShader pixelShader;

		ComputeBuffer _histogramBuffer;

		public override void Init(Color32[] inputColors, Action onDone, int collectionSize = 1, int cameraTotal = 6)
		{
			base.Init(inputColors, onDone, collectionSize, cameraTotal);

			TryLoadShader();

			SetKernels();
		}

		protected override void SafeClean()
		{
			base.SafeClean();
			_histogramBuffer?.Dispose();
		}

		protected override void OnCompleteReadback(AsyncGPUReadbackRequest request)
		{
			data.Set(Process(), _index);
		}

		void SetKernels()
		{
			CreateBuffers();

			_kernInitialize = pixelShader.FindKernel(PixelFinderInit);
			pixelShader.SetBuffer(_kernInitialize, PixelCountBuffer, _histogramBuffer);

			_kernMain = pixelShader.FindKernel(PixelFinderMain);
			pixelShader.SetBuffer(_kernMain, PixelCountBuffer, _histogramBuffer);
			pixelShader.SetTexture(_kernMain, ColorArrayTexture, colorStrip);
			pixelShader.SetInt(InputTextureSize, 512);

			if (_kernInitialize < 0 || _kernMain < 0 || null == _histogramBuffer || null == histogramData)
				isReady = false;
			else
				isReady = true;
		}

		void CreateBuffers()
		{
			_histogramBuffer?.Dispose();

			_histogramBuffer = new ComputeBuffer(colorCount, sizeof(uint));

			histogramData = new uint[colorCount];

			pixelShader.SetInt(ColorArraySize, colorCount);
		}

		void TryLoadShader()
		{
			if (pixelShader == null)
			{
				#if UNITY_EDITOR
				pixelShader =
					Instantiate(AssetDatabase.LoadAssetAtPath<ComputeShader>("Packages/com.sasaki.pixelfinder/Runtime/Shader/PixelFinder.compute"));
				#endif

				if (pixelShader == null)
				{
					Debug.LogWarning("No Active Shader Found");
					return;
				}
			}
		}

		double[] Process()
		{
			if (_histogramBuffer.count != colorCount)
			{
				Debug.Log("Updating Buffer");
				CreateBuffers();
			}

			pixelShader.SetBuffer(_kernInitialize, PixelCountBuffer, _histogramBuffer);
			pixelShader.SetBuffer(_kernMain, PixelCountBuffer, _histogramBuffer);
			pixelShader.SetTexture(_kernMain, InputTexture, texture);
			pixelShader.Dispatch(_kernInitialize, 256 / 64, 1, 1);
			pixelShader.Dispatch(_kernMain, (texture.width + 7) / 8, (texture.height + 7) / 8, 1);

			// NOTE: Performance impact with snagging the GPU data
			_histogramBuffer.GetData(histogramData);

			var res = new double[histogramData.Length];

			for (var i = 0; i < histogramData.Length; i++)
				res[i] = (double)histogramData[i] / PIXELS_IN_VIEW / cameraCount;

			return res;
		}

		//Old Value for Max 1395882500
		/// <summary>
		///   Max Pixel Count in a single viewer as measured :D
		/// </summary>
		/// 
		public const uint PIXELS_IN_VIEW = 2223114636;

		#region Shader Parameters
		const string PixelFinderInit = "PixelFinderInitialize";

		const string PixelFinderMain = "PixelFinderMain";

		const string InputTexture = "InputTexture";

		const string InputTextureSize = "InputTextureSize";

		const string ColorArraySize = "ColorArraySize";

		const string ColorArrayTexture = "ColorArrayTexture";

		const string PixelCountBuffer = "PixelCountBuffer";

		int _kernMain, _kernInitialize;
		#endregion

	}

}