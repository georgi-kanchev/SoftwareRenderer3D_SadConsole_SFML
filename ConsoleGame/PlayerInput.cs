using Dodo;
using SFML.System;
using SFML.Window;

namespace ConsoleGame
{
	public static class PlayerInput
	{
		public static void Init()
		{
			SadConsole.Game.Instance.FrameUpdate += OnUpdate;
		}

		private static void OnUpdate(object sender, SadConsole.GameHost e)
		{
			if (Settings.CurrentScene != Settings.Scene.Game)
				return;

			if (Keyboard.IsKeyPressed(Keyboard.Key.A).Once()) Move(new(-1, 0));
			if (Keyboard.IsKeyPressed(Keyboard.Key.D).Once()) Move(new(1, 0));
			if (Keyboard.IsKeyPressed(Keyboard.Key.W).Once()) Move(new(0, -1));
			if (Keyboard.IsKeyPressed(Keyboard.Key.S).Once()) Move(new(0, 1));

			void Move(Vector2i direction)
			{
				Game.Player.Move(direction);
				Game.AdvanceTime();
			}
		}
	}
}
