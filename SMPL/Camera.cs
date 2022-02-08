namespace SMPL
{
	public class Camera
	{
		public static Camera Main { get; } = new();
		public Area Area { get; } = new() { Position = new(0, 0, -100) };
		public float FieldOfView { get; set; } = 45;
	}
}
