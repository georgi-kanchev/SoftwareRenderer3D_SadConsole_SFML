using SMPL;
using System.Numerics;
using SFML.Window;
using Color = SadRogue.Primitives.Color;
using SMPL.Effects;

namespace TestGame
{
	class Program : Simple
	{
		Mesh mesh;
		Vector2 prevMousePos;

		static void Main() => Start(new Program(), glyphWidth: 4, glyphHeight: 4);
		public override void OnStart()
		{
			mesh = Mesh.Load("map.obj", new("map.png"));
			mesh.Area.Rotation = new(-90, 0, 0);
			mesh.Area.Scale = new(1, 1, 1);
			Camera.Main.BackgroundColor = Color.DarkCyan;
		}

		public override void OnUpdate()
		{
			RenderWindowSFML.SetTitle($"FPS: {Time.FPS:F1}");

			var dir = new Vector3();
			if (Keyboard.IsKeyPressed(Keyboard.Key.W) || Keyboard.IsKeyPressed(Keyboard.Key.Up)) dir += Camera.Main.Area.Forward;
			if (Keyboard.IsKeyPressed(Keyboard.Key.S) || Keyboard.IsKeyPressed(Keyboard.Key.Down)) dir += Camera.Main.Area.Back;
			if (Keyboard.IsKeyPressed(Keyboard.Key.A) || Keyboard.IsKeyPressed(Keyboard.Key.Left)) dir += Camera.Main.Area.Left;
			if (Keyboard.IsKeyPressed(Keyboard.Key.D) || Keyboard.IsKeyPressed(Keyboard.Key.Right)) dir += Camera.Main.Area.Right;
			if (Keyboard.IsKeyPressed(Keyboard.Key.LShift)) dir += Camera.Main.Area.Up;
			if (Keyboard.IsKeyPressed(Keyboard.Key.LControl)) dir += Camera.Main.Area.Down;

			Camera.Main.Area.Position += dir * Time.Delta * 500;

			var mousePos = Mouse.GetPosition();

			var deltaX = mousePos.X - prevMousePos.X;
			var deltaY = mousePos.Y - prevMousePos.Y;

			Camera.Main.Area.Rotation += new Vector3(deltaY * Time.Delta * 10, deltaX * Time.Delta * 10, 0);
			prevMousePos = new(mousePos.X, mousePos.Y);

			if (Keyboard.IsKeyPressed(Keyboard.Key.Z)) Camera.Main.FieldOfView-=2;
			if (Keyboard.IsKeyPressed(Keyboard.Key.X)) Camera.Main.FieldOfView+=2;

			mesh.Draw();
		}
	}
}
