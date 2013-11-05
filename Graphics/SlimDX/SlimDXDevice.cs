using System;
using DeltaEngine.Core;
using DeltaEngine.Datatypes;
using DeltaEngine.Graphics.Vertices;
using SlimDX;
using Matrix = DeltaEngine.Datatypes.Matrix;
using SlimD3D9 = SlimDX.Direct3D9;
using SlimDX.Direct3D9;

namespace DeltaEngine.Graphics.SlimDX
{
	/// <summary>
	/// SlimDX graphics device.
	/// </summary>
	public class SlimDXDevice : Device
	{
		public SlimDXDevice(Window window, Settings settings)
			: base(window)
		{
			this.settings = settings;
			d3D = new Direct3D();
			InitializeDevice();
			InitializeProjectionMatrix();
			window.ViewportSizeChanged += OnViewportSizeChanged;
			window.FullscreenChanged += OnFullscreenChanged;
		}

		private readonly Settings settings;
		private readonly Direct3D d3D;

		public SlimD3D9.Device NativeDevice { get; private set; }

		private void InitializeDevice()
		{
			NativeDevice = new SlimD3D9.Device(d3D, 0, DeviceType.Hardware,
				(IntPtr)window.Handle, CreateFlags.HardwareVertexProcessing,
				GetPresentParameters());
		}

		private PresentParameters GetPresentParameters()
		{
			return new PresentParameters
			{
				Windowed = !window.IsFullscreen,
				DeviceWindowHandle = (IntPtr)window.Handle,
				SwapEffect = SwapEffect.Discard,
				PresentationInterval = settings.UseVSync ? PresentInterval.Default : PresentInterval.Immediate,
				BackBufferWidth = (int)window.ViewportPixelSize.Width,
				BackBufferHeight = (int)window.ViewportPixelSize.Height,
				EnableAutoDepthStencil = true,
				AutoDepthStencilFormat = GetDepthBufferFormat(),
				//makes some GPUs crash CreateWithSwapChain with E_INVALIDARG
				Multisample = MultisampleType.None,//GetAntiAliasingType(),
				MultisampleQuality = 0//settings.AntiAliasingSamples > 1 ? 1 : 0
			};
		}

		private Format GetDepthBufferFormat()
		{
			switch (settings.DepthBufferBits)
			{
			case 16:
				return Format.D16;
			case 32:
				return Format.D32;
			}
			return Format.D24S8;
		}

		private MultisampleType GetAntiAliasingType()
		{
			switch (settings.AntiAliasingSamples)
			{
			case 2:
				return MultisampleType.TwoSamples;
			case 3:
				return MultisampleType.ThreeSamples;
			case 4:
				return MultisampleType.FourSamples;
			}
			return MultisampleType.None;
		}

		private void OnViewportSizeChanged(Size displaySize)
		{
			deviceMustBeReset = true;
		}

		private bool deviceMustBeReset;

		public override void SetViewport(Size viewportSize)
		{
			SetModelViewProjectionMatrixFor2D();
		}

		protected override void OnFullscreenChanged(Size displaySize, bool b)
		{
			deviceMustBeReset = true;
		}

		private void InitializeProjectionMatrix()
		{
			var projection = Matrix.CreateOrthoProjection(window.ViewportPixelSize);
			SetProjectionMatrix(projection);
		}

		public override void Clear()
		{
			if (NativeDevice == null)
				return;
			if (deviceMustBeReset)
				ResetDevice();
			ClearScreenAndBeginScene();
			runExecuted = true;
		}

		private bool runExecuted;

		private void ClearScreenAndBeginScene()
		{
			var slimDXColor = new Color4(window.BackgroundColor.AlphaValue,
				window.BackgroundColor.RedValue, window.BackgroundColor.GreenValue,
				window.BackgroundColor.BlueValue);
			NativeDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, slimDXColor,
				1.0f, 0);
			NativeDevice.BeginScene();
		}

		private void ResetDevice()
		{
			if (DisposeNativeBuffers != null)
				DisposeNativeBuffers();
			NativeDevice.GetRenderTarget(0).Dispose();
			NativeDevice.Reset(GetPresentParameters());
			if (ReloadNativeBuffersData != null)
				ReloadNativeBuffersData();
			currentBlendMode = BlendMode.Opaque;
			cullModeClockwise = false;
			deviceMustBeReset = false;
		}

		public Action DisposeNativeBuffers;
		public Action ReloadNativeBuffersData;

		private BlendMode currentBlendMode = BlendMode.Opaque;
		private bool cullModeClockwise;

		public override void Present()
		{
			if (NativeDevice == null || !runExecuted)
				return;
			NativeDevice.EndScene();
			NativeDevice.Present();
		}

		public override CircularBuffer CreateCircularBuffer(ShaderWithFormat shader,
			BlendMode blendMode, VerticesMode drawMode = VerticesMode.Triangles)
		{
			return new SlimDXCircularBuffer(this, shader, blendMode, drawMode);
		}

		public override bool CullBackFaces
		{
			get { return cullBackFaces; }
			set
			{
				if (cullBackFaces == value)
					return;
				cullBackFaces = value;
				NativeDevice.SetRenderState(RenderState.CullMode,
					cullBackFaces ? Cull.Counterclockwise : Cull.None);
			}
		}

		private bool cullBackFaces;

		public override void DisableDepthTest()
		{			
			NativeDevice.SetRenderState(RenderState.ZEnable, false);
		}

		public override void EnableDepthTest()
		{
			NativeDevice.SetRenderState(RenderState.ZEnable, true);
		}

		public override void Dispose()
		{
			NativeDevice.GetRenderTarget(0).Dispose();
			NativeDevice.Dispose();
			d3D.Dispose();
		}

		public override void SetBlendMode(BlendMode blendMode)
		{
			if (currentBlendMode == blendMode)
				return;
			NativeDevice.SetRenderState(RenderState.AlphaRef, 1);
			switch (blendMode)
			{
			case BlendMode.Opaque:
				SetupBlendRenderStateModeOpaque();
				break;
			case BlendMode.Normal:
				SetupBlendRenderStateModeNormal();
				break;
			case BlendMode.AlphaTest:
				SetupBlendRenderStateModeAlphaTest();
				break;
			case BlendMode.Additive:
				SetupBlendRenderStateModeAdditive();
				break;
			case BlendMode.Subtractive:
				SetupBlendRenderStateModeSubtractive();
				break;
			case BlendMode.LightEffect:
				SetupBlendRenderStateModeLightEffect();
				break;
			}
			currentBlendMode = blendMode;
		}

		private void SetupBlendRenderStateModeNormal()
		{
			NativeDevice.SetRenderState(RenderState.AlphaBlendEnable, true);
			NativeDevice.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
			NativeDevice.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
			NativeDevice.SetRenderState(RenderState.BlendOperation, BlendOperation.Add);
		}

		private void SetupBlendRenderStateModeOpaque()
		{
			NativeDevice.SetRenderState(RenderState.AlphaBlendEnable, false);
		}

		private void SetupBlendRenderStateModeAlphaTest()
		{
			NativeDevice.SetRenderState(RenderState.AlphaBlendEnable, false);
			NativeDevice.SetRenderState(RenderState.AlphaTestEnable, true);
			NativeDevice.SetRenderState(RenderState.AlphaFunc, Compare.GreaterEqual);
		}

		private void SetupBlendRenderStateModeAdditive()
		{
			NativeDevice.SetRenderState(RenderState.AlphaBlendEnable, true);
			NativeDevice.SetRenderState(RenderState.SourceBlend, Blend.One);
			NativeDevice.SetRenderState(RenderState.DestinationBlend, Blend.One);
			NativeDevice.SetRenderState(RenderState.BlendOperation, BlendOperation.Add);
		}

		private void SetupBlendRenderStateModeSubtractive()
		{
			NativeDevice.SetRenderState(RenderState.AlphaBlendEnable, true);
			NativeDevice.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
			NativeDevice.SetRenderState(RenderState.DestinationBlend, Blend.One);
			NativeDevice.SetRenderState(RenderState.BlendOperation, BlendOperation.ReverseSubtract);
		}

		private void SetupBlendRenderStateModeLightEffect()
		{
			NativeDevice.SetRenderState(RenderState.AlphaBlendEnable, true);
			NativeDevice.SetRenderState(RenderState.SourceBlend, Blend.DestinationColor);
			NativeDevice.SetRenderState(RenderState.DestinationBlend, Blend.One);
			NativeDevice.SetRenderState(RenderState.BlendOperation, BlendOperation.Add);
		}
	}
}