using System;
using UnityEngine;

namespace Sasaki.Unity
{
	public interface IPixelSystemSettings
	{
		public bool AutoRun { get; }

		public int FrameRate { get; }

	}

	[Serializable]
	public class PixelSystemSettings : IPixelSystemSettings
	{

		[SerializeField] bool _autoRun;

		[SerializeField] int _frameRate;

		public bool AutoRun
		{
			get => _autoRun;
			set => _autoRun = value;
		}

		public int FrameRate
		{
			get => _frameRate;
			set => _frameRate = value;
		}
	}
}