using System;
using UnityEngine;

namespace Sasaki.Unity
{

	public class ViewProcessor : MonoBehaviour
	{

		//Old Value for Max 1395882500

    /// <summary>
    ///   Max Pixel Count in a single viewer as measured :D
    /// </summary>
    public const uint PIXELS_IN_VIEW = 2223114636;

		[Header("|| Runtime Creations ||")] [SerializeField]
		Texture2D viewTexture;

		[SerializeField] uint[] histogramData;

		[SerializeField] [HideInInspector]
		int colorCount;

		public ComputeShader pixelShader;

		bool _counterReady;

		ComputeBuffer _histogramBuffer;

		public Action<double[]> OnDataReady;

		public bool isRunning { get; set; }

		void Awake()
		{
			// if (pixelShader == null && ViewToHub.Instance != null)
			//   pixelShader ??= Instantiate(ViewToHub.ProcessorShader);
		}

		void OnEnable()
		{
			_histogramBuffer?.Dispose();
		}

		void OnDisable()
		{
			_histogramBuffer?.Dispose();
		}

		void OnDestroy()
		{
			_histogramBuffer?.Dispose();
		}

		void SetKernels()
		{
			if (viewTexture == null)
			{
				Debug.LogError($"texture on {this} is not ready yet");
				return;
			}

			CreateBuffers();

			_kernInitialize = pixelShader.FindKernel(PixelFinderInit);
			pixelShader.SetBuffer(_kernInitialize, PixelCountBuffer, _histogramBuffer);

			_kernMain = pixelShader.FindKernel(PixelFinder);
			pixelShader.SetBuffer(_kernMain, PixelCountBuffer, _histogramBuffer);
			pixelShader.SetTexture(_kernMain, ColorArrayTexture, viewTexture);

			pixelShader.SetInt(InputTextureSize, 512);

			if (_kernInitialize < 0 || _kernMain < 0 || null == _histogramBuffer || null == histogramData)
				_counterReady = false;
			else
				_counterReady = true;
		}

		void CreateBuffers()
		{
			// Debug.Log( "Creating new buffer" );
			_histogramBuffer?.Dispose();

			_histogramBuffer = new ComputeBuffer(colorCount, sizeof(uint));
			histogramData = new uint[colorCount];

			pixelShader.SetInt(ColorArraySize, colorCount);
		}

		public void Init(Color32[] colors)
		{
			if (viewTexture != null)
				Destroy(viewTexture);

			if (colors != null && colors.Length > 0)
			{
				viewTexture = colors.DrawPixelLine();

				if (viewTexture == null)
				{
					Debug.LogError("View Texture did not set");
				}
				else
				{
					colorCount = colors.Length;
					SetKernels();
				}
			}
			else
			{
				Debug.LogError($"Could not create texture for {gameObject.name} ");
			}
		}

		public void Init(Color32[] colors, Action<double[]> onDataReady)
		{
			OnDataReady = onDataReady;
			Init(colors);
		}

		public void Process(RenderTexture source)
		{
			if (_counterReady && _histogramBuffer != null)
			{
				if (_histogramBuffer.count != colorCount)
				{
					Debug.Log("Updating Buffer");
					CreateBuffers();
				}

				pixelShader.SetBuffer(_kernInitialize, PixelCountBuffer, _histogramBuffer);
				pixelShader.SetBuffer(_kernMain, PixelCountBuffer, _histogramBuffer);
				pixelShader.SetTexture(_kernMain, InputTexture, source);

				pixelShader.Dispatch(_kernInitialize, 256 / 64, 1, 1);
				pixelShader.Dispatch(_kernMain, (source.width + 7) / 8, (source.height + 7) / 8, 1);

				// NOTE performance impact 
				if (isRunning)
				{
					_histogramBuffer.GetData(histogramData);

					var data = new double[histogramData.Length];
					for (var i = 0; i < histogramData.Length; i++)
						data[i] = (double)histogramData[i] / PIXELS_IN_VIEW / 6.0;

					OnDataReady?.Invoke(data);
				}
			}
		}

		#region Shader Parameters
		const string PixelFinderInit = "PixelFinderInitialize";

		const string PixelFinder = "PixelFinderMain";

		const string InputTexture = "InputTexture";

		const string InputTextureSize = "InputTextureSize";

		const string ColorArraySize = "ColorArraySize";

		const string ColorArrayTexture = "ColorArrayTexture";

		const string PixelCountBuffer = "PixelCountBuffer";

		int _kernMain;

		int _kernInitialize;
		#endregion

	}
}