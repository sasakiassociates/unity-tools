using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ViewObjects;

namespace Sasaki.Unity.Ui
{
	public class PixelFinderSystemUi : MonoBehaviour, ILinkableUi<IPixelSystem>
	{
		[SerializeField] Slider _pointPositionSlider;
		[SerializeField] TMP_Dropdown _dropdownMask;
		[SerializeField] PixelFinderLayoutUi _layoutUi;
		[SerializeField] ValueGroupUi _valueGroupUi;
		[SerializeField] Button _runButton;
		[SerializeField] TMP_InputField _inputField;
		[SerializeField] Slider _frameRateSlider;

		Dictionary<string, int> _maskValues;

		public void Link(IPixelSystem obj)
		{
			if (obj == default) return;

			// obj.OnAutoRun += isAutoRunning =>
			// {
			// 	if (_inputField != null) _inputField.interactable = !isAutoRunning;
			// 	if (_dropdownMask != null) _dropdownMask.interactable = !isAutoRunning;
			// 	if (_pointPositionSlider != null) _pointPositionSlider.interactable = !isAutoRunning;
			// };
			//
			// if (_layoutUi != null)
			// {
			// 	// TODO: Handle updating layout being shown
			// 	_layoutUi.Link(obj.Layouts.FirstOrDefault());
			// }
			//
			// if (_inputField != null)
			// {
			// 	obj.OnPointComplete += v => _inputField.SetTextWithoutNotify(v.ToString());
			//
			// 	_inputField.interactable = obj.AutoRun;
			//
			// 	_inputField.onValueChanged.AddListener((input) =>
			// 	{
			// 		if (int.TryParse(input, out int value))
			// 		{
			// 			obj.Capture(value);
			// 		}
			// 	});
			// }
			//
			// if (_frameRateSlider != null)
			// {
			// 	_frameRateSlider.wholeNumbers = true;
			// 	_frameRateSlider.onValueChanged.AddListener(value => obj.SetFrameRate((int)value));
			// 	obj.OnFrameRateSet += value => _frameRateSlider.SetValueWithoutNotify(value);
			// }
			//
			// if (_pointPositionSlider != null)
			// {
			// 	_pointPositionSlider.interactable = obj.AutoRun;
			//
			// 	_pointPositionSlider.wholeNumbers = true;
			// 	_pointPositionSlider.maxValue = obj.Points.Valid() ? obj.Points.Length : 1;
			// 	_pointPositionSlider.minValue = 0;
			// 	_pointPositionSlider.onValueChanged.AddListener(value => obj.Capture((int)value));
			// }
			//
			// if (_dropdownMask != null)
			// {
			// 	_dropdownMask.interactable = obj.AutoRun;
			//
			// 	_maskValues = obj.GetMaskLayers();
			//
			// 	_dropdownMask.ClearOptions();
			// 	_dropdownMask.AddOptions(_maskValues.Keys.ToList());
			// 	_dropdownMask.onValueChanged.AddListener(value =>
			// 	{
			// 		obj.Mask = _maskValues[_dropdownMask.options[value].text];
			// 		obj.Capture(obj.PointIndex);
			// 	});
			// }
			//
			// if (_valueGroupUi != null)
			// {
			// 	var coll = new HashSet<Color32>();
			// 	foreach (var finder in obj.Layouts.FirstOrDefault().Finders)
			// 	foreach (var c in finder.Colors)
			// 		coll.Add(c);
			//
			// 	_valueGroupUi.CreateGroup(coll.ToList(), obj.Layouts.FirstOrDefault().name);
			//
			// 	obj.OnCapture += sys =>
			// 	{
			// 		var valuesPerColor = new List<int>();
			//
			// 		foreach (var layoutArg in sys.args)
			// 		{
			// 			foreach (var finderArg in layoutArg.finderArgs)
			// 			{
			// 				if (finderArg.values == null)
			// 					continue;
			//
			// 				// raw data from finder
			// 				for (var i = 0; i < finderArg.values.Length; i++)
			// 				{
			// 					var value = finderArg.values[i];
			//
			// 					if (valuesPerColor.Count <= i)
			// 						valuesPerColor.Add(value);
			// 					else
			// 						valuesPerColor[i] += value;
			// 				}
			// 			}
			// 		}
			//
			// 		_valueGroupUi.SetText(valuesPerColor.Select(x => x.ToString()).ToList());
			// 	};
			// }
			//
			// if (_runButton != null)
			// {
			// 	_runButton.onClick.AddListener(obj.Run);
			// }
		}

	}

}