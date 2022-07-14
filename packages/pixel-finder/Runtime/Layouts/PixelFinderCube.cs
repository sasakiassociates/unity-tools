using System.Collections.Generic;
using UnityEngine;

namespace Sasaki.Unity
{

	/// <summary>
	///   A six sided pixel finder layout
	/// </summary>
	[AddComponentMenu("Sasaki/PixelFinder/Cube Layout")]
	public class PixelFinderCube : PixelFinderLayout
	{
		internal override IEnumerable<FinderDirection> finderSetups
		{
			get =>
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
}