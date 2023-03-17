using System.Collections.Generic;
using UnityEngine.Events;

namespace Sasaki.Unity
{
	public interface IPixelLayout
	{
		/// <summary>
		/// Name of the layout 
		/// </summary>
		public string LayoutName { get; }

		/// <summary>
		///  <para>Returns the count of how many finders are in the layout.</para>
		/// </summary>
		public int FinderCount { get; }

		/// <summary>
		///  <para>Returns the count of how many Colors are being analyzed.</para>
		/// </summary>
		public int ColorCount { get; }

		/// <summary>
		///  <para>Returns the point count for the data collection in <see cref="PixelDataContainer"/>.</para>
		/// </summary>
		public int CollectionSize { get; }

		/// <summary>
		/// <para>Current index of the point list.</para>
		/// </summary>
		public int PointIndex { get; }

		/// <summary>
		/// <para>Returns true if all finders have completed their tasks.</para>
		/// </summary>
		public bool WorkComplete { get; }

		/// <summary>
		///  <para>List of all pixel finders in layout.</para>
		/// </summary>
		public List<PixelFinder> Finders { get; }

		/// <summary>
		///  <para>Copies all the data from the finders in this layout.</para>
		/// </summary>
		public PixelDataContainer Data { get; }

		/// <summary>
		///  <para>Starts the coroutine for each pixel finder in the layout at the selected index.</para>
		/// </summary>
		/// <param name="index">Each run sets the <see cref="PointIndex"/> and moves forward</param>
		public void Run(int index);

		/// <summary>
		/// Event called when all finder objects are completed in a given layout
		/// </summary>
		public event UnityAction OnComplete;

		/// <summary>
		/// Event called when a screen shot is captured 
		/// </summary>
		public event UnityAction<LayoutCaptureArgs> OnCapture;

		/// <summary>
		/// Event called when a screen shot is captured 
		/// </summary>
		public event UnityAction<LayoutScreenCaptureArgs> OnScreenCapture;
	}
}