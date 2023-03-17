using System;
using System.Collections.Generic;

namespace Sasaki.Unity
{
	public class LayoutScreenCaptureArgs : EventArgs
	{
		public readonly List<FinderScreenCaptureArgs> finderArgs;
		public readonly string name;

		public LayoutScreenCaptureArgs(List<FinderScreenCaptureArgs> finderArgs, string name)
		{
			this.finderArgs = finderArgs;
			this.name = name;
		}
	}
}