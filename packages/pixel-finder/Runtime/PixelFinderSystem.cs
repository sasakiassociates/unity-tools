using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Sasaki.Unity
{
	/// <summary>
	///   A system object that handles positioning a series of layouts throughout a set of points
	/// </summary>
	[AddComponentMenu("Sasaki/PixelFinder/FinderSystem")]
	public class PixelFinderSystem : MonoBehaviour
	{
		[SerializeField] List<PixelFinderLayout> _layouts;

		[SerializeField] [HideInInspector] Vector3[] _points;

		[SerializeField] [HideInInspector] int _index;

		(bool auto, bool running, bool compelte) _is;

		/// <summary>
		///   Option for having the system move to the next point once the layouts are complete.
		/// </summary>
		public bool autoRun
		{
			get => _is.auto;
			set => _is.auto = value;
		}

		/// <summary>
		///   Returns true if layouts are working or the system is not through each point
		/// </summary>
		public bool isRunning
		{
			get => _is.running;
			protected set => _is.running = value;
		}

		/// <summary>
		///   Returns true if all layouts are done running and each point has been hit
		/// </summary>
		public bool isComplete
		{
			get => _is.compelte;
			protected set => _is.compelte = value;
		}

		/// <summary>
		///   Returns true if all layouts have completed their tasks
		/// </summary>
		public bool allDone
		{
			get
			{
				foreach (var f in _layouts)
					if (!f.allDone)
						return false;

				return true;
			}
		}

		/// <summary>
		///   Layouts used within this system
		/// </summary>
		public List<PixelFinderLayout> layouts
		{
			get => _layouts;
			protected set => _layouts = value;
		}

		/// <summary>
		///   Stored points for the system to move to
		/// </summary>
		public Vector3[] points
		{
			get => _points;
			protected set => _points = value;
		}

		/// <summary>
		///   Current point index for the finder
		/// </summary>
		public int pointIndex
		{
			get => _index;
			protected set => _index = value;
		}

		/// <summary>
		///   Returns the count of points
		/// </summary>
		public int collectionSize
		{
			get => points?.Length ?? 0;
		}

		/// <summary>
		///   Copies all the data in this system
		/// </summary>
		protected virtual IFinderSystemData dataContainer
		{
			get => new FinderSystemDataContainer(_layouts, name);
		}

		/// <summary>
		///   Sets the culling mask to all viewers within the system
		/// </summary>
		public int mask
		{
			set
			{
				foreach (var layout in layouts)
					layout.mask = value;
			}
		}

		/// <summary>
		///   Clears all active layouts in the system
		/// </summary>
		public void ClearLayouts()
		{
			if (_layouts != null && _layouts.Any())
				for (var i = _layouts.Count - 1; i >= 0; i--)
					Destroy(_layouts[i].gameObject);
		}

		/// <summary>
		///   Clears all values and creates a new collection for the size of the current point count
		/// </summary>
		protected void ResetDataContainer()
		{
			Debug.Log($"{name} is resetting data");

			if (layouts != null && layouts.Any())
				foreach (var f in layouts)
					f.ResetDataContainer(collectionSize);
		}

		/// <summary>
		///   Initializes a new system with layouts. Stores the points internally and passes the colors into the finders
		/// </summary>
		/// <param name="systemPoints"></param>
		/// <param name="color"></param>
		/// <param name="inputLayouts"></param>
		public void Init(Vector3[] systemPoints, Color32 color, List<PixelFinderLayout> inputLayouts = null)
		{
			Init(systemPoints, new[] { color }, inputLayouts);
		}

		/// <summary>
		///   Initialize the system. Will clear any existing layouts and data
		/// </summary>
		/// <param name="systemPoints">Points where analysis will be captured</param>
		/// <param name="colors">Pixel color that will be searched for</param>
		/// <param name="inputLayouts">Type of layouts to use with this system</param>
		public virtual void Init(Vector3[] systemPoints, Color32[] colors, List<PixelFinderLayout> inputLayouts = null)
		{
			name = "FinderSystem";

			if (inputLayouts != null && inputLayouts.Any())
			{
				ClearLayouts();
				_layouts = inputLayouts;
			}

			_points = systemPoints;

			foreach (var layout in _layouts)
			{
				layout.Init(_points.Length, colors);
				layout.onComplete += CheckLayoutsInSystem;
				layout.transform.SetParent(transform);
			}

			Application.targetFrameRate = 200;
		}

		/// <summary>
		///   Starts the system
		/// </summary>
		/// <param name="startingIndex">the index to start the system at</param>
		public void Run(int startingIndex = 0)
		{
			pointIndex = startingIndex;

			isRunning = true;

			MoveAndRender();
		}

		/// <summary>
		///   Main system call to set a new position and trigger a new render for each finder
		///   Handles Pre and Post move render calls
		/// </summary>
		void MoveAndRender()
		{
			PreMoveRender();

			// step move forward
			transform.position = points[pointIndex];

			foreach (var layout in _layouts)
				layout.Run(pointIndex);

			PostMoveRender();
		}

		/// <summary>
		///   Checks if all layouts are complete and if there are more points to gather from
		/// </summary>
		void CheckLayoutsInSystem()
		{
			if (allDone)
			{
				// if we move manually we send the data back at each point
				if (!autoRun)
				{
					isRunning = false;
					GatherSystemData();
					return;
				}

				// if we are in auto run we check our points and move on
				pointIndex++;
				if (pointIndex >= points.Length)
				{
					isRunning = false;
					GatherSystemData();
					// isComplete = true;
				}
				else
				{
					MoveAndRender();
				}
			}
		}

		/// <summary>
		///   Method to implement any logic after the system has moved
		/// </summary>
		protected virtual void PostMoveRender()
		{ }

		/// <summary>
		///   Method to implement any logic before the system has moved
		/// </summary>
		protected virtual void PreMoveRender()
		{ }

		/// <summary>
		///   Calls on complete and compiles the data into a container
		/// </summary>
		protected virtual void GatherSystemData()
		{
			OnComplete?.Invoke(dataContainer);
		}

		public event UnityAction<IFinderSystemData> OnComplete;
	}
}