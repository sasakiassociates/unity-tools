using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Sasaki.Unity
{
	public class PixelFinderSystem : MonoBehaviour
	{
		[SerializeField] List<PixelFinderLayout> _layouts;

		[SerializeField, HideInInspector] Vector3[] _points;

		[SerializeField, HideInInspector] int _index;

		(bool auto, bool running, bool compelte) _is;

		public bool autoRun
		{
			get => _is.auto;
			set => _is.auto = value;
		}

		public bool isRunning
		{
			get => _is.running;
			protected set => _is.running = value;
		}

		public bool isComplete
		{
			get => _is.compelte;
			protected set => _is.compelte = value;
		}

		public List<PixelFinderLayout> layouts
		{
			get => _layouts;
			protected set => _layouts = value;
		}

		public Vector3[] points
		{
			get => _points;
			protected set => _points = value;
		}

		public int pointIndex
		{
			get => _index;
			protected set => _index = value;
		}

		protected virtual IFinderSystemData dataContainer
		{
			get => new FinderSystemDataContainer(_layouts, name);
		}

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

		public void ClearLayouts()
		{
			if (_layouts != null && _layouts.Any())
				for (int i = _layouts.Count - 1; i >= 0; i--)
					Destroy(_layouts[i].gameObject);
		}

		protected void ResetDataContainer(int collectionSize)
		{
			if (layouts != null && layouts.Any())
				foreach (var f in layouts)
					f.ResetDataContainer(collectionSize);
		}

		public void Init(Vector3[] systemPoints, Color32 color, List<PixelFinderLayout> inputLayouts = null)
		{
			Init(systemPoints, new[] { color }, inputLayouts);
		}

		/// <summary>
		/// Initialize the system. Will clear any existing layouts and data
		/// </summary>
		/// <param name="systemPoints">Points where analysis will be captured</param>
		/// <param name="colors">Pixel color that will be searched for</param>
		/// <param name="inputLayouts">Type of layouts to use with this system</param>
		public virtual void Init(Vector3[] systemPoints, Color32[] colors, List<PixelFinderLayout> inputLayouts = null)
		{
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
		}

		/// <summary>
		/// Starts the system
		/// </summary>
		/// <param name="startingIndex">the index to start the system at</param>
		public void Run(int startingIndex = 0)
		{
			pointIndex = startingIndex;

			isRunning = true;
			// isComplete = false;

			MoveAndRender();
		}

		void MoveAndRender()
		{
			// step move forward
			transform.position = points[pointIndex];
			
			foreach (var layout in _layouts)
				layout.Run(pointIndex);
		}

		void CheckLayoutsInSystem()
		{
			if (allDone)
			{
				// if (isComplete)
				// {
				// 	Debug.Log($"{name} is already completed");
				// 	return;
				// }

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
					MoveAndRender();
			}
		}

		protected virtual void GatherSystemData()
		{
			onComplete?.Invoke(dataContainer);
		}

		#region Events

		public event UnityAction<IFinderSystemData> onComplete;

		#endregion

	}
}