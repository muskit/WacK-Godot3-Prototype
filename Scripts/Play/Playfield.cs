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
    [Export]
    private NodePath npChartReader;
    [Export]
    private NodePath npScroll;
    [Export]
    private NodePath npTickPlayer;
    [Export]
    private NodePath npMissTickPlayer;
    [Export]
    private NodePath npFeedbackCircle;
    [Export]
    private NodePath npHoldTexture;

    private ChartReader chartReader;

    private int nextTickBeatIndex;
    private int nextHittableBeatIndex;
    private float[] beatTimes;

    private Spatial scroll;
    private Node background;
    private HoldNotesTexture holdTexture;

    private AudioStreamPlayer hitTickPlayer;
    private AudioStreamPlayer missTickPlayer;
    private List<FeedbackSegment> feedbackCircle = new List<FeedbackSegment>();

    private float syncRatio = 1;
    private int resyncCount = 0;

    private bool setupDone = false;

    private GEvents gEvents;

    // Timing Windows (seconds)
    private const float TIMING_WINDOW_MARVELOUS = .05f;
    private const float TIMING_WINDOW_GREAT = 0.10f;
    private const float TIMING_WINDOW_GOOD = 0.15f;
    private const float TIMING_WINDOW_EARLYMISS = 0.17f;

    public Playfield()
    {
        Misc.cameraOffset = Translation.z;
    }

    public override async void _Ready()
    {
        gEvents = GetNode<GEvents>("/root/GEvents");
        gEvents.Connect(nameof(GEvents.OnResume), this, nameof(Resync));
        gEvents.Connect(nameof(GEvents.RhythmInputFire), this, nameof(OnCircleInputFire));
        gEvents.Connect(nameof(GEvents.NoteHit), this, nameof(OnNoteHit));
        gEvents.Connect(nameof(GEvents.NoteMiss), this, nameof(OnNoteMiss));

        scroll = GetNode<Spatial>(npScroll);
        hitTickPlayer = GetNode<AudioStreamPlayer>(npTickPlayer);
        missTickPlayer = GetNode<AudioStreamPlayer>(npMissTickPlayer);
        holdTexture = GetNode<HoldNotesTexture>(npHoldTexture);
        chartReader = GetNode<ChartReader>(npChartReader);

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

        // Wait for chart reader to finish loading
        await ToSignal(chartReader, "ready");
        while (!ChartReader.doneLoading)
        {
            await ToSignal(GetTree(), "idle_frame");
        }

        beatTimes = new float[chartReader.totalNotes.Keys.Count];
        chartReader.totalNotes.Keys.CopyTo(beatTimes, 0);
        nextTickBeatIndex = 0;

        setupDone = true;
    }

    public void Resync()
    {
        var audioTime = (float)(Misc.songPlayer.GetPlaybackPosition() + AudioServer.GetTimeSinceLastMix() - AudioServer.GetOutputLatency()); // audio time with lag compensation
        
        scroll.Translation = new Vector3(0, 0, -Misc.TimeToPosition(audioTime));
    }

    private void OnCircleInputFire(int segment, bool justTouched)
    {
        int curIndex = nextHittableBeatIndex;

        if (curIndex < beatTimes.Length)
        {
            int extraCheckIndex = 0;
            bool touchInteracted = false;
            float curHitDelta = Play.playbackTime - beatTimes[curIndex];

            while (!touchInteracted && curIndex < beatTimes.Length && (-TIMING_WINDOW_EARLYMISS <= curHitDelta && curHitDelta <= TIMING_WINDOW_GOOD))
            {
                curHitDelta = Play.playbackTime - beatTimes[curIndex];
                foreach (Note n in chartReader.totalNotes.Values[curIndex])
                {
                    if (!n.isEvent)
                    {
                        if (n.type == NoteType.Untimed || n.type == NoteType.HoldEnd) continue;

                        if (Misc.IsInSegmentRegion(n, segment)) // region check
                        {
                            // early miss
                            if (-TIMING_WINDOW_EARLYMISS <= curHitDelta && curHitDelta < -TIMING_WINDOW_GOOD && justTouched)
                            {
                                n.Miss();
                                touchInteracted = true;
                            }
                            // all encompassing hittable
                            else if (justTouched && -TIMING_WINDOW_GOOD <= curHitDelta && curHitDelta <= TIMING_WINDOW_GOOD)
                            {
                                // TODO: late windows should handle Untimed accordingly
                                touchInteracted = true;

                                // early good
                                if (-TIMING_WINDOW_GOOD <= curHitDelta && curHitDelta < -TIMING_WINDOW_GREAT)
                                    n.Hit(Accuracy.Good);
                                // early great
                                if (-TIMING_WINDOW_GREAT <= curHitDelta && curHitDelta < -TIMING_WINDOW_MARVELOUS)
                                    n.Hit(Accuracy.Great);
                                // marvelous
                                if (-TIMING_WINDOW_MARVELOUS <= curHitDelta && curHitDelta <= TIMING_WINDOW_MARVELOUS)
                                    n.Hit(Accuracy.Marvelous);
                                // late great
                                if (TIMING_WINDOW_MARVELOUS < curHitDelta && curHitDelta < TIMING_WINDOW_GREAT)
                                    n.Hit(Accuracy.Great);
                                // late good
                                if (TIMING_WINDOW_GREAT <= curHitDelta && curHitDelta <= TIMING_WINDOW_GOOD)
                                    n.Hit(Accuracy.Good);
                            }
                        }
                    }
                }
                extraCheckIndex++;
                curIndex = nextHittableBeatIndex + extraCheckIndex;
            }
        }
    }

    private void OnNoteHit(Note note)
    {
        hitTickPlayer.Play();
        Misc.debugStr = note.curAccuracy.ToString();
    }

    private void OnNoteMiss(Note note)
    {
        missTickPlayer.Play();
        Misc.debugStr = "Miss";
    }

    private void ProcessScroll(float delta)
    {
        if (!Misc.paused && ChartReader.doneLoading && Misc.songPlayer.Playing)
        {
            scroll.Translate(new Vector3(0, 0, -Misc.TimeToPosition(delta * syncRatio)));

            // calculate scroll multiplier for keeping in sync
            var audioTime = (float)(Misc.songPlayer.GetPlaybackPosition() + AudioServer.GetTimeSinceLastMix() - AudioServer.GetOutputLatency()); // audio time with lag compensation
            var posTime = -Misc.PositionToTime(scroll.Translation.z);
            Play.playbackTime = posTime;

            syncRatio = audioTime/posTime;

            // force jerky resync if needed
            if (Mathf.Abs(posTime - audioTime) > 0.05f)
            {
                GD.Print($"Force resync #{++resyncCount}: {syncRatio}");
                Resync();
            }
        }
        holdTexture.SetPosition(-scroll.Translation.z);
    }

    private void ProcessNoteTiming()
    {
        // perfect-timing processing
        while (nextTickBeatIndex < beatTimes.Length && Play.playbackTime >= beatTimes[nextTickBeatIndex])
        {
            float curBeatTime = beatTimes[nextTickBeatIndex];
            foreach (Note n in chartReader.totalNotes[curBeatTime])
            {
                if (!n.isEvent) // is visible (playable) note
                {
                    if (n.type == NoteType.Untimed)
                    {
                        foreach (int seg in RhythmInput.touchedSegments.Values)
                        {
                            if (Misc.IsInSegmentRegion(n, seg))
                                n.Hit(Accuracy.Marvelous);
                        }
                    }
                    else if (n.type == NoteType.HoldEnd) // TODO: associate HoldEnd with HoldStart (the whole hold)
                    {
                        n.Hit(Accuracy.Marvelous);
                    }
                }
                else // event note
                {
                    if (n.type == NoteType.BGAdd || n.type == NoteType.BGRem)
                    {
                        (background as Background).SetSegments(n.pos, n.size, n.type == NoteType.BGAdd, (DrawDirection) n.value);
                    }
                }
            }
            nextTickBeatIndex++;
        }
        // passed hit-window processing (miss "area")
        while (nextHittableBeatIndex < beatTimes.Length && Play.playbackTime - TIMING_WINDOW_GOOD > beatTimes[nextHittableBeatIndex])
        {
            float curBeatTime = beatTimes[nextHittableBeatIndex];
            foreach (Note n in chartReader.totalNotes[curBeatTime])
            {
                if (!n.isEvent && n.curAccuracy == Accuracy.Miss && n.type != NoteType.HoldEnd)
                {
                    n.Miss();
                }
            }
            nextHittableBeatIndex++;
        }
    }

    public override void _Process(float delta)
    {
        if (setupDone)
        {
            ProcessNoteTiming();
            ProcessScroll(delta);
        }
    }
}
