using System.Collections.Generic;

namespace Sasaki.Unity
{
	
	public class PixelFinderCube : APixelFinderSystem
	{
		internal override IEnumerable<FinderDirection> finderSetups =>
			new[]
			{
				FinderDirection.Front,
				FinderDirection.Left,
				FinderDirection.Back,
				FinderDirection.Right,
				FinderDirection.Up,
				FinderDirection.Down
			};

	}
}