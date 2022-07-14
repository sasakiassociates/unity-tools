using UnityEngine;

namespace Sasaki
{
	public static partial class SasakiUtils
	{ 
		public static int ToHex(this Color32 c) => (c.r << 16) | (c.g << 8) | c.b;

		public static int ToHex(this Color color)
		{
			Color32 c = color;
			return c.ToHex();
		}

		public static Color32 ToUnityColor32(this int c) =>
			// From integer 
			new((byte)(c >> 24),
			    (byte)(c >> 16),
			    (byte)(c >> 8),
			    (byte)c);}
}