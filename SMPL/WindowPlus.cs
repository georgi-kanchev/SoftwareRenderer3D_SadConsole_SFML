using SadConsole;
using SadConsole.UI;

namespace SMPL
{
	public class WindowPlus : Window
	{
		public WindowPlus(int w, int h, string title = "Window", HorizontalAlignment alignment = default) : base(w, h)
		{
			Title = title;
			TitleAlignment = alignment;
		}
	}
}
