using Godot;
using System;

namespace WacK
{

    public class GEvents : Node
    {
        [Signal]
        // use to bring up loading screen
        public delegate void PreLoading();
        [Signal]
        // use just before loading Play scene
        public delegate void LoadingStarted();
        [Signal]
        // emit a couple frames after completely loading the Play scene
        public delegate void LoadingFinished();

        [Signal]
        public delegate void Pause();
        [Signal]
        public delegate void Resume();

        [Signal]
        public delegate void RhythmInputFire(int segment, bool justTouched);
        [Signal]
        public delegate void NoteHit(Note note);
        [Signal]
        public delegate void NoteMiss(Note note);

        public void TogglePause()
        {
            SetPause(!Misc.paused);
        }

        public void SetPause(bool state)
        {
            Misc.paused = state;
            if (state)
                EmitSignal(nameof(Pause));
            else
                EmitSignal(nameof(Resume));
        }
    }
}
