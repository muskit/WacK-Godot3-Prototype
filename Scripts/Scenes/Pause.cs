using Godot;
using System;

namespace WacK
{
    public partial class Pause : CanvasLayer
    {
        [Export]
        private NodePath npPauseBtn;
        [Export]
        private NodePath npMenuBtn;

        private GEvents gEvents;
        private TextureButton pauseBtn;
        private TextureButton menuBtn;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            gEvents = GetNode<GEvents>("/root/GEvents");
            pauseBtn = GetNode<TextureButton>(npPauseBtn);
            pauseBtn.Connect("pressed",new Callable(this,nameof(OnPauseKeyPress)));
            menuBtn = GetNode<TextureButton>(npMenuBtn);
            menuBtn.Connect("pressed",new Callable(this,nameof(OnMenuKeyPress)));
        }

        private void OnPauseKeyPress()
        {
            gEvents.TogglePause();
        }

        private void OnMenuKeyPress()
        {
            GetTree().ChangeSceneToFile("res://Scenes/Menus/SongSelection.tscn");
        }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(double delta)
    //  {
    //      
    //  }
    }
}