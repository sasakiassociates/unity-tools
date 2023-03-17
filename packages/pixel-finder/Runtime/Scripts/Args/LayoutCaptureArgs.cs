using System;
using System.Collections.Generic;

namespace Sasaki.Unity
{
	public class LayoutCaptureArgs : EventArgs
	{
		public readonly List<FinderCaptureArgs> finderArgs;
		public readonly string name;

		public LayoutCaptureArgs(List<FinderCaptureArgs> finderArgs, string name)
		{
			this.finderArgs = finderArgs;
			this.name = name;
		}
	}
}