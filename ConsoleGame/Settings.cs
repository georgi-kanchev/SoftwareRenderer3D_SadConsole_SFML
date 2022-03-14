using SadConsole;
using SadConsole.UI;
using SadRogue.Primitives;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using Console = SadConsole.Console;

namespace ConsoleGame
{
   static class Settings
   {
      public enum Scene
      {
         Game, GraphicsViewer
      }

      public const int CELLS_WIDTH = 20, CELLS_HEIGHT = 11;

      public static RenderWindow Window { get; set; }
      public static Console Ground { get; set; }
      public static Console AboveGround { get; set; }
      public static Console UI { get; set; }
      public static Scene CurrentScene { get; set; }
      public static Vector2i MousePositionCell { get; private set; }

      private static void Main()
      {
         Window = new(new(1280, 704), "");
         Game.Create(CELLS_WIDTH, CELLS_HEIGHT, window: Window);

         Game.Instance.LoadFont("graphics.font");
         SadConsole.Settings.ResizeMode = SadConsole.Settings.WindowResizeOptions.Stretch;
         Game.Instance.OnStart = Init;
         Game.Instance.FrameUpdate += Update;

         Game.Instance.Run();
         Game.Instance.Dispose();
      }

		private static void Init()
      {
         Ground = (Console)GameHost.Instance.Screen;
         AboveGround = new(CELLS_WIDTH, CELLS_HEIGHT);
         InitLayerConsole(Ground);
         InitLayerConsole(AboveGround);

         UI = new(CELLS_WIDTH * 4, CELLS_HEIGHT);
         UI.FontSize = new(16, 32);

         Ground.Children.Add(AboveGround);
         Ground.Children.Add(UI);

         GraphicsViewer.Init();
      }
      private static void InitLayerConsole(Console console)
      {
         console.Font = Game.Instance.Fonts["graphics"];
         console.FontSize = new(64, 64);
      }
		private static void Update(object sender, GameHost e)
		{
         var mousePos = Mouse.GetPosition(Window);
         mousePos.X /= (int)Window.Size.X / CELLS_WIDTH;
         mousePos.Y /= (int)Window.Size.Y / CELLS_HEIGHT;
         MousePositionCell = mousePos;

         Ground.Clear();
         AboveGround.Clear();
         UI.Clear();
		}
   }
}
