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
        if (!Misc.paused)
        {
            scroll.Translate(new Vector3(0, 0, -PlayerPrefs.speedMultiplier * 10f * delta));

            // sync if necessary
            float audioTime = (float)(Misc.songPlayer.GetPlaybackPosition() + AudioServer.GetTimeSinceLastMix() - AudioServer.GetOutputLatency());
            var playTime = scroll.Translation.z/PlayerPrefs.speedMultiplier/10;
            if (Mathf.Abs(playTime + audioTime) > 0.05f)
            {
                GD.Print("Resynching!");
                scroll.Translation = new Vector3(0, 0, -PlayerPrefs.speedMultiplier * 10f * audioTime);
            }
        }
    }
}
