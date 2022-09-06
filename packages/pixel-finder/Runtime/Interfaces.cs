using UnityEngine;
using UnityEngine.Events;

namespace Sasaki.Unity
{
	public interface IPixelFinder
	{
		public Color background { get; set; }

		public Camera cam { get; }

		public RenderTexture texture { get; }

		public int size { get; }

		public event UnityAction<uint[]> onValueSet;

		public Color32[] colors { get; }
	}

	public interface IFinderLayoutData
	{
		public string name { get; }

		public string[] finderNames { get; }

		public PixelDataContainer[] data { get; }
	}

	public interface IFinderSystemData
	{
		public string name { get; }

		public string[] layoutNames { get; }

		public FinderLayoutDataContainer[] data { get; }
	}

}