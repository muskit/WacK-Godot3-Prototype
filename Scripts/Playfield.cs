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
    [Export]
    private NodePath npStrikeline;
    [Export]
    private NodePath npTickPlayer;
    [Export]
    private NodePath npTickDetector;

    private Spatial scroll;
    private float syncRatio = -1;
    private Strikeline strikeline;
    private AudioStreamPlayer tickPlayer;
    private Area tickDetector;
    private Node background;

    public Playfield()
    {
        Misc.cameraOffset = Translation.z;
    }

    public override void _Ready()
    {
        scroll = GetNode<Spatial>(npScroll);
        strikeline = GetNode<Strikeline>(npStrikeline);
        tickPlayer = GetNode<AudioStreamPlayer>(npTickPlayer);
        tickDetector = GetNode<Area>(npTickDetector);

        tickDetector.Scale = new Vector3(1, 1, PlaySettings.speedMultiplier * 10f);
        tickDetector.Connect("body_entered", this, nameof(OnTickEnter));

        background = FindNode("Background");
        foreach (var seg in background.GetChildren())
        {
            (seg as Spatial).Visible = false;
        }
    }

    // tick-accurate handler
    private void OnTickEnter(Node obj)
    {
        var note = obj.GetParent() as Note;

        if (note.type == NoteType.BGAdd || note.type == NoteType.BGRem)
        {
            for (int i = 0; i < note.size; ++i)
            {
                background.GetChild<Spatial>((i + note.pos)%60).Visible = (note.type == NoteType.BGAdd);
            }
        }

        if (note.type != NoteType.HoldMid)
        {
            tickPlayer.Stop();
            tickPlayer.Play();
        }
    }

    public override void _Process(float delta)
    {
        // scroll
        if (!Misc.paused && NotesCreator.doneLoading)
        {
            scroll.Translate(new Vector3(0, 0, -PlaySettings.speedMultiplier * 10f * delta * -syncRatio));

            // synchronization
            var audioTime = (float)(Misc.songPlayer.GetPlaybackPosition() + AudioServer.GetTimeSinceLastMix() - AudioServer.GetOutputLatency()); // audio time with lag compensation
            var playTime = scroll.Translation.z/PlaySettings.speedMultiplier/10;
            syncRatio = playTime/audioTime; // help prolong the need to resync
            // forced jerky resync
            if (Mathf.Abs(playTime + audioTime) > 0.05f)
            {
                GD.Print("Resynching!");
                scroll.Translation = new Vector3(0, 0, -PlaySettings.speedMultiplier * 10f * audioTime);
            }
        }
    }
}
