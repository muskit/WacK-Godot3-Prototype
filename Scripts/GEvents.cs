using Godot;
using System;

public class GEvents : Node
{
    [Signal]
    public delegate void on_pause();
    [Signal]
    public delegate void on_resume();

    public void TogglePause()
    {
        SetPause(!Misc.paused);
    }

    public void SetPause(bool state)
    {
        Misc.paused = state;
        if (state)
            EmitSignal(nameof(on_pause));
        else
            EmitSignal(nameof(on_resume));
    }
}
