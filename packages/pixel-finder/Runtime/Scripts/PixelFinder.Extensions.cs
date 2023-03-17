using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sasaki.Unity
{

	public static class ValueExtensions
	{
		public const uint MAX_VALUE = 16384;

		public const uint MAX_PIXELS_IN_VIEW = 2223114636;

		[Obsolete("Old max value for possible pixels in a view. Use MAX_VALUE", true)]
		public const uint MAX_VALUE_OLD = 1395882500;

		public static double[] NormalizeByCameraCount(uint[] values, int cameraCount)
		{
			if (values == null) return Array.Empty<double>();

			var res = new double[values.Length];

			for (var i = 0; i < values.Length; i++)
				res[i] = (double)values[i] / MAX_PIXELS_IN_VIEW / cameraCount;

			return res;
		}
	}

	public static class FinderExtensions
	{

		/// <summary>
		/// Shortcut for getting the Shader property ID of "_diffuseColor"
		/// </summary>
		public static int DiffuseColor => Shader.PropertyToID("_diffuseColor");

		public static bool TryGetDiffuseColor(this GameObject obj, out Color32 color)
		{
			color = default;

			if (obj != null && obj.GetComponent<MeshRenderer>() && obj.GetComponent<MeshRenderer>().material.HasProperty(DiffuseColor))
			{
				color = obj.GetComponent<MeshRenderer>().material.GetColor(DiffuseColor);
				return true;
			}

			return false;
		}

		/// <summary>
		///   Command for saving the finders texture to a png
		/// </summary>
		/// <param name="finder"></param>
		/// <param name="path"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static string SaveTextureAsPNG(this IPixelFinder finder, string path, string name) =>
			SasakiUtils.SaveTextureAsPNG(finder.ToTexture2D(), path, name);

		/// <summary>
		///   Command for saving finders texture to a png with some additional parameters
		/// </summary>
		/// <param name="finder"></param>
		/// <param name="path">directory to use</param>
		/// <param name="name">file name</param>
		/// <param name="format">format for the image</param>
		/// <param name="mipChain"></param>
		/// <param name="reSize">value to resize to</param>
		/// <returns></returns>
		public static string SaveTextureAsPNG(
			this IPixelFinder finder, string path, string name, TextureFormat format, bool mipChain = false,
			Vector2Int reSize = new()
		) => SasakiUtils.SaveTextureAsPNG(finder.ToTexture2D(format, mipChain, reSize), path, name);

		public static Texture2D ToTexture2D(
			this IPixelFinder finder, TextureFormat format = TextureFormat.RGB24, bool mipChain = false, Vector2Int reSize = new()
		)
		{
			var tex = new Texture2D(finder.Texture.width, finder.Texture.height, TextureFormat.RGB24, mipChain);

			RenderTexture.active = finder.Texture;
			tex.ReadPixels(new Rect(0, 0, finder.Texture.width, finder.Texture.height), 0, 0);
			tex.Apply();
			Object.Destroy(tex); //prevents memory leak

			return reSize == Vector2Int.zero ? tex : SasakiUtils.ScaleTexture(tex, reSize.x, reSize.y, FilterMode.Trilinear, format);
		}
	}
}