using SadConsole;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadRogue.Primitives;
using SadConsole.UI.Themes;
using System.Collections.Generic;

namespace ConsoleGame
{
	public static class Menu
	{
		private static Window window;

		public static void Init()
		{
			var theme = new ButtonLinesTheme();
			var buttons = new List<Button>();

			AddButton("PLAY");
			AddButton("MAP EDITOR");
			AddButton("SETTINGS");
			AddButton("EXIT");

			InitWindow();

			window.Controls[3].MouseButtonClicked += OnExit;

			void AddButton(string text)
			{
				buttons.Add(new Button(16, 3) { Text = text, Position = new(2, buttons.Count * 3 + 1), Theme = theme, CanFocus = false });
			}
			void InitWindow()
			{
				window = new(20, buttons.Count * 3 + 2) { CanDrag = false, Title = "" };
				window.Center();
				window.Show();
				window.BorderLineStyle = ICellSurface.ConnectedLineThick;

				for (int i = 0; i < buttons.Count; i++)
					window.Controls.Add(buttons[i]);
			}
		}

		private static void OnExit(object sender, ControlBase.ControlMouseState e)
		{
			Settings.Window.Close();
		}
	}
}
