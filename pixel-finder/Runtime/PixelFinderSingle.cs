using System.Collections.Generic;

namespace Sasaki.Unity
{

	public class PixelFinderSingle : APixelFinderSystem
	{
		internal override IEnumerable<FinderDirection> finderSetups => new[] { FinderDirection.Front };

	}
}