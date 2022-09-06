using System.Collections.Generic;
using System.Linq;
using Sasaki.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PixelFinderUi : MonoBehaviour
{

	[SerializeField] GameObject _header;

	[SerializeField] RawImage _image;

	[SerializeField] TextMeshProUGUI _title;

	List<TextMeshProUGUI> _counts;

	public void Link(IPixelFinder obj)
	{
		if (obj == default) return;

		if (_image != null)
			_image.texture = obj.texture;

		if (_title != null)
			_title.text = obj.cam.gameObject.name;

		_counts = new List<TextMeshProUGUI>();

		var countPrefab = new GameObject().AddComponent<TextMeshProUGUI>();
		countPrefab.fontSize = 8;
		countPrefab.alignment = TextAlignmentOptions.MidlineLeft;
		countPrefab.text = "0";
		countPrefab.rectTransform.sizeDelta = new Vector2(100, 8);

		foreach (var c in obj.colors)
		{
			var tc = Instantiate(countPrefab, _header.transform);
			tc.color = c;
			_counts.Add(tc);
		}
		
		Destroy(countPrefab.gameObject);

		obj.onValueSet += values =>
		{
			for (int i = 0; i < values.Length; i++)
				_counts[i].text = values[i].ToString();
		};
	}

}