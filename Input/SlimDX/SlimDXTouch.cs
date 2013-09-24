﻿using System.Collections.Generic;
using DeltaEngine.Datatypes;
using DeltaEngine.Entities;

namespace DeltaEngine.Input.SlimDX
{
	/// <summary>
	/// SlimDX does not support any touch input devices.
	/// </summary>
	public sealed class SlimDXTouch : Touch
	{
		public SlimDXTouch()
		{
			IsAvailable = false;
		}

		public override void Dispose() {}
		public override bool IsAvailable { get; protected set; }

		public override Vector2D GetPosition(int touchIndex)
		{
			return new Vector2D();
		}

		public override State GetState(int touchIndex)
		{
			return State.Released;
		}

		public override void Update(IEnumerable<Entity> entities) {}
	}
}