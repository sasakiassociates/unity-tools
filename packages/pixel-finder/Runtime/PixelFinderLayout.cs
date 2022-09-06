using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Sasaki.Unity
{
	/// <summary>
	///   Abstract class for handling different layouts of pixel finders.
	/// </summary>
	public abstract class PixelFinderLayout : MonoBehaviour
	{
		[SerializeField] [HideInInspector] List<PixelFinder> _finders;

		public UnityAction<Texture2D> OnScreenShotRendered;

		internal abstract IEnumerable<FinderDirection> finderSetups { get; }

		public bool captureScreenShot { get; set; }

		/// <summary>
		///   returns true if all finders have completed their tasks
		/// </summary>
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

		/// <summary>
		///   Simple property for setting the name of a layout type
		/// </summary>
		protected virtual string layoutName => GetType().ToString().Split('.').LastOrDefault();

		/// <summary>
		///   Returns the count of how many finders are in the layout
		/// </summary>
		public int finderCount => finderSetups.Count();

		/// <summary>
		///   Sets the culling mask for all finders
		/// </summary>
		public int mask
		{
			set
			{
				foreach (var finder in _finders)
					finder.mask = value;
			}
		}

		/// <summary>
		///   list of all pixel finders in layout
		/// </summary>
		public List<PixelFinder> finders => _finders;

		/// <summary>
		///   Copies all the data from the finders in this layout
		/// </summary>
		public FinderLayoutDataContainer container => new(_finders, name);

		/// <summary>
		///   Destroys all finders and their gameobjects
		/// </summary>
		public void ClearFinders()
		{
			if (_finders != null && _finders.Any())
				for (var i = _finders.Count - 1; i >= 0; i--)
					Destroy(_finders[i].gameObject);

			_finders = new List<PixelFinder>(finderCount);
		}

		/// <summary>
		///   Resets all data containers to be empty
		/// </summary>
		/// <param name="collectionSize">collection size to set each finder at</param>
		public void ResetDataContainer(int collectionSize)
		{
			if (_finders != null && _finders.Any())
				foreach (var f in _finders)
					f.SetNewDataCollection(collectionSize);
		}

		/// <summary>
		///   Initialize the layout with finders
		/// </summary>
		/// <param name="collectionSize">Total collection size for data container</param>
		/// <param name="color">Color to look for</param>
		/// <param name="onDone">Optional hook to use when rendering is complete</param>
		public void Init(int collectionSize, Color32 color, Action onDone = null) => Init(collectionSize, new[] { color }, onDone);

		/// <summary>
		///   Initialize the layout with new finders
		/// </summary>
		/// <param name="collectionSize">Total collection size for data container</param>
		/// <param name="colors">Colors to look for</param>
		/// <param name="onDone">Optional hook to use when rendering is complete</param>
		public virtual void Init(int collectionSize, Color32[] colors, Action onDone = null)
		{
			ClearFinders();

			name = layoutName;

			var prefab = new GameObject().AddComponent<PixelFinder>();

			foreach (var s in finderSetups)
			{
				var finder = Instantiate(prefab, transform, true);
				finder.name = $"Finder[{s}]";

				finder.transform.localRotation = Quaternion.Euler(s switch
				{
					FinderDirection.Front => new Vector3(0, 0, 0),
					FinderDirection.Left => new Vector3(0, 90, 0),
					FinderDirection.Back => new Vector3(0, 180, 0),
					FinderDirection.Right => new Vector3(0, -90, 0),
					FinderDirection.Up => new Vector3(-90, 0, 0),
					FinderDirection.Down => new Vector3(90, 0, 0),
					_ => new Vector3(0, 0, 0)
				});

				finder.Init(colors, onDone ?? CheckFindersInLayout, collectionSize, finderCount);
				_finders.Add(finder);
			}

			Destroy(prefab.gameObject);
		}

		/// <summary>
		///   Starts the coroutine for each pixel finder in the layout at the selected index
		/// </summary>
		/// <param name="index">position to store data at</param>
		public void Run(int index)
		{
			foreach (var finder in _finders)
				StartCoroutine(finder.Render(index));
		}

		/// <summary>
		///   Triggers on complete event if all layouts are complete
		/// </summary>
		protected void CheckFindersInLayout()
		{
			if (allDone) onComplete?.Invoke();
		}

		public event UnityAction onComplete;
	}

}