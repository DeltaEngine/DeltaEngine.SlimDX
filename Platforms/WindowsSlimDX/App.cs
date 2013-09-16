namespace DeltaEngine.Platforms
{
	/// <summary>
	/// Initializes the SlimDXResolver for Windows DirectX 9. To execute the app call Run.
	/// </summary>
	public abstract class App
	{
		protected void Run()
		{
			resolver.Run();
		}

		private readonly SlimDXResolver resolver = new SlimDXResolver();

		protected T Resolve<T>() where T : class
		{
			return resolver.Resolve<T>();
		}
	}
}