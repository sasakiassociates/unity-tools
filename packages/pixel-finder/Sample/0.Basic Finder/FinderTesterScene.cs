using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sasaki.Unity
{
	public enum FinderStyle
	{
		Finder,
		Layout
	}

	public class FinderTesterScene : MonoBehaviour
	{
		[SerializeField] GameObject _container;

		[SerializeField] GameObject _uiPrefab;

		[SerializeField] GameObject _uiContainer;

		[SerializeField] FinderStyle _finderStyle = FinderStyle.Finder;

		Coroutine _finderRoutine;

		PixelFinder _finder;
		PixelLayoutCube _layout;
		PixelSystemBasic _systemBasic;

		List<Color32> _colors;

		void Start()
		{
			if (_container == null)
			{
				Debug.Log("No object was found that contained pixel finding colors :(. Make sure to have an object with meshes and materials");
				return;
			}

			_colors = new List<Color32>();

			foreach (Transform obj in _container.transform)
			{
				if (obj.gameObject.TryGetDiffuseColor(out var color))
					_colors.Add(color);
			}

			switch (_finderStyle)
			{
				case FinderStyle.Finder:
					CreateFinder();
					break;
				case FinderStyle.Layout:
					CreateLayout();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		void CreateLayout()
		{
			_layout = new GameObject().AddComponent<PixelLayoutCube>();
			// _layout.Init(_colors, );

			var ui = Instantiate(_uiPrefab, _uiContainer.transform);

			ui.GetComponent<PixelFinderUi>().Link(_finder);
		}

		void CreateFinder()
		{
			_finder = new GameObject("PixelFinder").AddComponent<PixelFinder>();
			_finder.Init(_colors.ToArray(), ClearRoutine);

			var ui = Instantiate(_uiPrefab, _uiContainer.transform);

			ui.GetComponent<PixelFinderUi>().Link(_finder);
		}

		void ClearRoutine() => _finderRoutine = null;

		void Update()
		{
			switch (_finderStyle)
			{
				case FinderStyle.Finder:
					_finderRoutine ??= StartCoroutine(_finder.Render());
					break;
				case FinderStyle.Layout:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

	}
}