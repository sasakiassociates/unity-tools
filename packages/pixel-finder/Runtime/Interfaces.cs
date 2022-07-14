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

	public interface IFinderSystemData
	{
		public string name { get; }

		public string[] layoutNames { get; }

		public FinderLayoutDataContainer[] data { get; }
	}

	public interface IFinderLayoutData
	{
		public string name { get; }

		public string[] finderNames { get; }

		public PixelDataContainer[] data { get; }
	}
}