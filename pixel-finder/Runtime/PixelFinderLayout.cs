using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Sasaki.Unity
{
	public abstract class PixelFinderLayout : MonoBehaviour
	{
		[SerializeField, HideInInspector]
		List<PixelFinder> _finders;

		public int typeCount
		{
			get => finderSetups.Count();
		}
		
		public void Clear()
		{
			if (_finders != null && _finders.Any())
				for (int i = _finders.Count - 1; i >= 0; i--)
					Destroy(_finders[i].gameObject);

			_finders = new List<PixelFinder>(typeCount);
		}

		public FinderLayoutDataContainer data
		{
			get => new FinderLayoutDataContainer(_finders);
		}

		protected virtual void CheckFindersInLayout()
		{
			if (allDone)
			{
				onComplete?.Invoke();
			}
		}

		public void Init(int pointCount, Color32 color)
		{
			Init(pointCount, new[] { color });
		}

		public void Init(int pointCount, Color32[] colors)
		{
			Clear();

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

				finder.Init(colors, CheckFindersInLayout, pointCount, typeCount);
				_finders.Add(finder);
			}

			Destroy(prefab.gameObject);
		}

		public void Run(int index = 0)
		{
			foreach (var finder in _finders)
				StartCoroutine(finder.Run(index));
		}

		internal abstract IEnumerable<FinderDirection> finderSetups { get; }

		#region Events
		public event UnityAction onComplete;
		#endregion

		public bool allDone
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