using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Sasaki.Unity.Ui
{
  public class SimpleToggle : MonoBehaviour
  {
    [SerializeField] private Image button;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private Color defaultColor = Color.gray;
    [SerializeField] private Color activeColor = Color.green;

    public bool Toggle
    {
      set
      {
        if (button != null)
        {
          var color = value ? activeColor : defaultColor;
          button.color = color;
        }
      }
    }

  }
}