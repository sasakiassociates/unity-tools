using UnityEngine;
using UnityEngine.Events;

namespace Sasaki.Unity
{
	public interface IPixelFinder
	{
		/// <summary>
		/// Background color for viewer <see cref="Cam"/>
		/// </summary>
		public Color Background { get; set; }

		/// <summary>
		/// The main unity <see cref="Camera"/> object
		/// </summary>
		public Camera Cam { get; }

		/// <summary>
		/// The active texture used for sending data to the GPU
		/// </summary>
		public RenderTexture Texture { get; }

		/// <summary>
		/// Size of <see cref="Texture"/>
		/// </summary>
		public int TextureSize { get; }

		/// <summary>
		/// The collection of colors to search for in each pixel 
		/// </summary>
		public Color32[] Colors { get; }

		/// <summary>
		///   Callback event triggered once the analysis is completed
		/// </summary>
		public event UnityAction OnDone;

	}

}