using Godot;
using System;

namespace WacK
{
    public partial class FeedbackSegment : TextureProgressBar
    {
        [Export]
        private float LingerTime;
        private double timer;
        private GEvents gEvents;

        public override void _Ready()
        {
            gEvents = GetNode<GEvents>("/root/GEvents");
        }

        public async void Fire(bool justTouched = true)
        {
            gEvents.EmitSignal(nameof(GEvents.RhythmInputFireEventHandler), GetIndex(), justTouched);
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
}
