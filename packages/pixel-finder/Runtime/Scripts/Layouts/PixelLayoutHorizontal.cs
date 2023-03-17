using System.Collections.Generic;
using UnityEngine;

namespace Sasaki.Unity
{
	/// <summary>
	///   A four sided finder layout
	/// </summary>
	[AddComponentMenu("Sasaki/PixelFinder/Horizontal Layout")]
	public class PixelLayoutHorizontal : PixelLayout
	{
		internal override IEnumerable<FinderDirection> finderSetups
		{
			get =>
				new[]
				{
					FinderDirection.Front,
					FinderDirection.Left,
					FinderDirection.Back,
					FinderDirection.Right
				};
		}
	}
}