using SMPL;
using System.Collections.Generic;
using SMPL.Effects;

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
			effects = new() { new OutlineImage() };
		}
		public override void OnUpdate()
		{
			RenderWindowSFML.SetTitle($"{Time.FPS:F1}");

			mesh.Area.Scale = new(50, 50, 50);
			mesh.Draw(effects: effects);
		}
	}
}
