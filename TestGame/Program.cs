using SadConsole;
using SadRogue.Primitives;
using SMPL;
using System.Numerics;

namespace TestGame
{
	class Program : Simple
	{
		Mesh mesh;

		static void Main() => Start(1280, 720, new Program());
		public override void OnStart()
		{
			mesh = Mesh.Load("person.obj", null);
		}
		public override void OnUpdate()
		{
			mesh.Area.Position = new(0, 50, 100);
			mesh.Area.Scale = new(100, 100, 100);
			mesh.Area.Rotation += new Vector3(0, 0.01f, 0);
			mesh.Draw();
		}
	}
}
