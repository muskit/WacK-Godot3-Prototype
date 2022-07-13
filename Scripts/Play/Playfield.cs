/**
 * Playfield.cs
 * Handle scrolling notes.
 *
 * by muskit
 * July 1, 2022
 **/

using Godot;
using System.Collections.Generic;

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
    [Export]
    private NodePath npFeedbackCircle;
    [Export]
    private NodePath npHoldTexture;

    private Spatial scroll;
    private float syncRatio = 1;
    private Strikeline strikeline;
    private AudioStreamPlayer tickPlayer;
    private Area tickDetector;
    private List<FeedbackSegment> feedbackCircle = new List<FeedbackSegment>();
    private HoldSegmentsTexture holdTexture;

    private Node background;
    private int resyncCount = 0;

    public Playfield()
    {
        Misc.cameraOffset = Translation.z;
    }

    public override void _Ready()
    {
        var gEvents = GetNode<GEvents>("/root/GEvents");
        gEvents.Connect(nameof(GEvents.on_resume), this, nameof(Resync));

        scroll = GetNode<Spatial>(npScroll);
        strikeline = GetNode<Strikeline>(npStrikeline);
        tickPlayer = GetNode<AudioStreamPlayer>(npTickPlayer);
        tickDetector = GetNode<Area>(npTickDetector);
        holdTexture = GetNode<HoldSegmentsTexture>(npHoldTexture);

        //tickDetector.Scale = new Vector3(1, 1, PlaySettings.speedMultiplier * 10f);
        tickDetector.Connect("body_entered", this, nameof(OnTickEnter));


        background = FindNode("Background");
        foreach (var seg in background.GetChildren())
        {
            (seg as Spatial).Visible = false;
        }

        var feedbackSegmentsNode = GetNode(npFeedbackCircle);
        foreach (FeedbackSegment seg in feedbackSegmentsNode.GetChildren())
        {
            seg.Visible = false;
            feedbackCircle.Add(seg);
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

        // if (note.type != NoteType.HoldEnd &&
        //     note.type != NoteType.BGAdd &&
        //     note.type != NoteType.BGRem)
        // {
        //     int touchCenter = note.pos + note.size/2;
        //     feedbackCircle[touchCenter % 60].Fire();
        //     feedbackCircle[(touchCenter + 1) % 60].Fire();
        //     feedbackCircle[(touchCenter - 1) % 60].Fire();
        // }
    }

    public void Resync()
    {
        var audioTime = (float)(Misc.songPlayer.GetPlaybackPosition() + AudioServer.GetTimeSinceLastMix() - AudioServer.GetOutputLatency()); // audio time with lag compensation
        
        scroll.Translation = new Vector3(0, 0, -Misc.TimeToPosition(audioTime));
        holdTexture.SetPosition(Misc.TimeToPosition(audioTime));
    }

    public override void _Process(float delta)
    {
        // scroll if not paused
        if (!Misc.paused && NotesCreator.doneLoading && Misc.songPlayer.Playing)
        {
            scroll.Translate(new Vector3(0, 0, -Misc.TimeToPosition(delta * syncRatio)));
            holdTexture.Scroll(Misc.TimeToPosition(delta * syncRatio));

            // calculate scroll multiplier for keeping in sync
            var audioTime = (float)(Misc.songPlayer.GetPlaybackPosition() + AudioServer.GetTimeSinceLastMix() - AudioServer.GetOutputLatency()); // audio time with lag compensation
            var playTime = -Misc.PositionToTime(scroll.Translation.z);
            syncRatio = audioTime/playTime; // help prolong the need to resync.

            // force jerky resync if needed
            if (Mathf.Abs(playTime - audioTime) > 0.05f)
            {
                GD.Print($"Force resync #{++resyncCount}: {syncRatio}");
                Resync();
            }
        }
    }
}
