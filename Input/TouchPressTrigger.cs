﻿using System;
using DeltaEngine.Commands;
using DeltaEngine.Extensions;

namespace DeltaEngine.Input
{
	/// <summary>
	/// Allows a simple touch to be detected.
	/// </summary>
	public class TouchPressTrigger : Trigger
	{
		public TouchPressTrigger(State state = State.Pressing)
		{
			State = state;
			Start<Touch>();
		}

		public State State { get; internal set; }

		public TouchPressTrigger(string state)
		{
			State = String.IsNullOrEmpty(state) ? State.Pressing : state.Convert<State>();
			Start<Touch>();
		}
	}
}