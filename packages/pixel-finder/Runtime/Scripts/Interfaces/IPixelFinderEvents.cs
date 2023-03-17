using UnityEngine.Events;

namespace Sasaki.Unity
{
	public interface IPixelFinderEvents
	{
		/// <summary>
		/// Passes the raw values captured when getting the data back from the GPU
		/// </summary>
		public event UnityAction<uint[]> OnValueSet;

		/// <summary>
		/// Triggered event when a single capture is called
		/// </summary>
		public event UnityAction<FinderCaptureArgs> OnCapture;

		/// <summary>
		/// Triggered event when capture is called with texture
		/// </summary>
		public event UnityAction<FinderCaptureArgs> OnScreenCapture;
	}
}