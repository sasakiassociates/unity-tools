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

		public void Init(Vector3[] systemPoints, Color32 color, List<PixelFinderLayout> layouts = null)
		{
			Init(systemPoints, new[] { color });
		}

		public virtual void Init(Vector3[] systemPoints, Color32[] colors, List<PixelFinderLayout> layouts = null)
		{
			if (layouts != null && layouts.Any())
			{
				Clear();
				_layouts = layouts;
			}

			_points = systemPoints;

			foreach (var layout in _layouts)
			{
				layout.Init(_points.Length, colors);
				layout.onComplete += CheckFindersInSystem;
			}
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