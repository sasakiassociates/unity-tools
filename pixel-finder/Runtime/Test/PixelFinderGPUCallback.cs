using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Sasaki.Unity
{
	public class PixelFinderGPUCallback : APixelFinder
	{

		// NativeArray<Color32> _buffer;
		// // (RenderTexture grab, RenderTexture flip, RenderTexture cam) _rt;
		// // // (RenderTexture grab, RenderTexture flip) _rt;
		// //
		//
		// RenderTexture main;
		//
		// public IEnumerator Run()
		// {
		// 	_buffer = new NativeArray<Color32>(size * size,
		// 	                                   Allocator.Persistent,
		// 	                                   NativeArrayOptions.UninitializedMemory);
		//
		// 	main = new RenderTexture(size, size, 0);
		//
		// 	yield return new WaitForEndOfFrame();
		//
		// 	Graphics.Blit(texture, main);
		//
		// 	AsyncGPUReadback.RequestIntoNativeArray
		// 		(ref _buffer, main, 0, OnCompleteReadback);
		//
		// 	Store();
		// }


		protected override void OnCompleteReadback(AsyncGPUReadbackRequest request)
		{
			if (request.hasError)
			{
				Debug.Log("GPU readback error detected.");
				return;
			}

			var newTex = new Texture2D
			(
				Screen.width,
				Screen.height,
				TextureFormat.RGBA32,
				false
			);

			try
			{
				var da = request.GetData<Color32>();
				Debug.Log(da);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}

			newTex.LoadRawTextureData(request.GetData<uint>());
			newTex.Apply();

			var d = newTex.GetRawTextureData<Color32>();

			System.IO.File.WriteAllBytes(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "test.png"),
			                             ImageConversion.EncodeToPNG(newTex));
			Debug.Log("Request Complete");
		}

	

	}
}