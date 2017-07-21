using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1.Tools {
	class Mouse 
	{
		[DllImport("user32.dll")]
		static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData,
			int dwExtraInfo);

		private enum MouseEventFlags : uint 
		{
			LEFTDOWN = 0x00000002,
			LEFTUP = 0x00000004,
			MIDDLEDOWN = 0x00000020,
			MIDDLEUP = 0x00000040,
			MOVE = 0x00000001,
			ABSOLUTE = 0x00008000,
			RIGHTDOWN = 0x00000008,
			RIGHTUP = 0x00000010,
			WHEEL = 0x00000800,
			XDOWN = 0x00000080,
			XUP = 0x00000100
		}

		//public static void SimulateMouseClick(uint x, uint y)
		//{
		//	Task.Factory.StartNew(() =>
		//	{
		//		mouse_event((uint)MouseEventFlags.LEFTDOWN, x, y, 0, 0);
		//		Thread.Sleep(new Random().Next(20, 30));
		//		mouse_event((uint)MouseEventFlags.LEFTUP, x, y, 0, 0);
		//	});
		//}

		[DllImport("user32.dll", EntryPoint = "WindowFromPoint",
			CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern IntPtr WindowFromPoint(Point point);


		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, IntPtr windowTitle);

		[DllImport("user32.dll", SetLastError = true)]
		static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
		public enum WMessages : int {
			WM_LBUTTONDOWN = 0x201,
			WM_LBUTTONUP = 0x202
		}
		private const int BM_CLICK = 0xF5;

		private static int MAKELPARAM(int x, int y) {
			return ( ( y << 16 ) | ( x & 0xffff ) );
		}

		public static void DoMouseLeftClick(IntPtr handle, Point x) 
		{
			if (handle == IntPtr.Zero)
			{
				Console.Write(handle.ToString());
				return;
			}
			PostMessage(handle, (uint)WMessages.WM_LBUTTONDOWN, 0, MAKELPARAM(x.X, x.Y));
			Thread.Sleep(10);
			PostMessage(handle, (uint)WMessages.WM_LBUTTONUP, 0, MAKELPARAM(x.X, x.Y));
		}

		public static IntPtr Flash(WebBrowser webBrowser1) 
		{
			var pControl = FindWindowEx(webBrowser1.Handle, IntPtr.Zero, "Shell Embedding", IntPtr.Zero);
			pControl = FindWindowEx(pControl, IntPtr.Zero, "Shell DocObject View", IntPtr.Zero);
			pControl = FindWindowEx(pControl, IntPtr.Zero, "Internet Explorer_Server", IntPtr.Zero);
			pControl = FindWindowEx(pControl, IntPtr.Zero, "MacromediaFlashPlayerActiveX", IntPtr.Zero);
			return pControl;
		}
	}
}
