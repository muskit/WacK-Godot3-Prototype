using Godot;
using System;

namespace WacK
{

    public class DebugOverlay : CanvasLayer
    {
        private OrientationDetect orientationDetect;

        private Label fpsText;
        private Label orientationText;
        private RichTextLabel debugText;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            orientationDetect = GetNode<OrientationDetect>("/root/OrientationDetect");
            
            fpsText = FindNode("FPSLabel") as Label;
            orientationText = FindNode("OrientationLabel") as Label;
            debugText = FindNode("DebugLabel") as RichTextLabel;
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(float delta)
        {
            fpsText.Text = $"{Engine.GetFramesPerSecond().ToString()} FPS";
            orientationText.Text = $"{OrientationDetect.curOrientation} ({OS.ScreenOrientation})\n{OS.WindowSize}";
            debugText.Text = Misc.debugStr;
        }
    }
}
