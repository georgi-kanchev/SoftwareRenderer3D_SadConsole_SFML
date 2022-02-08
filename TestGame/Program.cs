using SMPL;
using System.Numerics;
using System.Collections.Generic;
using SMPL.Effects;
using SFML.Graphics;
using SadConsole;

namespace TestGame
{
	class Program : Simple
	{
		Mesh mesh;
		List<Effect> effects;

		static void Main() => Start(new Program(), glyphWidth: 8, glyphHeight: 8);
		public override void OnStart()
		{
			mesh = Mesh.Load(Mesh.Shape.Cube, new("mario.png"));
			effects = new() { new Dither() };
		}
		public override void OnUpdate()
		{
			Console.Print(0, 0, "Test");
			RenderWindowSFML.SetTitle($"{Time.FPS:F1}");
			mesh.Area.Position = new(0, 0, 0);
			mesh.Area.Scale = new(50, 50, 50);
			mesh.Area.Rotation += new Vector3(0.01f, 0.01f, 0);
			mesh.Draw(effects: effects);
		}
	}
}
