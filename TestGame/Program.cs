using SMPL;
using System.Numerics;
using System.Collections.Generic;
using SMPL.Effects;
using SadConsole;
using SadConsole.Input;

namespace TestGame
{
	class Program : Simple
	{
		Mesh mesh;
		Mesh mesh2;
		List<Effect> effects;

		static void Main() => Start(new Program(), glyphWidth: 8, glyphHeight: 8);
		public override void OnStart()
		{
			mesh = Mesh.Load(Mesh.Shape.Cube, new("mario.png"));
			mesh2 = Mesh.Load(Mesh.Shape.Pyramid);
			effects = new() { new Dither() };
		}
		public override void OnUpdate()
		{
			RenderWindowSFML.SetTitle($"{Time.FPS:F1}");

			var eff = (Dither)effects[0];
			mesh2.Area.Rotation += new Vector3(0, 0.01f, 0);
			mesh2.Area.Scale = new(100, 100, 100);
			mesh2.Draw(effects: effects);
			mesh.Area.Scale = new(50, 50, 50);
			mesh.Area.Position = new(20, 0, 0);
			mesh.Draw(effects: effects);
		}
	}
}
