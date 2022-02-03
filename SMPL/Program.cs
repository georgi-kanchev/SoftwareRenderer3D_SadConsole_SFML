using SadConsole;
using Console = SadConsole.Console;
using SFML.Graphics;
using Color = SadRogue.Primitives.Color;
using System.Numerics;
using SFML.System;
using SadConsole.UI;
using System;

namespace SMPL
{
   static class Program
   {
      internal static Image defaultImage = new(new SFML.Graphics.Color[,]
      {
         { SFML.Graphics.Color.Magenta, SFML.Graphics.Color.Black },
         { SFML.Graphics.Color.Black, SFML.Graphics.Color.Magenta }
      });
      public static float[,] zBuffer;
      public static Vector2 consoleSize;
      public static Window cubeWindow;
      public static Console console;
      static RenderWindow window;
      static readonly Clock deltaTime = new();
      const uint windowWidth = 800, windowHeight = 600;
      internal static readonly Image grass_top = new("grass_top.png");
      internal static readonly Image grass_side = new("grass_side.png");
      internal static readonly Image grass_bottom = new("grass_bottom.png");
      internal static readonly Image mario = new("mario.png");
      internal static readonly Image person = new("person.png");
      internal static readonly Image dragon = new("dragon.png");
      internal static Mesh personModel;
      internal static Mesh dragonModel;

      static void Main()
      {
         window = new RenderWindow(new(windowWidth, windowHeight), "");
         Game.Create(((int)windowWidth / 10), ((int)windowHeight / 24), window: window);

         Game.Instance.OnStart = Init;

         Game.Instance.Run();
         Game.Instance.Dispose();
      }

      private static void Init()
      {
         console = (Console)GameHost.Instance.Screen;
         console.FontSize = new(8, 16);
         cubeWindow = new Window(160, 160);
         zBuffer = new float[cubeWindow.Width * 2, cubeWindow.Height * 2];

         Game.Instance.FrameUpdate += Update;

			window.Resized += OnWindowResize;
         Settings.ResizeMode = Settings.WindowResizeOptions.None;
         UpdateConsoleSize();

         cubeWindow.FontSize = new(6, 6);
         cubeWindow.TitleAlignment = HorizontalAlignment.Left;
         cubeWindow.TitleAreaLength = cubeWindow.Width;
         cubeWindow.Show();

         personModel = Mesh.Load("person.obj", person);
         dragonModel = Mesh.Load("dragon.obj", dragon);
      }

      private static void OnWindowResize(object sender, SFML.Window.SizeEventArgs e)
      {
         UpdateConsoleSize();
      }
      private static void UpdateConsoleSize()
      {
         consoleSize = new((int)window.Size.X / console.Font.GlyphWidth, (int)window.Size.Y / console.Font.GlyphHeight);
         console.Resize((int)consoleSize.X, (int)consoleSize.Y, false);
      }
		private static void Update(object sender, GameHost e)
		{
         var fps = 1f / deltaTime.ElapsedTime.AsSeconds();
         deltaTime.Restart();

         console.Clear();
         cubeWindow.Clear();
         Array.Clear(zBuffer, 0, zBuffer.Length);

         personModel.Area.Position = new(0, 70, 50);
         personModel.Area.Scale = new(100, -100, 100);
         personModel.Area.Rotation += new Vector3(0, 0.01f, 0);
         personModel.Draw(cubeWindow, Camera.Main);

         dragonModel.Area.Position = new(0, 10, 100);
         dragonModel.Area.Scale = new(300, -300, 300);
         dragonModel.Area.Rotation += new Vector3(0, -0.01f, 0);
         dragonModel.Draw(cubeWindow, Camera.Main);

         DrawWindowBorder();
         console.Print(0, 0, $"FPS: {fps:F0}");

         void DrawWindowBorder()
         {
            var w = cubeWindow.Width - 1;
            var h = cubeWindow.Height - 1;

            // bottom
            cubeWindow.DrawLine(new(0, h), new(w, h), null, background: Color.White);
            // left
            cubeWindow.DrawLine(new(0, 0), new(0, h), null, background: Color.White);
            // right
            cubeWindow.DrawLine(new(w, 0), new(w, h), null, background: Color.White);
            // top
            cubeWindow.DrawLine(new(0, 0), new(w, 0), null, background: Color.Gray);
         }
      }
	}
}