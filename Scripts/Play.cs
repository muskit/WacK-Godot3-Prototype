using Godot;
using System;

public class Play : Spatial
{
    [Export]
    private NodePath npAudioPlayer;
    [Export]
    private NodePath npPauseText;
    private Label pauseText;
    private AudioStreamPlayer audioPlayer;
    public override void _Ready()
    {
        audioPlayer = GetNode<AudioStreamPlayer>(npAudioPlayer);
        Misc.songPlayer = audioPlayer;
        audioPlayer.Stream = Misc.currentAudio;

        pauseText = GetNode<Label>(npPauseText);
        
        audioPlayer.Play();
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
