using System;
using System.Numerics;

namespace SMPL
{
	public class Area
	{
		private Vector3 dir, right = Vector3.UnitX, up = Vector3.UnitY, front = Vector3.UnitZ;

		public Vector3 Right => right;
		public Vector3 Left => -right;
		public Vector3 Up => up;
		public Vector3 Down => -up;
		public Vector3 Front => front;
		public Vector3 Back => -front;

		public Vector3 Position { get; set; }
		public Vector3 Rotation { get; set; }
		public Vector3 Direction
		{
			get => dir;
			set { dir = Vector3.Normalize(value); Update(); }
		}
		public Vector3 Scale { get; set; } = new(1, 1, 1);

		private void Update()
		{
			right = Vector3.Normalize(Vector3.Cross(front, Vector3.UnitY));
			up = Vector3.Normalize(Vector3.Cross(right, front));
		}
	}
}
