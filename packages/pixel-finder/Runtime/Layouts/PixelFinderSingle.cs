using System.Collections.Generic;
using UnityEngine;

namespace Sasaki.Unity
{
	/// <summary>
	///   A single sided finder layout
	/// </summary>
	[AddComponentMenu("Sasaki/PixelFinder/Single Layout")]
	public class PixelFinderSingle : PixelFinderLayout
	{
		internal override IEnumerable<FinderDirection> finderSetups
		{
			get => new[] { FinderDirection.Front };
		}
	}
}