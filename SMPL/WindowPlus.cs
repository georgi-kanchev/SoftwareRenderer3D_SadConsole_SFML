using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadRogue.Primitives;
using System;

namespace SMPL
{
	public class WindowPlus : Window
	{
		public ListBox List { get; set; }
		public Button ButtonClose { get; set; }
		public Button ButtonClear { get; set; }
		private Keys hotkey;
		public Keys ToggleWindowHotkey
		{
			get => hotkey;
			set
			{
				hotkey = value;
				List.TryAddStringNewLine($"Press [{hotkey}] to hide or show this window.", true);
			}
		}

		public WindowPlus(int w, int h, Keys toggleKey) : base(w, h)
		{
			List = new(w - 4, h - 2) { Position = new(2, 1) };
			ButtonClose = new(3) { Text = "X", Position = new(w - 4, 0) };
			ButtonClear = new(7) { Text = "Clear", Position = new(w - 12, 0) };
			Title = " Window ";
			TitleAlignment = HorizontalAlignment.Left;

			ButtonClose.Click += CloseLogs;
			ButtonClear.Click += ClearLogs;
			PositionChanged += OnDragged;
			Game.Instance.FrameUpdate += OnUpdate;
			Simple.RenderWindowSFML.Resized += OnWindowResize;
			List.SelectedItemExecuted += OnLogClick;

			Controls.Add(ButtonClose);
			Controls.Add(ButtonClear);
			Controls.Add(List);

			ToggleWindowHotkey = toggleKey;
			List.TryAddStringNewLine("Double-click [LMB] on a line to remove it.", true);
			List.Items.Add("");
			Show();
		}

		private void OnWindowResize(object sender, SFML.Window.SizeEventArgs e) => this.KeepInConsole(Simple.Console);
		private void OnLogClick(object sender, ListBox.SelectedItemEventArgs e) => List.Items.Remove(e.Item);
		private void OnDragged(object sender, ValueChangedEventArgs<Point> e) => this.KeepInConsole(Simple.Console);
		private void CloseLogs(object sender, EventArgs e) => Hide();
		private void ClearLogs(object sender, EventArgs e) => List.Items.Clear();
		private void OnUpdate(object sender, GameHost e)
		{
			if (Game.Instance.Keyboard.IsKeyPressed(ToggleWindowHotkey))
				IsVisible = IsVisible == false;
		}
	}
}
