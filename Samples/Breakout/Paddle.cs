﻿using System.Collections.Generic;
using DeltaEngine.Commands;
using DeltaEngine.Content;
using DeltaEngine.Datatypes;
using DeltaEngine.Entities;
using DeltaEngine.Extensions;
using DeltaEngine.Input;
using DeltaEngine.Rendering.Sprites;

namespace Breakout
{
	/// <summary>
	/// Holds the paddle position
	/// </summary>
	public class Paddle : Sprite
	{
		public Paddle()
			: base(new Material(Shader.Position2DColorUv, "Paddle"), Rectangle.One)
		{
			RegisterInputCommands();
			Start<RunPaddle>();
			RenderLayer = 5;
		}

		private void RegisterInputCommands()
		{
			new Command(() => { xPosition -= PaddleMovementSpeed * Time.Delta; }).Add(
				new KeyTrigger(Key.CursorLeft, State.Pressed));
			new Command(() => { xPosition += PaddleMovementSpeed * Time.Delta; }).Add(
				new KeyTrigger(Key.CursorRight, State.Pressed));

			new Command(pos => { xPosition += pos.X - Position.X; }).Add(new MouseButtonTrigger(MouseButton.Left, State.Pressed));
			//inputCommands.Add(MouseButton.Left, State.Pressed,
			//	mouse => xPosition += mouse.Position.X - Position.X);
			//inputCommands.Add(State.Pressed, touch => xPosition += touch.GetPosition(0).X - Position.X);
			//inputCommands.Add(GamePadButton.Left, State.Pressed,
			//	() => xPosition -= PaddleMovementSpeed * Time.Delta);
			//inputCommands.Add(GamePadButton.Right, State.Pressed,
			//	() => xPosition += PaddleMovementSpeed * Time.Delta);
		}

		private float xPosition = 0.5f;
		private const float PaddleMovementSpeed = 1.5f;

		public class RunPaddle : UpdateBehavior
		{
			public override void Update(IEnumerable<Entity> entities)
			{
				foreach (var entity in entities)
				{
					var paddle = (Paddle)entity;
					var xPosition = paddle.xPosition.Clamp(HalfWidth, 1.0f - HalfWidth);
					paddle.DrawArea = Rectangle.FromCenter(xPosition, YPosition, Width, Height);
				}
			}
		}

		private const float YPosition = 0.9f;
		internal const float HalfWidth = Width / 2.0f;
		private const float Width = 0.2f;
		private const float Height = 0.04f;

		public Point Position
		{
			get { return new Point(DrawArea.Center.X, DrawArea.Top); }
		}
	}
}