using UnityEngine;

namespace Sasaki.Unity
{
	public static class FinderExtension
	{
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
			var tex = new Texture2D(finder.texture.width, finder.texture.height, TextureFormat.RGB24, mipChain);

			RenderTexture.active = finder.texture;
			tex.ReadPixels(new Rect(0, 0, finder.texture.width, finder.texture.height), 0, 0);
			tex.Apply();
			Object.Destroy(tex); //prevents memory leak

			return reSize == Vector2Int.zero ? tex : SasakiUtils.ScaleTexture(tex, reSize.x, reSize.y, FilterMode.Trilinear, format);
		}
	}
}