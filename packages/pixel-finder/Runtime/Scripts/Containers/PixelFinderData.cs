using System;
using System.Linq;
using UnityEngine;

namespace Sasaki.Unity
{

  /// <summary>
  /// The main data object that stores the pixel counts 
  /// </summary>
  public struct PixelDataContainer
  {
    uint[][] _data;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="totalSize">The amount of data to record</param>
    public PixelDataContainer(int totalSize = 1) => _data = new uint[totalSize][];

    public PixelDataContainer(PixelDataContainer container)
    {
      _data = new uint[container._data.Length][];
      for(int pointIndex = 0; pointIndex < _data.Length; pointIndex++)
      {
        if(container._data[pointIndex] != null)
        {
          _data[pointIndex] = Enumerable.Select<uint, uint>(container._data[pointIndex], x => x).ToArray();
        }
      }
    }

    /// <summary>
    /// Copy a single set of values from the container
    /// This will cast all the pixel data from <see cref="uint"/> to <see cref="int"/>
    /// </summary>
    /// <param name="index">The data index to grab</param>
    /// <returns>Values casted as ints</returns>
    public int[] Copy(int index)
    {
      if(_data?.Length <= index)
        return null;

      var result = new int[_data[index].Length];

      for(var i = 0; i < result.Length; i++)
      {
        result[i] = (int)_data[index][i];
      }

      return result;
    }

    /// <summary>
    /// Copy a single set of values from the container
    /// This will cast all the pixel data from <see cref="uint"/> to <see cref="int"/>
    /// </summary>
    /// <param name="index">The data index to grab</param>
    /// <returns>Values casted as ints</returns>
    public uint[] CopyRaw(int index)
    {
      if(_data?.Length <= index)
        return null;

      var result = new uint[_data[index].Length];

      for(var i = 0; i < result.Length; i++)
      {
        result[i] = _data[index][i];
      }

      return result;
    }

    /// <summary>
    /// Gather all the data from the container
    /// This will cast all the pixel data from <see cref="uint"/> to <see cref="int"/>
    /// </summary>
    /// <returns>Values casted as ints</returns>
    public int[][] Get()
    {
      // TODO: Get cannot be called on all data if there is no data stored
      var casted = new int[_data.Length][];

      for(int pointIndex = 0; pointIndex < _data.Length; pointIndex++)
      {
        casted[pointIndex] = new int[_data[pointIndex].Length];

        for(int colorIndex = 0; colorIndex < _data[pointIndex].Length; colorIndex++)
        {
          casted[pointIndex][colorIndex] = (int)_data[pointIndex][colorIndex];
        }
      }

      return casted;
    }

    public uint[][] GetRaw()
    {
      var casted = new uint[_data.Length][];

      for(int pointIndex = 0; pointIndex < _data.Length; pointIndex++)
      {
        casted[pointIndex] = new uint[_data[pointIndex].Length];

        for(int colorIndex = 0; colorIndex < _data[pointIndex].Length; colorIndex++)
        {
          casted[pointIndex][colorIndex] = _data[pointIndex][colorIndex];
        }
      }

      return casted;
    }

    /// <summary>
    /// Gets one dimension of values from the collection
    /// </summary>
    /// <returns></returns>
    public int[] Get1d(int colorIndex)
    {
      var column = new int[_data.Length];

      for(int pointIndex = 0; pointIndex < _data.Length; pointIndex++)
      {
        // if(column[pointIndex] >= int.MaxValue)
        // {
        //   Debug.LogWarning($"({pointIndex}) is too big for int: {column[pointIndex]}");
        // }
        //
        if(_data[pointIndex] != null && _data[pointIndex].Length > colorIndex)
        {
          // TODO: Figure out a better way to handle the conversion of uint to int
          // NOTE: This seems to only happen when the data is combined from the finder to the layout
          column[pointIndex] = (int)Math.Clamp(_data[pointIndex][colorIndex], 0, int.MaxValue);
        }
      }

      return column;
    }

    /// <summary>
    /// Gets one dimension of values from the collection
    /// </summary>
    /// <returns></returns>
    public uint[] GetRaw1d(int dimension)
    {
      var casted = new uint[_data.Length];

      for(int pointIndex = 0; pointIndex < _data.Length; pointIndex++)
      {
        if(_data[pointIndex] != null && _data[pointIndex].Length > dimension)
        {
          casted[pointIndex] = _data[pointIndex][dimension];
        }
      }

      return casted;
    }

    /// <summary>
    /// Adds the values to the array 
    /// </summary>
    /// <param name="values">Raw values</param>
    /// <param name="index">Associated position</param>
    public void Add(uint[] values, int index)
    {
      if(_data[index] == null)
      {
        _data[index] = values;
        return;
      }

      for(int i = 0; i < values.Length; i++)
      {
        _data[index][i] += values[i];
      }

    }

    /// <summary>
    /// Get the size of the pixel container
    /// </summary>
    /// <returns></returns>
    public int Size() => _data?.Length ?? 0;

    /// <summary>
    /// Store the values from a pixel finder
    /// </summary>
    /// <param name="values">Raw values</param>
    /// <param name="index">Associated position</param>
    public void Set(uint[] values, int index = 0)
    {
      _data[index] = values;
    }

  }

}
