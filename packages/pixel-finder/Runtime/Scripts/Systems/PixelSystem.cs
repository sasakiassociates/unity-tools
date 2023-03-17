using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Sasaki.Unity
{

  public abstract class PixelSystem : MonoBehaviour, IPixelSystem
  {
    [SerializeField] List<PixelLayout> _layouts;

    [SerializeField][HideInInspector] Vector3[] _points;

    [SerializeField][HideInInspector] int _index;

    IPixelSystemDataContainer _data;

    (bool queued, bool autoRun) _is;

    int _nextIndex;

    /// <summary>
    /// <inheritdoc />
    /// <para>Triggers <see cref="OnAutoRun"/> if value is different.</para>
    /// </summary>
    public bool AutoRun
    {
      get => _is.autoRun;
      protected set
      {
        if(_is.autoRun == value) return;

        _is.autoRun = value;
        OnAutoRun?.Invoke(value);
      }
    }

    /// <summary>
    /// Stored value for knowing if there is another point to look for 
    /// </summary>
    public bool IsQueued
    {
      get => _is.queued;
      protected set { _is.queued = value; }
    }

    /// <summary>
    /// <inheritdoc />
    ///  Returns true if all layouts have completed their tasks
    /// </summary>
    public bool WorkComplete
    {
      get
      {
        foreach(var l in _layouts)
          if(!l.WorkComplete)
            return false;

        return true;
      }
    }

    /// <inheritdoc />
    public string SystemName
    {
      get => GetType().ToString().Split('.').LastOrDefault();
    }

    /// <inheritdoc />
    public int CollectionSize
    {
      get => Points?.Length ?? 0;
    }

    /// <inheritdoc />
    public Vector3[] Points
    {
      get => _points;
      protected set => _points = value;
    }

    /// <inheritdoc />
    public List<PixelLayout> Layouts
    {
      get => _layouts;
      protected set => _layouts = value;
    }

    /// <inheritdoc />
    public int PointIndex
    {
      get => _index;
      protected set => _index = value;
    }

    /// <summary>
    /// <inheritdoc />
    /// <para>Triggers <see cref="OnDataGathered"/> event</para>
    /// </summary>
    public IPixelSystemDataContainer Data
    {
      get => _data;
      protected set
      {
        _data = value;
        OnDataGathered?.Invoke(_data);
      }
    }

    /// <inheritdoc />
    public int Mask
    {
      set => _layouts.ForEach(x => x.Mask = value);
    }

    /// <inheritdoc />
    public void Run()
    {
      AutoRun = true;
      PointIndex = 0;
      StartWork();
    }

    /// <inheritdoc />
    public void Capture(int pointIndex)
    {
      AutoRun = false;

      if(!WorkComplete)
      {
        Debug.Log("Is working still, will store this index for next capture");
        IsQueued = true;
        _nextIndex = pointIndex;
        return;
      }

      if(_points != null && _points.Length > Mathf.Max(0, pointIndex))
      {
        Debug.Log("Running Capture");
        PointIndex = pointIndex;
        StartWork();
      }
      else
      {
        Debug.Log("Did not run capture correctly");
      }
    }

    /// <summary>
    /// Initializes a new system with layouts. Stores the points internally and passes the colors into the finders
    /// </summary>
    /// <param name="systemPoints"></param>
    /// <param name="color"></param>
    /// <param name="inputLayouts"></param>
    public void Init(Vector3[] systemPoints, Color32 color, List<PixelLayout> inputLayouts = null)
    {
      Init(systemPoints, new[] {color}, inputLayouts);
    }

    /// <inheritdoc />
    public void Init(Vector3[] systemPoints, Color32[] colors, List<PixelLayout> inputLayouts = null)
    {
      if(inputLayouts != null && inputLayouts.Any())
      {
        ClearLayouts();
        _layouts = inputLayouts;
      }

      name = SystemName;
      Points = systemPoints;
      AutoRun = false;

      foreach(var layout in _layouts)
      {
        layout.Init(_points.Length, colors);
        layout.transform.SetParent(transform);
        layout.OnComplete += CheckLayoutsInSystem;
        OnLayoutAdded?.Invoke(layout);
      }
    }

    /// <inheritdoc />
    public virtual Dictionary<string, int> GetMaskLayers() => new Dictionary<string, int>();

    /// <inheritdoc />
    public void SetFrameRate(int value)
    {
      Application.targetFrameRate = value;
      OnFrameRateSet?.Invoke(value);
    }

    public void MoveToPoint(int index)
    {
      if(_points != null && _points.Length > Mathf.Max(0, index))
      {
        PointIndex = index;
        transform.position = _points[PointIndex];
      }
    }

    /// <summary>
    /// <para>Main system call to set a new <see cref="UnityEngine.Transform.position"/> for the system game object and triggers a new run for each object in <see cref="Layouts"/>.</para>
    /// <para>Also is the method that handles <see cref="PreMoveRender"/> and <see cref="PostMoveRender"/> render calls</para>
    /// </summary>
    protected void StartWork()
    {
      PreMoveRender();

      transform.position = Points[PointIndex];

      foreach(var layout in _layouts)
      {
        layout.Run(PointIndex);
      }

      PostMoveRender();
    }

    protected virtual void ResetSystem()
    {
      AutoRun = false;
      PointIndex = 0;
      ResetDataContainer();
    }

    /// <summary>
    ///   Clears all active layouts in the system
    /// </summary>
    public void ClearLayouts()
    {
      if(_layouts != null && _layouts.Any())
        for(var i = _layouts.Count - 1; i >= 0; i--)
          Destroy(_layouts[i].gameObject);
    }

    /// <summary>
    ///   Method to implement any logic after the system has moved
    /// </summary>
    protected virtual void PostMoveRender()
    { }

    /// <summary>
    ///   Method to implement any logic before the system has moved
    /// </summary>
    protected virtual void PreMoveRender()
    { }

    /// <summary>
    /// <para>Calls on complete and compiles the data into a container.</para>
    /// </summary>
    protected virtual IPixelSystemDataContainer GatherSystemData()
    {
      return new PixelSystemData(this);
    }

    protected virtual bool ShouldSystemRunAgain()
    {
      return true;
    }

    /// <summary>
    /// <para>Clears all values and creates a new collection for the size of the current point count.</para>
    /// </summary>
    protected void ResetDataContainer()
    {
      Debug.Log($"{name} is resetting data");

      if(Layouts != null && Layouts.Any())
      {
        foreach(var f in Layouts)
        {
          f.SetCollectionSize(CollectionSize);
        }
      }
    }

    /// <summary>
    ///   Checks if all layouts are complete and if there are more points to gather from
    /// </summary>
    void CheckLayoutsInSystem()
    {
      
      if(!WorkComplete) return;

      OnPointComplete?.Invoke(PointIndex);

      // if we are in auto run we check our points and move on
      if(AutoRun)
      {
        PointIndex++;
        if(PointIndex >= Points.Length)
        {
          Data = GatherSystemData();

          if(ShouldSystemRunAgain())
          {
            Debug.Log("Running System again");
            Run();
          }
          else
          {
            ResetSystem();
            OnComplete?.Invoke();
          }
        }
        else
        {
          StartWork();
        }

        return;
      }

      Debug.Log("Only doing capture call ");

      var args = CreateArgs();
      
      OnCapture?.Invoke(args);
      // if we move manually we send the data back at each point\
      if(IsQueued)
      {
        IsQueued = false;
        Debug.Log($"Getting Point in queue({_nextIndex})");
        Capture(_nextIndex);
      }
    }

    SystemCaptureArgs CreateArgs()
    {
      return new SystemCaptureArgs(
        _layouts
          .Select(x => new LayoutCaptureArgs(x.Finders
            .Select(f => new FinderCaptureArgs(f.name, _index, f.PixelData
              .Select(data => (int)data).ToArray()))
            .ToList(), x.name))
          .ToList(), name
      );
    }

    /// <inheritdoc />
    public event UnityAction<SystemCaptureArgs> OnCapture;

    /// <inheritdoc />
    public event UnityAction<IPixelLayout> OnLayoutAdded;

    /// <inheritdoc />
    public event UnityAction<IPixelSystemDataContainer> OnDataGathered;

    /// <inheritdoc />
    public event UnityAction OnComplete;

    /// <inheritdoc />
    public event UnityAction<int> OnPointComplete;

    /// <inheritdoc />
    public event UnityAction<int> OnFrameRateSet;

    /// <inheritdoc />
    public event UnityAction<bool> OnAutoRun;
  }

}
