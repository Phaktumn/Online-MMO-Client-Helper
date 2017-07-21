using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Tools.Extensions {
	static class RectangleExtensions {
		public static Point Center(this Rectangle rect) {
			return new Point(rect.Left + rect.Width / 2,
							 rect.Top + rect.Height / 2);
		}
	}
}
