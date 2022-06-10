using System.Collections.Generic;

namespace Sasaki.Unity
{

	public class PixelFinderHorizontal : PixelFinderSystem
	{
		internal override IEnumerable<FinderDirection> finderSetups =>
			new[]
			{
				FinderDirection.Front,
				FinderDirection.Left,
				FinderDirection.Back,
				FinderDirection.Right
			};
	}
}