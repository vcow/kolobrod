using UnityEngine;

namespace Common
{
	public static class BoundsExtentions
	{
		public static bool Intersection(this Bounds b1, Bounds b2, out Bounds intersection)
		{
			var minx = Mathf.Max(b1.min.x, b2.min.x);
			var maxx = Mathf.Min(b1.max.x, b2.max.x);
			var miny = Mathf.Max(b1.min.y, b2.min.y);
			var maxy = Mathf.Min(b1.max.y, b2.max.y);
			var minz = Mathf.Max(b1.min.z, b2.min.z);
			var maxz = Mathf.Min(b1.max.z, b2.max.z);

			if (minx > maxx || miny > maxy || minz > maxz)
			{
				intersection = new Bounds();
				return false;
			}

			intersection = new Bounds {min = new Vector3(minx, miny, minz), max = new Vector3(maxx, maxy, maxz)};
			return true;
		}
	}
}