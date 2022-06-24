using System.Collections.Generic;

namespace Sasaki.Unity
{

	public class PixelFinderHorizontal : PixelFinderLayout
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