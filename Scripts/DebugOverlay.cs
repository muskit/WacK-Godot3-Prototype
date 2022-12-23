using Godot;
using System;

namespace WacK
{

    public partial class DebugOverlay : CanvasLayer
    {
        private OrientationDetect orientationDetect;

        private Label fpsText;
        private Label orientationText;
        private RichTextLabel debugText;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            orientationDetect = GetNode<OrientationDetect>("/root/OrientationDetect");
            
            fpsText = FindChild("FPSLabel") as Label;
            orientationText = FindChild("OrientationLabel") as Label;
            debugText = FindChild("DebugLabel") as RichTextLabel;
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
            fpsText.Text = $"{Engine.GetFramesPerSecond().ToString()} FPS";
            orientationText.Text = $"{OrientationDetect.curOrientation} ({OS.ScreenOrientation})\n{OS.WindowSize}";
            debugText.Text = Misc.debugStr;
        }
    }
}
