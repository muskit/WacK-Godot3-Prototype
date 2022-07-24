using Godot;
using System;

public class GEvents : Node
{
    [Signal]
    public delegate void OnPause();
    [Signal]
    public delegate void OnResume();

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
            EmitSignal(nameof(OnPause));
        else
            EmitSignal(nameof(OnResume));
    }
}
