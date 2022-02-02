namespace SMPL
{
	public class Camera
	{
		public static Camera Main { get; }

		public Area Area { get; } = new();

		static Camera()
		{
			Main = new();
			Main.Area.Position = new(0, 0, -10);
		}
	}
}
