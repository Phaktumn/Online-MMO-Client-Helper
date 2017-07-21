using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Tools.Extensions {
	static class PointExtension {
		
		public static float Distance(this Point from, Point to)
		{
			var dist = (float) Math.Sqrt(Math.Pow(to.X - from.X, 2) + Math.Pow(to.Y - from.Y, 2));
			return Math.Abs(dist);
		}

		public static float Magnitude(this Point point)
		{
			return point.Distance(new Point(0, 0));
		}
	}
}
