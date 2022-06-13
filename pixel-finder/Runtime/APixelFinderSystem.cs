using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Sasaki.Unity
{
	public abstract class APixelFinderSystem : MonoBehaviour
	{
		[SerializeField, HideInInspector]
		List<PixelFinder> _finders;

		[SerializeField, HideInInspector]
		Vector3[] _points;

		[SerializeField, HideInInspector]
		int _index;

		public int typeCount
		{
			get => finderSetups.Count();
		}

		public Vector3[] points
		{
			get => _points;
		}

		public bool isRunning { get; protected set; }

		public void Clear()
		{
			if (_finders != null && _finders.Any())
				for (int i = _finders.Count - 1; i >= 0; i--)
					Destroy(_finders[i].gameObject);

			_finders = new List<PixelFinder>(typeCount);
		}

		void MoveAndRender()
		{
			// step move forward
			transform.position = points[_index];
			foreach (var finder in _finders)
				StartCoroutine(finder.Run(_index));
		}

		protected virtual void CheckFindersInSystem()
		{
			if (allDone)
			{
				_index++;

				if (_index >= points.Length)
				{
					isRunning = false;
					onSystemRunComplete?.Invoke(new FinderSystemDataContainer(_finders));
				}
				else
					MoveAndRender();
			}
		}

		public void Init(Vector3[] systemPoints, Color32 color)
		{
			Init(systemPoints, new[] { color });
		}

		public void Init(Vector3[] systemPoints, Color32[] colors)
		{
			Clear();

			_points = systemPoints;

			var prefab = new GameObject().AddComponent<PixelFinder>();

			foreach (var s in finderSetups)
			{
				var finder = Instantiate(prefab, transform, true);
				finder.name = "Finder[" + s + "]";

				transform.localRotation = Quaternion.Euler(s switch
				{
					FinderDirection.Front => new Vector3(0, 0, 0),
					FinderDirection.Left => new Vector3(0, 90, 0),
					FinderDirection.Back => new Vector3(0, 180, 0),
					FinderDirection.Right => new Vector3(0, -90, 0),
					FinderDirection.Up => new Vector3(90, 0, 0),
					FinderDirection.Down => new Vector3(-90, 0, 0),
					_ => new Vector3(0, 0, 0)
				});

				finder.Init(colors, CheckFindersInSystem, _points.Length, typeCount);
				_finders.Add(finder);
			}

			Destroy(prefab.gameObject);
		}

		public void Run(int startingIndex = 0)
		{
			_index = startingIndex;
			MoveAndRender();
		}

		internal abstract IEnumerable<FinderDirection> finderSetups { get; }

		#region Events
		public event UnityAction<FinderSystemDataContainer> onSystemRunComplete;
		#endregion

		internal enum FinderDirection
		{
			Front,
			Left,
			Back,
			Right,
			Up,
			Down,
		}

		private bool allDone
		{
			get
			{
				foreach (var f in _finders)
					if (!f.isDone)
						return false;

				return true;
			}
		}
	}

}