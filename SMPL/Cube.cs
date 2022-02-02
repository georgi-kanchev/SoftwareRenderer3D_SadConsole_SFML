using SadConsole;
using SFML.Graphics;

namespace SMPL
{
	public class Cube
	{
		public Area Area { get; } = new();
		private readonly Triangle[] triangles;

		public Cube(Image front, Image back, Image left, Image right, Image top, Image bottom)
		{
			triangles = new Triangle[]
			{
				new(new(new(-10, -10, -10), new(0, 0, 1)), new(new(-10, 10, -10), new(0, 1, 1)), new(new(10, -10, -10), new(1, 0, 1)), front),
				new(new(new(-10, 10, -10), new(0, 1, 1)), new(new(10, 10, -10), new(1, 1, 1)), new(new(10, -10, -10), new(1, 0, 1)), front),

				new(new(new(10, -10, 10), new(1, 0, 1)), new(new(-10, 10, 10), new(0, 1, 1)), new(new(-10, -10, 10), new(0, 0, 1)), back),
				new(new(new(10, -10, 10), new(1, 0, 1)), new(new(10, 10, 10), new(1, 1, 1)), new(new(-10, 10, 10), new(0, 1, 1)), back),

				new(new(new(-10, -10, 10), new(0, 1, 1)), new(new(-10, -10, -10), new(0, 0, 1)), new(new(10, -10, 10), new(1, 1, 1)), top),
				new(new(new(-10, -10, -10), new(0, 0, 1)), new(new(10, -10, -10), new(1, 0, 1)), new(new(10, -10, 10), new(1, 1, 1)), top),
				
				new(new(new(-10, 10, -10), new(0, 0, 1)), new(new(10, 10, 10), new(1, 1, 1)), new(new(10, 10, -10), new(1, 0, 1)), bottom),
				new(new(new(-10, 10, -10), new(0, 0, 1)), new(new(-10, 10, 10), new(0, 1, 1)), new(new(10, 10, 10), new(1, 1, 1)), bottom),
				
				new(new(new(-10, -10, 10), new(1, 0, 1)), new(new(-10, 10, -10), new(0, 1, 1)), new(new(-10, -10, -10), new(0, 0, 1)), left),
				new(new(new(-10, -10, 10), new(1, 0, 1)), new(new(-10, 10, 10), new(1, 1, 1)), new(new(-10, 10, -10), new(0, 1, 1)), left),
				
				new(new(new(10, -10, -10), new(1, 0, 1)), new(new(10, 10, -10), new(1, 1, 1)), new(new(10, -10, 10), new(0, 0, 1)), right),
				new(new(new(10, -10, 10), new(0, 0, 1)), new(new(10, 10, -10), new(1, 1, 1)), new(new(10, 10, 10), new(0, 1, 1)), right),
			};
		}

		public void Draw(Console console, Camera camera)
		{
			for (int i = 0; i < triangles.Length; i++)
			{
				triangles[i].UpdatePoints(Area, console, camera);
				triangles[i].Draw(console, triangles[i].Image, true);
			}
		}
	}
}
