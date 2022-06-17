using System.Collections.Generic;
using System.Linq;

namespace Sasaki.Unity
{
	public readonly struct PixelDataContainer
	{
		public PixelDataContainer(int totalSize = 1)
		{
			data = new double[totalSize][];
		}

		public double[][] data { get; }

		public void Set(double[] values, int index = 0)
		{
			this.data[index] = values;
		}
	}

	public readonly struct FinderLayoutDataContainer : IFinderLayoutData
	{
		public FinderLayoutDataContainer(IReadOnlyList<PixelFinder> finders, string name)
		{
			finderNames = null;
			data = null;
			this.name = name;

			if (finders == null || !finders.Any())
				return;

			finderNames = new string[finders.Count];
			data = new PixelDataContainer[finders.Count];

			for (var i = 0; i < finders.Count; i++)
			{
				finderNames[i] = finders[i].name;
				data[i] = finders[i].data;
			}
		}
		public string name { get; }
		/// <summary>
		/// Names of each finder
		/// </summary>
		public string[] finderNames { get; }

		/// <summary>
		/// Data of each finder
		/// </summary>
		public PixelDataContainer[] data { get; }
	}

	public readonly struct FinderSystemDataContainer : IFinderSystemData
	{
		public FinderSystemDataContainer(IReadOnlyList<PixelFinderLayout> finders, string name)
		{
			this.layoutNames = null;
			this.data = null;
			this.name = name;

			if (finders == null || !finders.Any())
				return;

			layoutNames = new string[finders.Count];
			data = new FinderLayoutDataContainer[finders.Count];

			for (var i = 0; i < finders.Count; i++)
			{
				layoutNames[i] = finders[i].name;
				data[i] = finders[i].container;
			}
		}

		public string name { get; }
		/// <summary>
		/// Names of different layouts 
		/// </summary>
		public string[] layoutNames { get; }

		/// <summary>
		/// Data from each layout
		/// </summary>
		public FinderLayoutDataContainer[] data { get; }
	}

}