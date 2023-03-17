using System.Collections.Generic;
using System.Linq;

namespace Sasaki.Unity
{

	/// <inheritdoc />
	public struct PixelSystemData : IPixelSystemDataContainer
	{
		public PixelSystemData(IPixelSystem obj)
		{
			name = obj.SystemName;
			layoutNames = new string[obj.Layouts.Count];
			data = new PixelLayoutData[obj.Layouts.Count];

			for (var i = 0; i < obj.Layouts.Count; i++)
			{
				layoutNames[i] = obj.Layouts[i].LayoutName;
				data[i] = new PixelLayoutData(obj.Layouts[i]);
			}
		}
	
		public string name { get; }
	
		/// <summary>
		///   Names of different layouts
		/// </summary>
		public string[] layoutNames { get; }
	
		/// <summary>
		///   Data from each layout
		/// </summary>
		public PixelLayoutData[] data { get; }
	}

}