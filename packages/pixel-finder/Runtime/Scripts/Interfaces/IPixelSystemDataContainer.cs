namespace Sasaki.Unity
{
	public interface IPixelSystemDataContainer
	{
		public string name { get; }

		public string[] layoutNames { get; }

		public PixelLayoutData[] data { get; }
	}
}