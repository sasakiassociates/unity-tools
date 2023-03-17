using System;
using UnityEngine;

namespace Sasaki.Unity
{

  [ExecuteAlways]
  public class PixelFinderDebugger : MonoBehaviour
  {
    [SerializeField] PixelFinder finder;
    [SerializeField] Material material;
    [SerializeField] Gradient gradient;


    [SerializeField] RenderTexture texture;
    static readonly int MainTex = Shader.PropertyToID("_MainTex");


    void OnGUI()
    {
      const int width = 200;
      GUIStyle style = new GUIStyle(GUI.skin.button)
      {
        fontSize = 10
      };
      if(GUI.Button(new Rect(10, 10, width, 20), nameof(RenderDebugTexture), style)) RenderDebugTexture();
      if(GUI.Button(new Rect(10, 30, width, 20), nameof(LogDebugValues), style)) LogDebugValues();
    }


    void RenderDebugTexture()
    {
      texture = finder.DebugView(gradient);
      material.SetTexture(MainTex, texture);
    }

    void LogDebugValues()
    {
      finder.DebugViewValues();
    }

  }

}
