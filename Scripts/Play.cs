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
    
    public override void _Ready()
    {
        // Engine.IterationsPerSecond = 120;
        audioPlayer = GetNode<AudioStreamPlayer>(npAudioPlayer);
        Misc.songPlayer = audioPlayer;
        audioPlayer.Stream = Misc.currentAudio;
        audioPlayer.Play();

        pauseText = GetNode<Label>(npPauseText);
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("pause"))
        {
            Misc.paused = !Misc.paused;
            audioPlayer.StreamPaused = Misc.paused;
        }
        if (Input.IsActionJustPressed("reset"))
        {
            audioPlayer.Stop();
            audioPlayer.Play();
        }

        pauseText.Visible = Misc.paused;
    }
}
