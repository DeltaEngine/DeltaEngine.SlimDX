﻿using System.IO;
using DeltaEngine.Content;
using DeltaEngine.Core;
using DeltaEngine.Datatypes;
using SlimDX.Direct3D9;

namespace DeltaEngine.Graphics.SlimDX
{
	/// <summary>
	/// SlimDX image.
	/// </summary>
	public class SlimDXImage : Image
	{
		protected SlimDXImage(string contentName, SlimDXDevice device)
			: base(contentName)
		{
			this.device = device;
		}

		private SlimDXImage(ImageCreationData data, SlimDXDevice device)
			: base(data)
		{
			this.device = device;
		}

		private readonly SlimDXDevice device;

		protected override void LoadImage(Stream fileData)
		{
			NativeTexture = Texture.FromStream(device.NativeDevice, fileData, Usage.None, Pool.Managed);
			if (HasAlpha && NativeTexture.GetLevelDescription(0).Format != Format.A8R8G8B8)
				Logger.Warning("Image '" + Name +
					"' is supposed to have alpha pixels, but the image pixel format is not using alpha.");
			else if (!HasAlpha && NativeTexture.GetLevelDescription(0).Format == Format.A8R8G8B8)
				Logger.Warning("Image '" + Name +
					"' is supposed to have no alpha pixels, but the image pixel format is using alpha.");
			var textureSize = new Size(NativeTexture.GetLevelDescription(0).Width,
				NativeTexture.GetLevelDescription(0).Height);
			CompareActualSizeMetadataSize(textureSize);
		}

		public Texture NativeTexture { get; protected set; }

		public override void Fill(Color[] colors)
		{
			if (PixelSize.Width * PixelSize.Height != colors.Length)
				throw new InvalidNumberOfColors(PixelSize);
			if (NativeTexture == null)
				NativeTexture = new Texture(device.NativeDevice, (int)PixelSize.Width,
					(int)PixelSize.Height, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
			var rectangle = NativeTexture.LockRectangle(0, LockFlags.None);
			for (int i = 0; i < colors.Length; i++)
				rectangle.Data.Write(colors[i].PackedBgra);
			NativeTexture.UnlockRectangle(0);
		}

		public override void Fill(byte[] colors)
		{
			if (PixelSize.Width * PixelSize.Height * 4 != colors.Length)
				throw new InvalidNumberOfBytes(PixelSize);
			if (NativeTexture == null)
				NativeTexture = new Texture(device.NativeDevice, (int)PixelSize.Width,
					(int)PixelSize.Height, 0, Usage.None, Format.A8B8G8R8, Pool.Managed);
			NativeTexture.Fill((Fill2DCallback)null);
		}

		protected override void SetSamplerState()
		{
			device.NativeDevice.SetTexture(0, NativeTexture);
			device.NativeDevice.SetSamplerState(0, SamplerState.MipFilter, TextureFilter.None);
			device.NativeDevice.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Linear);
			device.NativeDevice.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Linear);
		}

		protected override void DisposeData()
		{
			NativeTexture.Dispose();
		}
	}
}