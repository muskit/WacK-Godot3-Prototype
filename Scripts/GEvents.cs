using Godot;
using System;

namespace WacK
{

	public partial class GEvents : Node
	{
		[Signal]
		// use to bring up loading screen
		public delegate void PreLoadingEventHandler();
		[Signal]
		// use just before loading Play scene
		public delegate void LoadingStartedEventHandler();
		[Signal]
		// emit a couple frames after completely loading the Play scene
		public delegate void LoadingFinishedEventHandler();

		[Signal]
		public delegate void PauseEventHandler();
		[Signal]
		public delegate void ResumeEventHandler();

		[Signal]
		public delegate void RhythmInputFireEventHandler(int segment, bool justTouched);
		[Signal]
		public delegate void NoteHitEventHandler(Note note);
		[Signal]
		public delegate void NoteMissEventHandler(Note note);

		public void TogglePause()
		{
			SetPause(!Misc.paused);
		}

		public void SetPause(bool state)
		{
			Misc.paused = state;
			if (state)
				EmitSignal(nameof(PauseEventHandler));
			else
				EmitSignal(nameof(ResumeEventHandler));
		}
	}
}
