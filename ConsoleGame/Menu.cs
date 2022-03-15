using SadConsole;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadRogue.Primitives;
using SadConsole.UI.Themes;
using System.Collections.Generic;
using System;

namespace ConsoleGame
{
	public static class Menu
	{
		private static Window window;

		public static void Init()
		{
			var theme = new ButtonLinesTheme();
			var buttons = new List<Button>();

			CreateButton("PLAY");
			CreateButton("MAP EDITOR");
			CreateButton("SETTINGS");
			CreateButton("EXIT");

			InitWindow();

			window.Controls[0].MouseButtonClicked += OnPlay;
			window.Controls[^1].MouseButtonClicked += OnExit;

			void CreateButton(string text)
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

		public static void Show()
		{
			window.Show();
		}
		private static void OnPlay(object sender, ControlBase.ControlMouseState e)
		{
			Settings.CurrentScene = Settings.Scene.Game;
			window.Hide();
			Game.WindowUI.Show();
		}
		private static void OnExit(object sender, ControlBase.ControlMouseState e)
		{
			Settings.Window.Close();
		}
	}
}
