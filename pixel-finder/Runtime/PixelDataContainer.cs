using System.Collections.Generic;

namespace Sasaki.Unity
{
	public readonly struct PixelDataContainer
	{
		public PixelDataContainer(int totalSize = 1)
		{
			Data = new double[totalSize][];
		}

		public double[][] Data { get; }

		public void Set(double[] data, int index = 0)
		{
			Data[index] = data;
		}
	}


	public readonly struct FinderLayoutDataContainer
	{
		public FinderLayoutDataContainer(List<PixelFinder> finders)
		{
			data = new Dictionary<string, PixelDataContainer>();

			foreach (var f in finders)
				data.Add(f.name, f.data);
		}
		
		public readonly Dictionary<string, PixelDataContainer> data;

	}
	
	public readonly struct FinderSystemDataContainer
	{
		public FinderSystemDataContainer(List<PixelFinderLayout> finders)
		{
			data = new Dictionary<string, FinderLayoutDataContainer>();

			foreach (var f in finders)
				data.Add(f.name, f.data);
		}
		
		public readonly Dictionary<string, FinderLayoutDataContainer> data;
	}
	

}