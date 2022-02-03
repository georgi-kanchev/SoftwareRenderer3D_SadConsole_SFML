using System.Numerics;
using static System.MathF;

namespace SMPL
{
	public static class Extensions
	{
		public static Vector3 Translate(this Vector3 point, Vector3 translation)
		{
			point.X += translation.X;
			point.Y += translation.Y;
			point.Z += translation.Z;
			return point;
		}
		public static Vector3 Rotate(this Vector3 point, Vector3 rotation)
		{
			var result = new Vector3()
			{
				X = point.X * (Cos(rotation.Z) * Cos(rotation.Y)) +
				point.Y * (Cos(rotation.Z) * Sin(rotation.Y) * Sin(rotation.X) - Sin(rotation.Z) * Cos(rotation.X)) +
				point.Z * (Cos(rotation.Z) * Sin(rotation.Y) * Cos(rotation.X) + Sin(rotation.Z) * Sin(rotation.X)),
				Y = point.X * (Sin(rotation.Z) * Cos(rotation.Y)) +
				point.Y * (Sin(rotation.Z) * Sin(rotation.Y) * Sin(rotation.X) + Cos(rotation.Z) * Cos(rotation.X)) +
				point.Z * (Sin(rotation.Z) * Sin(rotation.Y) * Cos(rotation.X) - Cos(rotation.Z) * Sin(rotation.X)),
				Z = point.X * (-Sin(rotation.Y)) +
				point.Y * (Cos(rotation.Y) * Sin(rotation.X)) +
				point.Z * (Cos(rotation.Y) * Cos(rotation.X)),
			};
			return result;
		}
		public static Vector3 ApplyPerspective(this Vector3 point, float camResolutionX, float camFieldOfView)
		{
			var Z0 = (camResolutionX / 2f) / Tan((camFieldOfView / 2f) * PI / 180f);
			point.X *= Z0 / (Z0 + point.Z);
			point.Y *= Z0 / (Z0 + point.Z);
			point.Z *= Z0 / (Z0 + point.Z);
			return point;
		}
		public static Vector3 FixAffineCoordinates(this Vector3 texCoords, float pointZ, float camResolutionX, float camFieldOfView)
		{
			var Z0 = (camResolutionX / 2f) / Tan((camFieldOfView / 2f) * PI / 180f);
			texCoords.X *= Z0 / (Z0 + pointZ);
			texCoords.Y *= Z0 / (Z0 + pointZ);
			texCoords.Z *= Z0 / (Z0 + pointZ);
			return texCoords;
		}
		public static Vector3 CenterScreen(this Vector3 point, Vector2 size)
		{
			return new(point.X + size.X * 0.5f, point.Y + size.Y * 0.5f, point.Z);
		}
	}
}
