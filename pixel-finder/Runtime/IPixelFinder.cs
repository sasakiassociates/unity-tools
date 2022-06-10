using UnityEngine;

namespace Sasaki.Unity
{
	public interface IPixelFinder
	{
		public Color background { get; set; }

		public Camera cam { get; }

		public RenderTexture texture { get; }

		public int size { get; }
	}
}