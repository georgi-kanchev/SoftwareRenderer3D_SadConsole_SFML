using System.Numerics;

namespace SMPL
{
	public class Area
	{
		public Vector3 Position { get; set; }
		public Vector3 Rotation { get; set; }
		public Vector3 Scale { get; set; } = new(1, 1, 1);
	}
}
