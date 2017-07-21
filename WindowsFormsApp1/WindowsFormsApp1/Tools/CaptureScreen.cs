using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace WindowsFormsApplication7 {
	public class CaptureScreen
	{
		public delegate void OnPrtScreen(Bitmap print);
		public event OnPrtScreen OnPrtScreenEvent;

		public Bitmap ScreenShot(Control control)
		{
			if(control == null)
				return null;

			if(control.Visible == false)
				return null;
			
			Rectangle rect = control.RectangleToScreen(control.Bounds);
			Bitmap bmp = new Bitmap(rect.Width - 25, rect.Height + 10, PixelFormat.Format32bppArgb);
			Graphics g = Graphics.FromImage(bmp);
			g.CopyFromScreen(control.Bounds.X, control.Bounds.Y, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
			OnPrtScreenEvent?.Invoke(bmp);
			return bmp;
		}
	}
}
