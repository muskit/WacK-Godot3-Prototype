using Godot;
using System;

public class Play : Node
{
    [Export]
    private NodePath npAudioPlayer;
    [Export]
    private NodePath npPauseText;
    
    private Label pauseText;
    private float pauseTime;
    
    public override void _Ready()
    {
        Misc.songPlayer = GetNode<AudioStreamPlayer>(npAudioPlayer);
        Misc.songPlayer.Bus = "BGM";
        Misc.songPlayer.Stream = Misc.currentAudio;

        pauseText = GetNode<Label>(npPauseText);
        HandlePause(true);
    }

    private void HandlePause(bool state)
    {
        Misc.paused = state;
        if (state)
        {
            pauseTime = Misc.songPlayer.GetPlaybackPosition();
            Misc.songPlayer.Stop();
        }
        else
        {
            Misc.songPlayer.Play(pauseTime);
        }
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("pause"))
        {
            HandlePause(!Misc.paused);
        }
        if (Input.IsActionJustPressed("reset"))
        {
            Misc.songPlayer.Seek(0);
        }

        pauseText.Visible = Misc.paused;
    }
}
