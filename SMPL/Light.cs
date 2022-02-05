using SadRogue.Primitives;
using System.Numerics;

namespace SMPL
{
	public class Light
	{
		internal enum Type { Ambient, Directional, Point }
		public static class Sun
		{
			public static Vector3 Direction { get; set; } = new(1, -1, -1);
			public static Color ColorLight { get; set; } = Color.White;
			public static Color ColorShadow { get; set; } = new(50, 50, 50, 255);
		}
	}
}
