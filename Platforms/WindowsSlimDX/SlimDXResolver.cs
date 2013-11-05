﻿using DeltaEngine.Content.Xml;
using DeltaEngine.Graphics;
using DeltaEngine.Graphics.SlimDX;
using DeltaEngine.Input.SlimDX;
using DeltaEngine.Input.Windows;
using DeltaEngine.Multimedia.SlimDX;
using DeltaEngine.Platforms.Windows;
using DeltaEngine.Rendering2D;

namespace DeltaEngine.Platforms
{
	internal class SlimDXResolver : AppRunner
	{
		public SlimDXResolver()
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