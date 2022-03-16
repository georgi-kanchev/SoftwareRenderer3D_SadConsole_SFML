using SadConsole;
using SadConsole.UI.Themes;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Console = SadConsole.Console;

namespace ConsoleGame
{
   static class Settings
   {
      public enum Scene
      {
         Menu, Game, MapEditor, GraphicsViewer
      }

      public const int CELLS_WIDTH = 20, CELLS_HEIGHT = 11;

      public static RenderWindow Window { get; set; }

      public static Console Ground { get; set; }
      public static Console AboveGround { get; set; }
      public static Console Units { get; set; }
      public static Console AboveUnits { get; set; }
      public static Console UI { get; set; }


      public static Scene CurrentScene { get; set; }
      public static Vector2i MousePositionCell { get; private set; }

      private static void Main()
      {
         Window = new(new(1280, 704), "");
         SadConsole.Game.Create(CELLS_WIDTH, CELLS_HEIGHT, window: Window);

         SadConsole.Game.Instance.LoadFont("graphics.font");
         SadConsole.Settings.ResizeMode = SadConsole.Settings.WindowResizeOptions.Stretch;
         SadConsole.Game.Instance.OnStart = Init;
         SadConsole.Game.Instance.FrameUpdate += Update;

         SadConsole.Game.Instance.Run();
         SadConsole.Game.Instance.Dispose();
      }

		private static void Init()
      {
         Ground = (Console)GameHost.Instance.Screen;
         AboveGround = new(CELLS_WIDTH, CELLS_HEIGHT);
         Units = new(CELLS_WIDTH, CELLS_HEIGHT);
         AboveUnits = new(CELLS_WIDTH, CELLS_HEIGHT);
         InitLayerConsole(Ground);
         InitLayerConsole(AboveGround);
         InitLayerConsole(Units);
         InitLayerConsole(AboveUnits);

         UI = new(CELLS_WIDTH * 4, CELLS_HEIGHT);
         UI.FontSize = new(16, 32);

         Ground.Children.Add(AboveGround);
         Ground.Children.Add(Units);
         Ground.Children.Add(AboveUnits);
         Ground.Children.Add(UI);

         Menu.Init();
         Game.Init();
         GraphicsViewer.Init();
         PlayerInput.Init();
      }
      private static void InitLayerConsole(Console console)
      {
         console.Font = SadConsole.Game.Instance.Fonts["graphics"];
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
         Units.Clear();
         AboveUnits.Clear();
         UI.Clear();
		}
   }
}
