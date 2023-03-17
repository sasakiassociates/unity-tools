namespace Sasaki.Unity
{

  public interface IPixelDataContainer
  {
    public string Name { get; }
    public string[] ItemName { get; }
  }

  public interface IPixelSystemDataContainer
  {
    public PixelLayoutData[] Data { get; }
  }

}
