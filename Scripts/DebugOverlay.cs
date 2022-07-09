using Godot;
using System;

public class DebugOverlay : CanvasLayer
{
    private OrientationDetect orientationDetect;

    private Label fpsText;
    private Label orientationText;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        fpsText = FindNode("FPSLabel") as Label;
        orientationText = FindNode("OrientationLabel") as Label;
        orientationDetect = GetNode<OrientationDetect>("/root/OrientationDetect");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        fpsText.Text = $"{Engine.GetFramesPerSecond().ToString()} FPS";
        orientationText.Text = $"{orientationDetect.curOrientation}\n{OS.WindowSize}";
    }
}
