using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Sasaki.Unity.Ui
{
  public class SimpleBar : MonoBehaviour
  {
    [SerializeField] private Image bar;
    [SerializeField] private TextMeshProUGUI label;

    public int size
    {
      set
      {
        if (_rt != null) _rt.sizeDelta = new Vector2(value, 0);
      }
    }

    public string text
    {
      set
      {
        if (label != null)
        {
          label.text = value;
          // TODO: add in back up text or something spiffy like this 
          // if (label.isTextOverflowing)
          // {
          //   //
          // }
        }
      }
    }

    public Color barColor
    {
      set
      {
        if (bar != null) bar.color = value;
      }
    }

    public bool showText
    {
      set
      {
        if (label != null) label.enabled = value;
      }
    }

    private RectTransform _rt;

    public void Awake()
    {
      _rt = gameObject.GetComponent<RectTransform>();
      
      
    }
  }
}