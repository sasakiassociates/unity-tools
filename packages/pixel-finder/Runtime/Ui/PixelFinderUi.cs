using System.Collections.Generic;
using Sasaki.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PixelFinderUi : MonoBehaviour, ILinkableUi<PixelFinder>
{
  [SerializeField] GameObject header;
  [SerializeField] RawImage image;
  [SerializeField] TextMeshProUGUI titleText, valueText;

  List<TextMeshProUGUI> _counts;

  public void Link(PixelFinder obj)
  {
    if(obj == default) return;

    if(image != null)
      image.texture = obj.Texture;

    if(titleText != null)
      titleText.text = obj.Cam.gameObject.name;

    _counts = new List<TextMeshProUGUI>();

    valueText.color = obj.Colors[0];
    _counts.Add(valueText);

    for(int i = 1; i < obj.Colors.Length; i++)
    {
      var tc = Instantiate(valueText, header.transform);
      tc.color = obj.Colors[i];
      _counts.Add(tc);
    }


    obj.OnValueSet += values =>
    {
      // Debug.Log("Value set");
      for(int i = 0; i < values.Length; i++)
      {
        // Debug.Log(values[i]);
        _counts[i].text = values[i].ToString();
      }
    };
  }

}
