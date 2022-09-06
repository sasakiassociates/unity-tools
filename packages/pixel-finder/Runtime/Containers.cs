﻿using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Sasaki.Unity
{

	/// <summary>
	/// The main data object that stores the pixel counts 
	/// </summary>
	public readonly struct PixelDataContainer
	{
		readonly uint[][] _data;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="totalSize">The amount of data to record</param>
		public PixelDataContainer(int totalSize = 1) => _data = new uint[totalSize][];

		/// <summary>
		/// Store the values from a pixel finder
		/// </summary>
		/// <param name="values">Raw values</param>
		/// <param name="index">Associated position</param>
		public void Set(uint[] values, int index = 0) => _data[index] = values;

		/// <summary>
		/// Gather all the data from the container
		/// </summary>
		/// <returns></returns>
		public uint[][] Get() => _data;

		/// <summary>
		/// Copy a single set of values from the container
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public uint[] Copy(int index) => _data?.Length > index ? _data[index] : null;
		
		/// <summary>
		/// Get the size of the pixel container
		/// </summary>
		/// <returns></returns>
		public int Size() => _data?.Length ?? 0;

	}

	public readonly struct FinderLayoutDataContainer : IFinderLayoutData
	{
		public FinderLayoutDataContainer(IReadOnlyList<PixelFinder> finders, string name)
		{
			finderNames = null;
			data = null;
			this.name = name;

			if (finders == null || !finders.Any())
				return;

			finderNames = new string[finders.Count];
			data = new PixelDataContainer[finders.Count];

			for (var i = 0; i < finders.Count; i++)
			{
				finderNames[i] = finders[i].name;
				data[i] = finders[i].data;
			}
		}

		public string name { get; }

		/// <summary>
		///   Names of each finder
		/// </summary>
		public string[] finderNames { get; }

		/// <summary>
		///   Data of each finder
		/// </summary>
		public PixelDataContainer[] data { get; }
	}

	public readonly struct FinderSystemDataContainer : IFinderSystemData
	{
		public FinderSystemDataContainer(IReadOnlyList<PixelFinderLayout> finders, string name)
		{
			layoutNames = null;
			data = null;
			this.name = name;

			if (finders == null || !finders.Any())
				return;

			layoutNames = new string[finders.Count];
			data = new FinderLayoutDataContainer[finders.Count];

			for (var i = 0; i < finders.Count; i++)
			{
				layoutNames[i] = finders[i].name;
				data[i] = finders[i].container;
			}
		}

		public string name { get; }

		/// <summary>
		///   Names of different layouts
		/// </summary>
		public string[] layoutNames { get; }

		/// <summary>
		///   Data from each layout
		/// </summary>
		public FinderLayoutDataContainer[] data { get; }
	}

}