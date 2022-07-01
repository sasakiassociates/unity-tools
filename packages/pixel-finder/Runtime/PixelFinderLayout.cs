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

		public int cullingMasks
		{
			set
			{
				foreach (var finder in _finders)
					finder.cam.cullingMask = value;
			}
		}

		public void ClearFinders()
		{
			if (_finders != null && _finders.Any())
				for (int i = _finders.Count - 1; i >= 0; i--)
					Destroy(_finders[i].gameObject);

			_finders = new List<PixelFinder>(typeCount);
		}

		public List<PixelFinder> finders
		{
			get => _finders;

		}

		public FinderLayoutDataContainer container
		{
			get => new FinderLayoutDataContainer(_finders, name);
		}

		protected virtual void CheckFindersInLayout()
		{
			if (allDone)
			{
				onComplete?.Invoke();
			}
		}

		public void ResetDataContainer(int collectionSize)
		{
			if (_finders != null && _finders.Any())
				foreach (var f in _finders)
					f.SetNewDataCollection(collectionSize);
		}

		public void Init(int collectionSize, Color32 color)
		{
			Init(collectionSize, new[] { color });
		}

		public virtual void Init(int collectionSize, Color32[] colors)
		{
			ClearFinders();

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

				finder.Init(colors, CheckFindersInLayout, collectionSize, typeCount);
				_finders.Add(finder);
			}

			Destroy(prefab.gameObject);
		}

		public void Run(int index)
		{
			foreach (var finder in _finders)
				StartCoroutine(finder.Render(index));
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