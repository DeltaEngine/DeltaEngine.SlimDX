﻿using System;
using System.Diagnostics;
using System.IO;
using DeltaEngine.Core;
using DeltaEngine.Extensions;
using DeltaEngine.Multimedia.MusicStreams;
using SlimDX.Multimedia;
using SlimDX.XAudio2;

namespace DeltaEngine.Multimedia.SlimDX
{
	/// <summary>
	/// Native XAudio implementation for music playback.
	/// </summary>
	public class XAudioMusic : Music
	{
		protected XAudioMusic(string contentName, XAudioDevice device, Settings settings)
			: base(contentName, device, settings)
		{
			CreateBuffers();
		}

		protected override void LoadData(Stream fileData)
		{
			try
			{
				var stream = new MemoryStream();
				fileData.CopyTo(stream);
				stream.Position = 0;
				musicStream = new MusicStreamFactory().Load(stream, "Content/" + Name);
				source = new SourceVoice((device as XAudioDevice).XAudio2,
					new WaveFormat
					{
						SamplesPerSecond = musicStream.Samplerate,
						BitsPerSample = 16,
						Channels = (short)musicStream.Channels,
						AverageBytesPerSecond = (musicStream.Samplerate / 16) / 8,
						BlockAlignment = (short)(2 * musicStream.Channels),
						FormatTag = WaveFormatTag.Pcm
					});
			}
			catch (Exception ex)
			{
				if (Debugger.IsAttached)
					throw new MusicNotFoundOrAccessible(Name, ex);
			}
		}

		private BaseMusicStream musicStream;
		private SourceVoice source;

		private void CreateBuffers()
		{
			buffers = new StreamBuffer[NumberOfBuffers];
			for (int i = 0; i < NumberOfBuffers; i++)
				buffers[i] = new StreamBuffer();
		}

		private StreamBuffer[] buffers;
		private const int NumberOfBuffers = 2;

		protected override void PlayNativeMusic(float volume)
		{
			musicStream.Rewind();
			source.Start();
			source.Volume = volume;
			isPlaying = true;
			playStartTime = DateTime.Now;
		}

		private DateTime playStartTime;
		private bool isPlaying;

		public override void Run()
		{
			while (source.State.BuffersQueued < NumberOfBuffers)
			{
				PutInStreamIfDataAvailable();
				if (isAbleToStream)
					continue;
				Stop();
				break;
			}
		}

		protected override void StopNativeMusic()
		{
			isPlaying = false;
			source.Stop();
			source.FlushSourceBuffers();
		}
		
		public override bool IsPlaying()
		{
			return isPlaying;
		}

		private void PutInStreamIfDataAvailable()
		{
			StreamBuffer currentBuffer = buffers[nextBufferIndex];
			isAbleToStream = currentBuffer.FillFromStream(musicStream);
			if (!isAbleToStream)
				return;
			source.SubmitSourceBuffer(currentBuffer.XAudioBuffer);
			nextBufferIndex = (nextBufferIndex + 1) % NumberOfBuffers;
		}

		private int nextBufferIndex;
		private bool isAbleToStream;

		protected override void DisposeData()
		{
			if (musicStream == null)
				return;

			base.DisposeData();
			musicStream = null;

			if (source != null)
				DisposeSource();
		}

		private void DisposeSource()
		{
			for (int i = 0; i < NumberOfBuffers; i++)
				buffers[i].Dispose();

			source.FlushSourceBuffers();
			source.Dispose();
			source = null;
		}

		public override float DurationInSeconds
		{
			get { return musicStream.LengthInSeconds; }
		}

		public override float PositionInSeconds
		{
			get
			{
				float seconds = (float)DateTime.Now.Subtract(playStartTime).TotalSeconds;
				return MathExtensions.Round(seconds.Clamp(0f, DurationInSeconds), 2);
			}
		}
	}
}