using SadConsole;
using Console = SadConsole.Console;
using SFML.Graphics;
using System.Numerics;
using System;
using System.Threading;

namespace SMPL
{
   public abstract class Simple
   {
      internal static Simple userGame;

      public static RenderWindow RenderWindowSFML { get; private set; }
      public static Console Console { get; private set; }

      static void Main() { }
      public static void Start(Simple simpleGame, uint resolutionWidth = 1280, uint resolutionHeight = 720, int glyphWidth = 8, int glyphHeight = 16)
      {
         userGame = simpleGame;
         RenderWindowSFML = new RenderWindow(new(resolutionWidth, resolutionHeight), "SMPL Game");
         Game.Create((int)resolutionWidth / glyphWidth, (int)resolutionHeight / glyphHeight, window: RenderWindowSFML);
         Console = (Console)GameHost.Instance.Screen;
         Console.FontSize = new(glyphWidth, glyphHeight);

         Game.Instance.OnStart = userGame.OnStart;
         Game.Instance.OnEnd = userGame.OnStop;
         Game.Instance.FrameUpdate += Update;

         Settings.ResizeMode = Settings.WindowResizeOptions.Stretch;
         Triangle.RecreateDepthBuffer();

         Game.Instance.Run();
         Game.Instance.Dispose();
      }

		public virtual void OnStart() { }
      public virtual void OnUpdate() { }
      public virtual void OnStop() { }

      private static void Update(object sender, GameHost e)
      {
         Console.Fill(background: Camera.Main.BackgroundColor);
         Time.Update();
         Triangle.ClearDepthBuffer();
         userGame.OnUpdate();
      }
	}
}