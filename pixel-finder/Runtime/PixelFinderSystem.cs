using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Sasaki.Unity
{
	public abstract class PixelFinderSystem : MonoBehaviour
	{
		[SerializeField, HideInInspector]
		List<PixelFinderGPU> _finders;

		[SerializeField, HideInInspector]
		Vector3[] _points;

		[SerializeField, HideInInspector]
		int _index;

		public bool isRunning { get; protected set; }

		public Vector3[] points
		{
			get => _points;
		}

		public void Clear()
		{
			if (_finders != null && _finders.Any())
				for (int i = _finders.Count - 1; i >= 0; i--)
					Destroy(_finders[i].gameObject);

			_finders = new List<PixelFinderGPU>(typeCount);
		}

		public void Create(Vector3[] systemPoints, Color32 color)
		{
			Create(systemPoints, new[] { color });
		}

		public void Create(Vector3[] systemPoints, Color32[] colors)
		{
			Clear();

			_points = systemPoints;
			_isFirst = true;

			var prefab = new GameObject().AddComponent<PixelFinderGPU>();

			foreach (var s in finderSetups)
			{
				var finder = Instantiate(prefab, transform, true);
				finder.name = "Finder{" + s + "}";

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

				finder.Init(colors, _points.Length, typeCount);

				_finders.Add(finder);
			}

			Destroy(prefab.gameObject);
		}

		float _updateTime;

		bool _isFirst, _useUpdate;
		private float getStartTime()
		{
			_updateTime = 0f;
			return Time.realtimeSinceStartup;
		}

		private void getEndTime(ref float startTime)
		{
			var dt = Time.realtimeSinceStartup - startTime;
			_updateTime = _updateTime < 0 ? dt : Mathf.Lerp(_updateTime, dt, 0.3f);
			Debug.Log($"Complete!: {_updateTime * 1000.0f:F2}ms");
		}

		public Task RunTask(int startingIndex = 0)
		{
			isRunning = true;
			_index = startingIndex;

			var t0 = getStartTime();

			for (_index = startingIndex; _index < _points.Length; _index++)
			{
				foreach (var finder in _finders)
					finder.Run(_index).ToUniTask(this);

				Task.Yield();
			}

			OnSystemDataComplete?.Invoke(new FinderSystemDataContainer(_finders));

			isRunning = false;

			getEndTime(ref t0);

			// await Task.Yield();
			return Task.CompletedTask;
		}

		public IEnumerator RunCoroutine(int startingIndex = 0)
		{
			Debug.Log("Starting Run");

			isRunning = true;
			_index = startingIndex;

			var t0 = getStartTime();

			while (_index < _points.Length)
			{
				foreach (var finder in _finders)
					yield return StartCoroutine(finder.Run(_index));

				_index++;
				yield return null;
			}

			OnSystemDataComplete?.Invoke(new FinderSystemDataContainer(_finders));

			isRunning = false;

			getEndTime(ref t0);

			yield return null;
		}

		public void RunSync(int startingIndex = 0)
		{
			Debug.Log("Starting Run");
			isRunning = true;

			var t0 = getStartTime();

			for (_index = startingIndex; _index < _points.Length; _index++)
			{
				foreach (var finder in _finders)
					StartCoroutine(finder.Run(_index));
			}

			OnSystemDataComplete?.Invoke(new FinderSystemDataContainer(_finders));
			isRunning = false;

			getEndTime(ref t0);
		}

		internal abstract IEnumerable<FinderDirection> finderSetups { get; }

		public int typeCount => finderSetups.Count();

		#region Events
		public event UnityAction<FinderSystemDataContainer> OnSystemDataComplete;
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
					if (!f.isRunning)
						return false;

				return true;
			}
		}
	}

}