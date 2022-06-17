using System.Collections.Generic;
using UnityEngine;

namespace Sasaki.Unity
{
	public class PixelFinderOrtho : PixelFinderLayout
	{
		[SerializeField] private float _orthoSize;

		public float orthoSize
		{
			get => _orthoSize;
			set => _orthoSize = value;
		}

		internal override IEnumerable<FinderDirection> finderSetups
		{
			get => new[] { FinderDirection.Front };
		}

		public override void Init(int collectionSize, Color32[] colors)
		{
			base.Init(collectionSize, colors);
			foreach (var finder in finders)
			{
				finder.orthographicSize = orthoSize;
			}
		}
	}

}