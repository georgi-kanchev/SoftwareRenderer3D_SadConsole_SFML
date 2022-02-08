using SMPL;
using System.Numerics;
using System.Collections.Generic;
using SadConsole;
using SMPL.Effects;
using SFML.Graphics;

namespace TestGame
{
	class Program : Simple
	{
		Mesh mesh;
		List<Effect> effects;

		static void Main() => Start(new Program(), glyphWidth: 2, glyphHeight: 4);
		public override void OnStart()
		{
			mesh = Mesh.Load("person.obj", new Image("person.png"));
			effects = new() { new Blink() };
		}
		public override void OnUpdate()
		{
			RenderWindowSFML.SetTitle($"{Time.FPS:F1}");
			mesh.Area.Position = new(0, -100, 100);
			mesh.Area.Scale = new(200, 200, 200);
			mesh.Area.Rotation += new Vector3(0, 0.01f, 0);
			mesh.Draw(effects: effects);
		}
	}
}
