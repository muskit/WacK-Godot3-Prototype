using Godot;
using System;

public class FeedbackSegment : TextureProgress
{
    [Export]
    private float LingerTime;
    private float timer;

    public async void Fire(bool justTouched = true)
    {
        Visible = true;
        timer = -GetProcessDeltaTime();
        while (timer < LingerTime)
        {
            timer += GetProcessDeltaTime();
            await ToSignal(GetTree(), "idle_frame");
        }
        Visible = false;
    }
}
