namespace Sasaki.Unity
{
	public struct PixelLayoutData : IPixelLayoutDataContainer
	{
		public PixelLayoutData(IPixelLayout obj)
		{
			data = obj.Data;
			name = obj.LayoutName;
			finderNames = new string[obj.Finders.Count];
	
			for (var i = 0; i < obj.Finders.Count; i++)
			{
				finderNames[i] = obj.Finders[i].name;
			}
			
		}
	
		/// <summary>
		/// Name of the Layout
		/// </summary>
		public string name { get; }
	
		/// <summary>
		///   Names of each finder
		/// </summary>
		public string[] finderNames { get; }
	
		/// <summary>
		///   Data of each finder
		/// </summary>
		public PixelDataContainer data { get; }
	}
}