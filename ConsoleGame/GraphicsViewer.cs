using SadConsole;
using SadRogue.Primitives;
using SFML.Window;
using static ConsoleGame.Settings;

namespace ConsoleGame
{
	public static class GraphicsViewer
	{
      public static int ScrollY { get; set; }

      public static void Init()
      {
         SadConsole.Game.Instance.FrameUpdate += Update;
         Settings.Window.MouseWheelScrolled += OnScroll;
      }

		private static void Update(object sender, GameHost e)
		{
         if (CurrentScene != Scene.GraphicsViewer)
            return;

         var mousePos = MousePositionCell;
         var display = $"ID: {mousePos.X + Ground.Width * (mousePos.Y + ScrollY)}";

         DrawGraphics();
         UI.Fill(new Rectangle(new(0, 10), new(display.Length - 1, 10)), Color.White, Color.Black, 0);
         UI.Print(0, 10, display);
         AboveGround.Fill(mousePos.X, mousePos.Y, 1, Color.White, Color.Transparent, 3191);
      }
		private static void DrawGraphics()
      {
         for (int x = 0; x < Ground.Width; x++)
            for (int y = 0; y < Ground.Height; y++)
            {
               Ground.SetGlyph(x, y, x + Ground.Width * (y + ScrollY));
               Ground.SetBackground(x, y, Color.Magenta);
            }
      }
      private static void OnScroll(object sender, MouseWheelScrollEventArgs e)
      {
         if (CurrentScene != Scene.GraphicsViewer)
            return;

         ScrollY -= (int)e.Delta;
         ScrollY = MathHelpers.Clamp(ScrollY, 0, 291);
      }
   }
}
