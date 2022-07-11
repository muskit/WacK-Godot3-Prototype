using Godot;
using System;

public class Pause : CanvasLayer
{
    [Export]
    private NodePath npPauseBtn;

    private GEvents gEvents;
    private TextureButton pauseBtn;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        gEvents = GetNode<GEvents>("/root/GEvents");
        pauseBtn = GetNode<TextureButton>(npPauseBtn);
        pauseBtn.Connect("pressed", this, nameof(OnPauseKeyPress));
    }

    private void OnPauseKeyPress()
    {
        gEvents.TogglePause();
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
