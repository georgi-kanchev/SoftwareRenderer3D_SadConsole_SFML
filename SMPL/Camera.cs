namespace SMPL
{
	public class Camera
	{
		public static Camera Main { get; } = new();
		public Area Area { get; } = new();
		public float FieldOfView { get; set; } = 45;
	}
}
