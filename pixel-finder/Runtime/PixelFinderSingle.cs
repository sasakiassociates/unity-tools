using System.Collections.Generic;

namespace Sasaki.Unity
{

	public class PixelFinderSingle : PixelFinderSystem
	{
		internal override IEnumerable<FinderDirection> finderSetups => new[] { FinderDirection.Front };

	}
}