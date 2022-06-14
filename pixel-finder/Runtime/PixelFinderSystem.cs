using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Sasaki.Unity
{
	public class PixelFinderSystem : MonoBehaviour
	{
		[SerializeField] List<PixelFinderLayout> _layouts;

		[SerializeField, HideInInspector]
		Vector3[] _points;

		[SerializeField, HideInInspector]
		int _index;

		(bool auto, bool running) _is;

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
				layout.onComplete += CheckFindersInSystem;
				layout.transform.SetParent(transform);
			}
		}

		public void Run(int startingIndex = 0)
		{
			isRunning = true;
			pointIndex = startingIndex;
			MoveAndRender();
		}

		void MoveAndRender()
		{
			// step move forward
			transform.position = points[pointIndex];
			foreach (var layout in _layouts)
				layout.Run();
		}

		protected virtual void OnPostListComplete()
		{
			onComplete?.Invoke(dataContainer);
		}

		protected virtual void CheckFindersInSystem()
		{
			if (allDone)
			{
				// if we move manually we send the data back at each point
				if (!autoRun)
				{
					isRunning = false;
					onComplete?.Invoke(dataContainer);
					return;
				}

				// if we are in auto run we check our points and move on
				pointIndex++;
				if (pointIndex >= points.Length)
				{
					isRunning = false;
					OnPostListComplete();
				}
				else
					MoveAndRender();
			}
		}

		#region Events

		public event UnityAction<IFinderSystemData> onComplete;

		#endregion

	}
}