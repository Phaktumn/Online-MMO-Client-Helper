using System.Windows.Forms;

namespace WindowsFormsApp1.Tools.Extensions {
	static class FormControllerExtension {

		public static void ToggleVisible(this Control control)
		{
			control.Visible = !control.Visible;
		}

		public static void ToggleActive(this Control control)
		{
			control.Enabled = !control.Enabled;
		}
	}
}
