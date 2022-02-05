using SadConsole;
using Console = SadConsole.Console;
using SFML.Graphics;
using System.Numerics;
namespace SMPL
{
   public abstract class Simple
   {
      internal static Simple userGame;
      private static Vector2 consoleSize;

      public static RenderWindow RenderWindowSFML { get; private set; }
      public static Console Console { get; private set; }

      static void Main() { }
      public static void Start(uint width, uint height, Simple simpleGame)
      {
         userGame = simpleGame;
         RenderWindowSFML = new RenderWindow(new(width, height), "SMPL Game");
         Game.Create(((int)width / 10), ((int)height / 24), window: RenderWindowSFML);
         Console = (Console)GameHost.Instance.Screen;

         Game.Instance.OnStart = userGame.OnStart;
         Game.Instance.OnEnd = userGame.OnStop;
         Game.Instance.FrameUpdate += Update;
         RenderWindowSFML.Resized += OnWindowResize;

         Settings.ResizeMode = Settings.WindowResizeOptions.None;
         UpdateConsoleSize();

         Game.Instance.Run();
         Game.Instance.Dispose();
      }

      public virtual void OnStart() { }
      public virtual void OnUpdate() { }
      public virtual void OnStop() { }

      private static void OnWindowResize(object sender, SFML.Window.SizeEventArgs e) => UpdateConsoleSize();
		private static void Update(object sender, GameHost e) => userGame.OnUpdate();

      private static void UpdateConsoleSize()
      {
         consoleSize = new((int)RenderWindowSFML.Size.X / Console.Font.GlyphWidth, (int)RenderWindowSFML.Size.Y / Console.Font.GlyphHeight);
         Console.Resize((int)consoleSize.X, (int)consoleSize.Y, false);
      }
	}
}