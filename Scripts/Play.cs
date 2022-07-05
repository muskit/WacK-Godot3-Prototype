using Godot;
using System;

public class Play : Spatial
{
    [Export]
    private NodePath npAudioPlayer;
    [Export]
    private NodePath npPauseText;
    
    private AudioStreamPlayer audioPlayer;
    private Label pauseText;
    private float pauseTime;
    
    public override void _Ready()
    {
        audioPlayer = GetNode<AudioStreamPlayer>(npAudioPlayer);
        Misc.songPlayer = audioPlayer;
        audioPlayer.Stream = Misc.currentAudio;

        pauseText = GetNode<Label>(npPauseText);
        HandlePause(true);
    }

    private void HandlePause(bool state)
    {
        Misc.paused = state;
        if (state)
        {
            pauseTime = audioPlayer.GetPlaybackPosition();
            audioPlayer.Stop();
        }
        else
        {
            audioPlayer.Play(pauseTime);
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
            audioPlayer.Seek(0);
        }

        pauseText.Visible = Misc.paused;
    }
}
