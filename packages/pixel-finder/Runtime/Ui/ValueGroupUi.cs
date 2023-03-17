using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ValueGroupUi : MonoBehaviour
{

	[SerializeField] GameObject _header;
	[SerializeField] TextMeshProUGUI _titleText,_valueText;
	List<TextMeshProUGUI> _counts;


	public void CreateGroup(List<Color32> colors, string title)
	{
		_titleText.text = title;
		_counts = new List<TextMeshProUGUI>();
				
		_valueText.color = colors[0];
		_counts.Add(_valueText);
		
		for (int i = 1; i < colors.Count; i++)
		{
			var tc = Instantiate(_valueText, _header.transform);
			tc.color = colors[i];
			_counts.Add(tc);
		}

	}
	
	public void SetText(List<string> values)
	{
		for (int i = 0; i < values.Count; i++)
			_counts[i].text = values[i];
	}
}