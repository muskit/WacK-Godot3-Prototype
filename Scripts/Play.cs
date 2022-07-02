using Godot;
using System;

public class Play : Spatial
{
    [Export]
    private NodePath npAudioPlayer;

    private AudioStreamPlayer audioPlayer;
    public override void _Ready()
    {
        audioPlayer = GetNode<AudioStreamPlayer>(npAudioPlayer);
        Misc.songPlayer = audioPlayer;

        audioPlayer.Stream = Misc.currentAudio;
        audioPlayer.Play();
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
