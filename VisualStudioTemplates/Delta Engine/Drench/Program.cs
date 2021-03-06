using DeltaEngine.Core;
using DeltaEngine.Platforms;
using DeltaEngine.ScreenSpaces;

namespace $safeprojectname$
{
	internal class Program : App
	{
		public Program()
		{
			new RelativeScreenSpace(Resolve<Window>());
			new DrenchMenu();
		}

		public static void Main()
		{ 
			new Program().Run();
		}
	}
}