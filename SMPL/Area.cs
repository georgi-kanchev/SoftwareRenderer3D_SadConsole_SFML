using System;
using System.Numerics;

namespace SMPL
{
	public class Area
	{
		internal Vector3 rot, right = Vector3.UnitX, up = Vector3.UnitY, front = Vector3.UnitZ;

		public Vector3 Right => right;
		public Vector3 Left => -right;
		public Vector3 Up => up;
		public Vector3 Down => -up;
		public Vector3 Forward => front;
		public Vector3 Back => -front;

		public Vector3 Position { get; set; }
		public Vector3 Rotation 
		{
			get => new(rot.X.ToDegrees(), rot.Y.ToDegrees(), rot.Z.ToDegrees());
			set { rot = new(value.X.ToRadians(), value.Y.ToRadians(), value.Z.ToRadians()); Update(); }
		}
		public Vector3 Scale { get; set; } = new(10, 10, 10);

		private void Update()
		{
			front.X = MathF.Cos(-rot.X) * MathF.Sin(rot.Y);
			front.Y = MathF.Sin(-rot.X);
			front.Z = MathF.Cos(-rot.X) * MathF.Cos(rot.Y);

			front = Vector3.Normalize(front);
			right = Vector3.Normalize(Vector3.Cross(-front, Vector3.UnitY));
			up = Vector3.Normalize(Vector3.Cross(right, -front));
		}
	}
}
