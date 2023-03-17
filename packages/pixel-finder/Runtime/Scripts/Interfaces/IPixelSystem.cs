using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Sasaki.Unity
{
	public interface IPixelSystem
	{
		
		/// <summary>
		/// Name of the system
		/// </summary>
		public string SystemName { get; }

		/// <summary>
		///  <para>Option for having the system move to the next point once the layouts are complete.</para>
		/// </summary>
		public bool AutoRun { get; }

		/// <summary>
		/// <para>Check if system has completed its work.</para>
		/// </summary>
		public bool WorkComplete { get; }

		/// <summary>
		///  <para>Current point index for the finder.</para>
		/// </summary>
		public int PointIndex { get; }

		/// <summary>
		/// <para>Stored points for the system to move to.</para>
		/// </summary>
		public Vector3[] Points { get; }

		/// <summary>
		/// <para>Returns the count of points.</para>
		/// </summary>
		public int CollectionSize { get; }

		/// <summary>
		/// <para>Sets the culling mask to all viewers within the system.</para>
		/// </summary>
		public int Mask { set; }

		/// <summary>
		/// <para>Layouts used within this system.</para>
		/// </summary>
		public List<PixelLayout> Layouts { get; }

		/// <summary>
		/// The collection of data that can be gathered after using <see cref="Run"/>
		/// </summary>
		public IPixelSystemDataContainer Data { get; }

		/// <summary>
		/// <para> Starts the system and works through all the <see cref="Points"/> stored in the finder. This will automatically toggle <see cref="AutoRun"/> to true.</para>
		/// </summary>
		public void Run();

		/// <summary>
		/// <para>Manually sets the viewer to a specific point in the collection. This will automatically toggle <see cref="AutoRun"/> to false.</para>
		/// </summary>
		/// <param name="pointIndex">the point to use</param>
		public void Capture(int pointIndex);

		/// <summary>
		///  <para>Initialize the system. Will clear any existing layouts and data.</para>
		/// </summary>
		/// <param name="systemPoints">Points where analysis will be captured</param>
		/// <param name="colors">Pixel color that will be searched for</param>
		/// <param name="inputLayouts">Type of layouts to use with this system</param>
		public void Init(Vector3[] systemPoints, Color32[] colors, List<PixelLayout> inputLayouts = null);

		
		/// <summary>
		/// <para>Little macro for getting the culling mask layers to use</para>
		/// </summary>
		/// <returns></returns>
		public Dictionary<string, int> GetMaskLayers();

		/// <summary>
		/// <para>Sets the value for <see cref="Application.targetFrameRate"/></para>
		/// </summary>
		/// <param name="value"></param>
		public void SetFrameRate(int value);

		public event UnityAction<SystemCaptureArgs> OnCapture;

		public event UnityAction<IPixelLayout> OnLayoutAdded;

		public event UnityAction<IPixelSystemDataContainer> OnDataGathered;

		public event UnityAction OnComplete;

		public event UnityAction<int> OnPointComplete;

		public event UnityAction<int> OnFrameRateSet; 

		public event UnityAction<bool> OnAutoRun;
	}
}