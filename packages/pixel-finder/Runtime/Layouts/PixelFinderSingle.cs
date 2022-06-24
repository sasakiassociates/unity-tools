using System.Collections.Generic;

namespace Sasaki.Unity
{

	public class PixelFinderSingle : PixelFinderLayout
	{
		internal override IEnumerable<FinderDirection> finderSetups => new[] { FinderDirection.Front };

	}
}