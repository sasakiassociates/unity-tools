using System.Collections.Generic;
using System.Linq;

namespace Sasaki.Unity
{

	/// <inheritdoc />
	public struct PixelSystemData : IPixelSystemDataContainer
	{
		public PixelSystemData(IPixelSystem obj)
		{
			Name = obj.SystemName;
			ItemName = new string[obj.Layouts.Count];
			Data = new PixelLayoutData[obj.Layouts.Count];

			for (var i = 0; i < obj.Layouts.Count; i++)
			{
				ItemName[i] = obj.Layouts[i].LayoutName;
				Data[i] = new PixelLayoutData(obj.Layouts[i]);
			}
		}
	
		public string Name { get; }
	
		/// <summary>
		///   Names of different layouts
		/// </summary>
		public string[] ItemName { get; }
	
		/// <summary>
		///   Data from each layout
		/// </summary>
		public PixelLayoutData[] Data { get; }
	}

}