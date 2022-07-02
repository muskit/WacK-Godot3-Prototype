/**
 * Playfield.cs
 * Handle scrolling notes.
 *
 * by muskit
 * July 1, 2022
 **/

using Godot;
using System;

public class Playfield : Spatial
{
    [Export]
    private NodePath npScroll;
    private Spatial scroll;
    public Playfield()
    {
        Misc.cameraOffset = Translation.z;
    }

    public override void _Ready()
    {
        scroll = GetNode<Spatial>(npScroll);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        // scroll
        scroll.Translate(new Vector3(0, 0, -PlayerPrefs.speedMultiplier * 10f * delta));
        
        // sync audio for 5s (don't do whole song to avoid jittery feel)
        if (Misc.songPlayer.GetPlaybackPosition() < 5f)
        {
            float audioTime = (float)(Misc.songPlayer.GetPlaybackPosition() + AudioServer.GetTimeSinceLastMix() - AudioServer.GetOutputLatency());
            if (Mathf.Abs(audioTime + scroll.Translation.z/PlayerPrefs.speedMultiplier) > 0.05f)
            {
                scroll.Translation = new Vector3(0, 0, -PlayerPrefs.speedMultiplier * 10f * audioTime);
            }
        }
    }
}
