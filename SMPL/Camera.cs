using SadRogue.Primitives;
using System;
using System.Numerics;

namespace SMPL
{
	public class Camera
	{
		private float fov;

		public Color BackgroundColor { get; set; }
		public static Camera Main { get; } = new();
		public Area Area { get; } = new() { Position = new(0, 0, -100) };
		public float FieldOfView { get => fov; set { fov = Math.Clamp(value, 0, 180); } }
		public bool AppliesDepth { get; set; } = true;

		public Camera()
		{
			fov = 80;
		}
	}
}
