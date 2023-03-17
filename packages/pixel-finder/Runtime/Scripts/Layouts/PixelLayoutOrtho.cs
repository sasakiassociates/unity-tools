using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Sasaki.Unity
{
	/// <summary>
	///   A single finder with ortho settings
	/// </summary>
	[AddComponentMenu("Sasaki/PixelFinder/Ortho Layout")]
	public class PixelLayoutOrtho : PixelLayout
	{
		[SerializeField] float _orthoSize;

		public float orthoSize
		{
			get => _orthoSize;
			set => _orthoSize = value;
		}

		internal override IEnumerable<FinderDirection> finderSetups
		{
			get => new[] { FinderDirection.Front };
		}

		public override void Init(int collectionSize, Color32[] colors, UnityAction onDone = null)
		{
			base.Init(collectionSize, colors, onDone);

			foreach (var finder in Finders) finder.OrthographicSize = orthoSize;
		}
	}

}