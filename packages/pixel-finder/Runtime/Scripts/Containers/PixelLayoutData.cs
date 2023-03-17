namespace Sasaki.Unity
{
	public struct PixelLayoutData : IPixelLayoutDataContainer
	{
		public PixelLayoutData(IPixelLayout obj)
		{
			Data = obj.Data;
			Name = obj.LayoutName;
			FinderNames = new string[obj.Finders.Count];
	
			for (var i = 0; i < obj.Finders.Count; i++)
			{
				FinderNames[i] = obj.Finders[i].name;
			}
			
		}
	
		/// <summary>
		/// Name of the Layout
		/// </summary>
		public string Name { get; }
	
		/// <summary>
		///   Names of each finder
		/// </summary>
		public string[] FinderNames { get; }
	
		/// <summary>
		///   Data of each finder
		/// </summary>
		public PixelDataContainer Data { get; }
		
		
	}
}