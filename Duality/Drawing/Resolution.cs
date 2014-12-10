using System.Collections;
using System.Collections.Generic;

namespace Duality.Drawing
{
	public struct Resolution
	{
		public int Width { get; set; }
		public int Height { get; set; }
		public float RefreshRate { get; set; }
	}

	public class ResolutionComparer : IEqualityComparer<Resolution>
	{
		public bool Equals(Resolution x, Resolution y)
		{
			return x.Width == y.Width && x.Height == y.Height && x.RefreshRate == y.RefreshRate;
		}

		public int GetHashCode(Resolution obj)
		{
			return obj.Width.GetHashCode() ^ obj.Height.GetHashCode() ^ obj.RefreshRate.GetHashCode();
		}
	}
}
