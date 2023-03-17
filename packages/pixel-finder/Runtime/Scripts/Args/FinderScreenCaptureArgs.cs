using UnityEngine;

namespace Sasaki.Unity
{
	public class FinderScreenCaptureArgs : FinderCaptureArgs
	{

		public readonly Texture2D image;

		public FinderScreenCaptureArgs(string name, int point, int[] values, Texture2D image) : base(name, point, values)
		{
			this.image = image;
		}
	}
}