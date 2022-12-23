/**
 * Playfield.cs
 * Handle scrolling notes.
 *
 * by muskit
 * July 1, 2022
 **/

using Godot;
using System.Collections.Generic;
namespace WacK
{
    public partial class Playfield : Node3D
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

        private Node3D scroll;
        private Node background;
        private TextureCone holdTexture;

        private AudioStreamPlayer hitTickPlayer;
        private AudioStreamPlayer missTickPlayer;
        private List<FeedbackSegment> feedbackCircle = new List<FeedbackSegment>();

        private float syncRatio = 1;
        private int resyncCount = 0;

        private bool setupDone = false;

        private GEvents gEvents;

        // Timing Windows (seconds)
        public const float TIMING_WINDOW_MARVELOUS = .05f;
        public const float TIMING_WINDOW_GREAT = 0.10f;
        public const float TIMING_WINDOW_GOOD = 0.15f;
        public const float TIMING_WINDOW_EARLYMISS = 0.17f;

        public Playfield()
        {
            
        }

        public override async void _Ready()
        {
            Misc.cameraOffset = Position.z;
            Misc.strikelineZPos = GlobalTransform.origin.z;

            gEvents = GetNode<GEvents>("/root/GEvents");
            gEvents.Connect(nameof(GEvents.ResumeEventHandler),new Callable(this,nameof(Resync)));
            gEvents.Connect(nameof(GEvents.RhythmInputFireEventHandler),new Callable(this,nameof(OnCircleInputFire)));
            gEvents.Connect(nameof(GEvents.NoteHitEventHandler),new Callable(this,nameof(OnNoteHit)));
            gEvents.Connect(nameof(GEvents.NoteMissEventHandler),new Callable(this,nameof(OnNoteMiss)));

            scroll = GetNode<Node3D>(npScroll);
            hitTickPlayer = GetNode<AudioStreamPlayer>(npTickPlayer);
            missTickPlayer = GetNode<AudioStreamPlayer>(npMissTickPlayer);
            holdTexture = GetNode<TextureCone>(npHoldTexture);
            chartReader = GetNode<ChartReader>(npChartReader);

            background = FindChild("Background");
            foreach (var seg in background.GetChildren())
            {
                (seg as Node3D).Visible = false;
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
            GD.Print($"totalNotes={chartReader.totalNotes}");
            chartReader.totalNotes.Keys.CopyTo(beatTimes, 0);
            nextTickBeatIndex = 0;

            setupDone = true;
        }

        public void Resync()
        {
            var audioTime = (float)(Misc.songPlayer.GetPlaybackPosition() + AudioServer.GetTimeSinceLastMix() - AudioServer.GetOutputLatency()); // audio time with lag compensation
            
            scroll.Position = new Vector3(0, 0, -Util.TimeToPosition(audioTime));
        }

        private void OnCircleInputFire(int segment, bool justTouched)
        {
            int curIndex = nextHittableBeatIndex;
            GD.Print($"{curIndex}, {beatTimes}");
            if (curIndex < beatTimes.Length)
            {
                int extraCheckIndex = 0;
                bool touchInteracted = false;
                float curHitDelta = Play.playbackTime - beatTimes[curIndex];
                
                while (curHitDelta > TIMING_WINDOW_GOOD && curIndex < beatTimes.Length - 1)
                {
                    extraCheckIndex++;
                    curIndex = nextHittableBeatIndex + extraCheckIndex;
                    curHitDelta = Play.playbackTime - beatTimes[curIndex];
                }

                // note already interacted check; interactable timing range check
                while (!touchInteracted && curIndex < beatTimes.Length && (-TIMING_WINDOW_GOOD <= curHitDelta && curHitDelta <= TIMING_WINDOW_GOOD))
                {
                    foreach (Note n in chartReader.totalNotes.Values[curIndex])
                    {
                        if (!n.isEvent && Misc.NoteIsInSegmentRegion(n, segment) && !n.hasBeenProcessed) // non-event + region check
                        {
                            // all encompassing hittable (interactable timing range)
                            if (-TIMING_WINDOW_GOOD <= curHitDelta && curHitDelta <= TIMING_WINDOW_GOOD)
                            {
                                switch (n.type)
                                {
                                    case NoteType.Touch: case NoteType.HoldStart:
                                        if (justTouched)
                                        {
                                            touchInteracted = true;
                                            n.Hit(curHitDelta);
                                        }
                                        break;
                                    case NoteType.SwipeCCW: case NoteType.SwipeCW:
                                    case NoteType.SwipeIn: case NoteType.SwipeOut:
                                        touchInteracted = true;
                                        // TODO: check for swipe motion
                                        // early to marvelous
                                        if (-TIMING_WINDOW_GOOD <= curHitDelta && curHitDelta <= TIMING_WINDOW_MARVELOUS)
                                        {
                                            n.noteSwiped = true;
                                        }
                                        else
                                        {
                                            n.Hit(curHitDelta);
                                        }
                                        break;
                                    case NoteType.Untimed:
                                        // late
                                        if (TIMING_WINDOW_MARVELOUS < curHitDelta && curHitDelta <= TIMING_WINDOW_GOOD)
                                        {
                                            touchInteracted = true;
                                            n.Hit(curHitDelta);
                                        }
                                        break;
                                }
                            }
                        }
                    }
                    extraCheckIndex++;
                    curIndex = nextHittableBeatIndex + extraCheckIndex;
                    if (curIndex < beatTimes.Length)
                        curHitDelta = Play.playbackTime - beatTimes[curIndex];
                }
            }
        }

        private void OnNoteHit(Note note)
        {
            hitTickPlayer.Play();
        }

        private void OnNoteMiss(Note note)
        {
            // missTickPlayer.Play();
        }

        private void ProcessScroll(float delta)
        {
            if (!Misc.paused && ChartReader.doneLoading && Misc.songPlayer.Playing)
            {
                scroll.Translate(new Vector3(0, 0, -Util.TimeToPosition(delta * syncRatio)));

                // calculate scroll multiplier for keeping in sync
                var audioTime = (float)(Misc.songPlayer.GetPlaybackPosition() + AudioServer.GetTimeSinceLastMix() - AudioServer.GetOutputLatency()); // audio time with lag compensation
                var posTime = -Util.PositionToTime(scroll.Position.z);
                Play.playbackTime = posTime;

                syncRatio = audioTime/posTime;

                // force jerky resync if needed
                if (Mathf.Abs(posTime - audioTime) > 0.05f)
                {
                    Misc.DebugPrintln($"Force resync #{++resyncCount}: {syncRatio}");
                    Resync();
                }
            }
            holdTexture.SetPosition(-scroll.Position.z);
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
                                if (Misc.NoteIsInSegmentRegion(n, seg))
                                    n.Hit(Accuracy.Marvelous);
                            }
                        }
                        else if (n.noteSwiped && !n.hasBeenProcessed) // swipe note
                        {
                            n.Hit(Accuracy.Marvelous);
                        }
                        else if (n.type == NoteType.HoldEnd) // TODO: associate HoldEnd with HoldStart (the whole hold)
                        {
                            // n.Hit(Accuracy.Marvelous);
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
                    if (!n.isEvent && !n.hasBeenProcessed && n.curAccuracy == Accuracy.Miss && n.type != NoteType.HoldEnd)
                    {
                        n.Miss();
                    }
                }
                nextHittableBeatIndex++;
            }
        }

        public override void _Process(double delta)
        {
            if (setupDone)
            {
                ProcessNoteTiming();
                ProcessScroll(delta);
            }
        }
    }
}