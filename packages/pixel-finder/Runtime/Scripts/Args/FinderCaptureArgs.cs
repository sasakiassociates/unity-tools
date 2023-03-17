using System;

namespace Sasaki.Unity
{
	public class FinderCaptureArgs : EventArgs
	{

		public readonly string name;
		public readonly int point;
		public readonly int[] values;

		public FinderCaptureArgs( string name, int point, int[]values)
		{
			this.name = name;
			this.point = point;
			this.values = values;
		}
	}
}