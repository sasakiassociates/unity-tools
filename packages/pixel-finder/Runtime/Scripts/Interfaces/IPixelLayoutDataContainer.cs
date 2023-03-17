namespace Sasaki.Unity
{
	public interface IPixelLayoutDataContainer
	{
		public string name { get; }

		public string[] finderNames { get; }

		public PixelDataContainer data { get; }

	}
}