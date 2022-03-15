using SadConsole;
using SadConsole.UI;
using System.Collections.Generic;

namespace ConsoleGame
{
	public static class Game
	{
		private static List<List<int>> map;
		public static Window WindowUI { get; private set; }

		public const int UI_WIDTH = 4;

		public static void Init()
		{
			SadConsole.Game.Instance.FrameUpdate += Update;
			WindowUI = new(32, 44) { CanDrag = false, Title = "" };
			WindowUI.BorderLineStyle = ICellSurface.ConnectedLineThick;

			map = new();
			for (int x = 0; x < Settings.CELLS_WIDTH - UI_WIDTH; x++)
			{
				map.Add(new());
				for (int y = 0; y < Settings.CELLS_HEIGHT; y++)
					map[x].Add(5);
			}
		}
		private static void Update(object sender, GameHost e)
		{
			if (Settings.CurrentScene != Settings.Scene.Game)
				return;

			for (int i = 0; i < map.Count; i++)
				for (int j = 0; j < map[i].Count; j++)
					Settings.Ground.SetGlyph(i + UI_WIDTH, j, map[i][j]);
		}
	}
}
