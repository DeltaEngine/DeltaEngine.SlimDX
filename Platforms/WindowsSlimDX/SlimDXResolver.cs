using DeltaEngine.Content.Xml;
using DeltaEngine.Graphics;
using DeltaEngine.Graphics.SlimDX;
using DeltaEngine.Input.SlimDX;
using DeltaEngine.Input.Windows;
using DeltaEngine.Multimedia.SlimDX;
using DeltaEngine.Platforms.Windows;
using DeltaEngine.Rendering2D;
#if !DEBUG 
using System;
using DeltaEngine.Core;
using DeltaEngine.Extensions;
#endif

namespace DeltaEngine.Platforms
{
	internal class SlimDXResolver : AppRunner
	{
		public SlimDXResolver()
		{
#if DEBUG
			InitializeSlimDX();
#else
			// Some machines with missing frameworks initialization will crash, we need useful errors
			try
			{
				InitializeSlimDX();
			}
			catch (Exception exception)
			{
				Logger.Error(exception);
				if (StackTraceExtensions.StartedFromNCrunchOrNunitConsole)
					throw;
				DisplayMessageBoxAndCloseApp("Fatal SlimDX Initialization Error", exception);
			}
#endif
		}

		private void InitializeSlimDX()
		{
			RegisterCommonEngineSingletons();
			RegisterSingleton<FormsWindow>();
			RegisterSingleton<WindowsSystemInformation>();
			RegisterSingleton<SlimDXDevice>();
			RegisterSingleton<Drawing>();
			RegisterSingleton<BatchRenderer>();
			RegisterSingleton<SlimDXScreenshotCapturer>();
			RegisterSingleton<XAudioDevice>();
			RegisterSingleton<SlimDXMouse>();
			RegisterSingleton<SlimDXKeyboard>();
			RegisterSingleton<SlimDXGamePad>();
			RegisterSingleton<WindowsTouch>();
			RegisterSingleton<WindowsGamePad>();
			RegisterSingleton<CursorPositionTranslater>();
			Register<InputCommands>();
			if (IsAlreadyInitialized)
				throw new UnableToRegisterMoreTypesAppAlreadyStarted();
		}

		protected override void RegisterMediaTypes()
		{
			base.RegisterMediaTypes();
			Register<SlimDXImage>();
			Register<SlimDXShader>();
			Register<SlimDXGeometry>();
			Register<XAudioSound>();
			Register<XAudioMusic>();
			Register<XmlContent>();
		}
	}
}