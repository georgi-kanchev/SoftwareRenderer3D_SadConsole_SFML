using SMPL;
using SMPL.Profiling;
using SadConsole.Input;
using SadConsole;

namespace TestGame
{
	class Program : Simple
	{
		static void Main() => Start(1280, 720, new Program());
		public override void OnStart()
		{
			Multiplayer.StartServer();
		}
	}
}
