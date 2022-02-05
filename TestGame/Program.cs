using SMPL;
using SMPL.Profiling;

namespace TestGame
{
	class Program : Simple
	{
		static void Main() => Start(1280, 720, new Program());
		public override void OnStart()
		{
			Debug.LogError(0, "penka");
		}
	}
}
