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
    [Signal]
    public delegate void note_tick(Note note);

    [Export]
    private NodePath npScroll;
    [Export]
    private NodePath npStrikeline;
    [Export]
    private NodePath npTickPlayer;
    [Export]
    private NodePath npTickDetector;

    private Spatial scroll;
    private float syncRatio = 1;
    private Strikeline strikeline;
    private AudioStreamPlayer tickPlayer;
    private Area tickDetector;
    private Node background;
    private int resyncCount = 0;

    public Playfield()
    {
        Misc.cameraOffset = Translation.z;
    }

    public override void _Ready()
    {
        var singleton = GetNode<Singleton>("/root/Singleton");

        scroll = GetNode<Spatial>(npScroll);
        strikeline = GetNode<Strikeline>(npStrikeline);
        tickPlayer = GetNode<AudioStreamPlayer>(npTickPlayer);
        tickDetector = GetNode<Area>(npTickDetector);

        //tickDetector.Scale = new Vector3(1, 1, PlaySettings.speedMultiplier * 10f);
        tickDetector.Connect("body_entered", this, nameof(OnTickEnter));

        singleton.Connect("on_resume", this, nameof(Resync));

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
        // EmitSignal(nameof(note_tick), note);
        // GD.Print($"{note.Name} ({note.type})");

        if (note.type == NoteType.BGAdd || note.type == NoteType.BGRem)
        {
            (background as Background).SetSegments(note.pos, note.size, note.type == NoteType.BGAdd, (DrawDirection) note.value);
        }

        if (note.type != NoteType.HoldMid &&
            note.type != NoteType.HoldEnd &&
            note.type != NoteType.BGAdd &&
            note.type != NoteType.BGRem)
        {
            tickPlayer.Stop();
            tickPlayer.Play();
        }
    }

    public void Resync()
    {
        var audioTime = (float)(Misc.songPlayer.GetPlaybackPosition() + AudioServer.GetTimeSinceLastMix() - AudioServer.GetOutputLatency()); // audio time with lag compensation
        scroll.Translation = new Vector3(0, 0, -PlaySettings.speedMultiplier * 10f * audioTime);
    }

    public override void _Process(float delta)
    {
        // scroll if not paused
        if (!Misc.paused && NotesCreator.doneLoading && Misc.songPlayer.Playing)
        {
            scroll.Translate(new Vector3(0, 0, -PlaySettings.speedMultiplier * 10f * delta * syncRatio));

            // calculate scroll multiplier for keeping in sync
            var audioTime = (float)(Misc.songPlayer.GetPlaybackPosition() + AudioServer.GetTimeSinceLastMix() - AudioServer.GetOutputLatency()); // audio time with lag compensation
            var playTime = -scroll.Translation.z/PlaySettings.speedMultiplier/10;
            syncRatio = audioTime/playTime; // help prolong the need to resync.

            // force jerky resync if needed
            if (Mathf.Abs(playTime - audioTime) > 0.15f)
            {
                GD.Print($"Force resync #{++resyncCount}: {syncRatio}");
                Resync();
            }
        }
    }
}
