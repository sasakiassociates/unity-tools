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
  public abstract class PixelLayout : MonoBehaviour, IPixelLayout
  {
    [SerializeField][HideInInspector] List<PixelFinder> _finders;

    internal abstract IEnumerable<FinderDirection> finderSetups { get; }

    public bool captureScreenShot { get; set; }

    /// <inheritdoc />
    public bool WorkComplete
    {
      get
      {
        foreach(var l in _finders)
          if(!l.WorkComplete)
            return false;

        return true;
      }
    }

    /// <inheritdoc />
    public virtual string LayoutName
    {
      get => GetType().ToString().Split('.').LastOrDefault();
    }

    /// <inheritdoc />
    public int FinderCount
    {
      get => finderSetups?.Count() ?? 0;
    }

    /// <inheritdoc />
    public int ColorCount { get; protected set; }

    /// <inheritdoc />
    public int CollectionSize
    {
      get => Data.Size();
    }

    /// <inheritdoc />
    public int PointIndex { get; protected set; }

    /// <summary>
    ///   Copies all the data from the finders in this layout
    /// </summary>
    public PixelDataContainer Data { get; protected set; }

    /// <summary>
    ///   Sets the culling mask for all finders
    /// </summary>
    public int Mask
    {
      set
      {
        foreach(var finder in _finders)
        {
          finder.Mask = value;
        }
      }
    }

    /// <summary>
    ///   list of all pixel finders in layout
    /// </summary>
    public List<PixelFinder> Finders
    {
      get => _finders;
    }

    /// <summary>
    ///   Destroys all finders and their <see cref="GameObject"/>
    /// </summary>
    public void ClearFinders()
    {
      if(_finders != null && _finders.Any())
      {
        for(var i = _finders.Count - 1; i >= 0; i--)
        {
          Destroy(_finders[i].gameObject);
        }
      }

      _finders = new List<PixelFinder>(FinderCount);
    }

    /// <summary>
    ///   Resets all data containers to be empty
    /// </summary>
    /// <param name="collectionSize">collection size to set each finder at</param>
    public void SetCollectionSize(int collectionSize)
    {
      Data = new PixelDataContainer(collectionSize);
    }

    /// <summary>
    ///   Initialize the layout with finders
    /// </summary>
    /// <param name="collectionSize">Total collection size for data container</param>
    /// <param name="color">Color to look for</param>
    /// <param name="onDone">Optional hook to use when rendering is complete</param>
    public void Init(int collectionSize, Color32 color, UnityAction onDone = null)
    {
      Init(collectionSize, new[] {color}, onDone);
    }

    /// <summary>
    ///   Initialize the layout with new finders
    /// </summary>
    /// <param name="collectionSize">Total collection size for data container</param>
    /// <param name="colors">Colors to look for</param>
    /// <param name="onDone">Optional hook to use when rendering is complete</param>
    public virtual void Init(int collectionSize, Color32[] colors, UnityAction onDone = null)
    {
      ClearFinders();
      name = LayoutName;

      ColorCount = colors?.Length ?? 0;
      Data = new PixelDataContainer(collectionSize);
      OnComplete = onDone;

      var prefab = new GameObject().AddComponent<PixelFinder>();

      foreach(var s in finderSetups)
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

        finder.Init(colors, onDone ?? CheckFindersInLayout, FinderCount);
        _finders.Add(finder);
      }

      SetCollectionSize(collectionSize);

      Destroy(prefab.gameObject);
    }


    /// <inheritdoc />
    public void Run(int index)
    {
      PointIndex = index;

      _finders.ForEach(x => x.PreRender());

      foreach(var finder in _finders)
      {
        StartCoroutine(finder.Render());
      }
    }


    public void OnDestroy()
    {
      for(int i = _finders.Count - 1; i > 0; i--)
      {
        Finders[i].StopAllCoroutines();
        Destroy(Finders[i].gameObject);
      }
    }

    /// <summary>
    ///   Triggers on complete event if all layouts are complete
    /// </summary>
    protected void CheckFindersInLayout()
    {
      if(!WorkComplete)
        return;

      var combinedData = new uint[ColorCount];


      foreach(var finder in _finders)
      {
        for(int i = 0; i < finder.PixelData.Length; i++)
        {
          combinedData[i] += finder.PixelData[i];
        }
      }


      // TODO: Figure out where this should live, or just annotate it properly so it has some info to the user 
      // TODO: Or just move this into a compute shader since the layout system should handle all the gpu callbacks instead of a the finder itself
      var modifier = 1.0f / FinderCount;
      for(var i = 0; i < combinedData.Length; i++)
      {
        var rawCombinedValue = combinedData[i];
        // NOTE: this is casted back to 
        combinedData[i] = (uint)(rawCombinedValue * modifier);

      }

      Data.Set(combinedData, PointIndex);

      OnComplete?.Invoke();
    }

    /// <inheritdoc />
    public event UnityAction OnComplete;

    /// <inheritdoc />
    public event UnityAction<LayoutCaptureArgs> OnCapture;

    /// <inheritdoc />
    public event UnityAction<LayoutScreenCaptureArgs> OnScreenCapture;

  }

}
