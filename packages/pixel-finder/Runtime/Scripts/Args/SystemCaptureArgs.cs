using System;
using System.Collections.Generic;

namespace Sasaki.Unity
{
	public class SystemCaptureArgs : EventArgs
	{
		public readonly List<LayoutCaptureArgs> args;
		public readonly string name;

		public SystemCaptureArgs(List<LayoutCaptureArgs> args, string name)
		{
			this.args = args;
			this.name = name;
		}
	}

}