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

		public bool isRunning { get; protected set; }

		public List<PixelFinderLayout> layouts
		{
			get => _layouts;
			protected set => _layouts = value;
		}

		public Vector3[] points
		{
			get => _points;
		}

		public int pointIndex
		{
			get => _index;
			protected set => _index = value;
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

		public void Clear()
		{
			if (_layouts != null && _layouts.Any())
				for (int i = _layouts.Count - 1; i >= 0; i--)
					Destroy(_layouts[i].gameObject);
		}

		public void Init(Vector3[] systemPoints, Color32 color, List<PixelFinderLayout> inputLayouts = null)
		{
			Init(systemPoints, new[] { color }, inputLayouts);
		}

		public virtual void Init(Vector3[] systemPoints, Color32[] colors, List<PixelFinderLayout> inputLayouts = null)
		{
			if (inputLayouts != null && inputLayouts.Any())
			{
				Clear();
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

		protected virtual void StartRun(int startingIndex = 0)
		{
			isRunning = true;

			Run(startingIndex);
		}

		public void Run(int startingIndex = 0)
		{
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

		protected virtual void CheckFindersInSystem()
		{
			if (allDone)
			{
				pointIndex++;

				if (pointIndex >= points.Length)
				{
					isRunning = false;
					onComplete?.Invoke(new FinderSystemDataContainer(_layouts));
				}
				else
					MoveAndRender();
			}
		}

		#region Events
		public event UnityAction<FinderSystemDataContainer> onComplete;
		#endregion

	}
}