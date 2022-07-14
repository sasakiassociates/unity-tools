using System.IO;
using UnityEngine;

namespace Sasaki
{
	public static partial class SasakiUtils
	{
		/// <summary>
		///   Returns a scaled copy of given texture.
		/// </summary>
		/// <param name="src">Source texture to scale</param>
		/// <param name="width">Destination texture width</param>
		/// <param name="height">Destination texture height</param>
		/// <param name="mode">Filtering mode</param>
		/// <param name="format"></param>
		public static Texture2D ScaleTexture(
			Texture2D src, int width, int height, FilterMode mode = FilterMode.Trilinear, TextureFormat format = TextureFormat.ARGB32
		)
		{
			var texR = new Rect(0, 0, width, height);

			//We need the source texture in VRAM because we render with it
			src.filterMode = mode;
			src.Apply(true);

			//Using RTT for best quality and performance. Thanks, Unity 5
			var rtt = new RenderTexture(width, height, 32);

			//Set the RTT in order to render to it
			Graphics.SetRenderTarget(rtt);

			//Setup 2D matrix in range 0..1, so nobody needs to care about sized
			GL.LoadPixelMatrix(0, 1, 1, 0);

			//Then clear & draw the texture to fill the entire RTT.
			GL.Clear(true, true, new Color(0, 0, 0, 0));

			Graphics.DrawTexture(new Rect(0, 0, 1, 1), src);
			//Get rendered data back to a new texture
			var result = new Texture2D(width, height, format, true);

			result.Reinitialize(width, height);

			result.ReadPixels(texR, 0, 0, true);
			return result;
		}

		public static string SaveTextureAsPNG(Texture2D texture, string path, string name)
		{
			var _bytes = texture.EncodeToPNG();

			if (!Directory.Exists(path))
				Directory.Exists(path);

			var fullPath = Path.Combine(path, name + ".png");

			File.WriteAllBytes(fullPath, _bytes);

			return path;
		}
	}
}