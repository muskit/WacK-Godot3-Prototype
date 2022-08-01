/**
 * ChartReader.cs
 * For the Play scene, handle placing notes on Scrolls from a Chart.
 *
 * by muskit
 * July 1, 2022
 **/

using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Collections.Generic;

namespace WacK
{
    public class ChartReader : Node
    {
        [Export]
        private NodePath npNoteScroll;
        [Export]
        private NodePath npMeasureScroll;
        [Export]
        private NodePath npHoldTexture;

        private Spatial noteScroll;
        private Spatial measureScroll;
        private TextureCone holdTexture;

        public static bool doneLoading { get; private set; } = false;
        // [zPos] = List<Note> (use list for chord marking creation)
        public SortedList<float, List<Note>> totalNotes { get; private set; }

        // Preloaded note types
        private static PackedScene measureLine = GD.Load<PackedScene>("res://Things/3D/TunnelObjects/MeasureLine.tscn");
        private static PackedScene noteTouch = GD.Load<PackedScene>("res://Things/3D/TunnelObjects/Notes/Touch.tscn");
        private static PackedScene noteHoldStart = GD.Load<PackedScene>("res://Things/3D/TunnelObjects/Notes/HoldStart.tscn");
        private static PackedScene noteInvisible = GD.Load<PackedScene>("res://Things/3D/TunnelObjects/Notes/Invisible.tscn");
        private static PackedScene noteHoldEnd = GD.Load<PackedScene>("res://Things/3D/TunnelObjects/Notes/HoldEnd.tscn");
        private static PackedScene noteUntimed = GD.Load<PackedScene>("res://Things/3D/TunnelObjects/Notes/Untimed.tscn");
        private static PackedScene noteSwipeIn = GD.Load<PackedScene>("res://Things/3D/TunnelObjects/Notes/SwipeIn.tscn");
        private static PackedScene noteSwipeOut = GD.Load<PackedScene>("res://Things/3D/TunnelObjects/Notes/SwipeOut.tscn");
        private static PackedScene noteSwipeCW = GD.Load<PackedScene>("res://Things/3D/TunnelObjects/Notes/SwipeCW.tscn");
        private static PackedScene noteSwipeCCW = GD.Load<PackedScene>("res://Things/3D/TunnelObjects/Notes/SwipeCCW.tscn");
        private static SpatialMaterial matHoldLine = GD.Load<SpatialMaterial>("res://Materials/HoldLine.tres");

        public ChartReader()
        {
            doneLoading = false;
        }

        public async override void _Ready()
        {
            noteScroll = GetNode<Spatial>(npNoteScroll);
            measureScroll = GetNode<Spatial>(npMeasureScroll);
            holdTexture = GetNode<TextureCone>(npHoldTexture);

            await ToSignal(holdTexture, "ready");
            doneLoading = false;
            Misc.currentChart = new Chart(Misc.currentMer);
            var scoreKeeper = GetNode<ScoreKeeper>("/root/ScoreKeeper");
            scoreKeeper.NewChart(Misc.currentChart);
            Load(new Chart(Misc.currentMer));
        }

        // place notes and events relative to the previous
        public void Load(Chart chart)
        {
            doneLoading = false;

            if (totalNotes != null)
            {
                foreach (KeyValuePair<float, List<Note>> obj in totalNotes.AsEnumerable())
                    foreach (Note n in obj.Value)
                    {
                        try
                        {
                            n?.QueueFree();
                        }
                        catch (ObjectDisposedException) { }
                    }
            }
            totalNotes = new SortedList<float, List<Note>>();

            List<float> tempo = new List<float>();
            List<int> tempoChangeMeasures = new List<int>();
            List<int> tempoChangeBeats = new List<int>();
            List<float> tempoChangePositions = new List<float>();
            List<int> beatsPerMeasure = new List<int>();
            List<int> bpmChangeMeasures = new List<int>();
            List<float> bpmChangePositions = new List<float>();

            tempo.Add(0);
            tempoChangeMeasures.Add(0);
            tempoChangeBeats.Add(0);
            tempoChangePositions.Add(0);
            beatsPerMeasure.Add(0);
            bpmChangeMeasures.Add(0);
            bpmChangePositions.Add(0);

            float queuedTempo = -1;
            int queuedBPM = -1;

            // timing info of the previous beat
            float prevTime = 0;
            float prevPosition = 0;
            int prevMeasure = 0;
            int prevBeat = 0; // (/1920 beats per measure)

            Note prevNote = null;
            Note curNote = null;
            var nextHoldNote = new System.Collections.Generic.Dictionary<int, Note>(); // <next hold idx, Note>
            var curHoldSegment = new System.Collections.Generic.Dictionary<int, Note>(); // <next hold idx, HoldStart>

            // Notes and Events //
            foreach (var measure in chart.notes) // <measure, List>
            {
                foreach (var chartNote in measure.Value) // List<beat, ChartNote>
                {
                    var curTime = prevTime + Util.NoteTime(measure.Key - prevMeasure, chartNote.Item1 - prevBeat, tempo.Last<float>(), beatsPerMeasure.Last<int>());
                    var curPos = prevPosition + Util.NotePosition(measure.Key - prevMeasure, chartNote.Item1 - prevBeat, tempo.Last<float>(), beatsPerMeasure.Last<int>());

                    if (prevMeasure != measure.Key && prevBeat != chartNote.Item1)
                    {
                        if (queuedTempo != -1)
                        {
                            tempo.Add(queuedTempo);
                            tempoChangeMeasures.Add(measure.Key);
                            tempoChangeBeats.Add(chartNote.Item1);
                            tempoChangePositions.Add(curPos);
                            queuedTempo = -1;
                        }

                        if (queuedBPM != -1)
                        {
                            beatsPerMeasure.Add(queuedBPM);
                            bpmChangeMeasures.Add(measure.Key);
                            queuedBPM = -1;
                        }
                    }

                    // notetype-dependent operations
                    switch (chartNote.Item2.noteType)
                    {
                        case NoteType.Tempo:
                            if (tempo.Count == 1)
                            {
                                tempo.Add(chartNote.Item2.value);
                                tempoChangeMeasures.Add(measure.Key);
                                tempoChangeBeats.Add(chartNote.Item1);
                                tempoChangePositions.Add(curPos);
                            }
                            else
                                queuedTempo = chartNote.Item2.value;
                            break;
                        case NoteType.BeatsPerMeasure:
                            if (beatsPerMeasure.Count == 1)
                            {
                                beatsPerMeasure.Add((int)chartNote.Item2.value);
                                bpmChangeMeasures.Add(measure.Key);
                                bpmChangePositions.Add(curPos);
                            }
                            else
                                queuedBPM = (int)chartNote.Item2.value;
                            break;
                        case NoteType.Touch:
                            curNote = noteTouch.Instance<Note>();
                            break;
                        case NoteType.HoldStart:
                            curNote = noteHoldStart.Instance<Note>();
                            curNote.noteIndex = chartNote.Item2.holdIdx;
                            nextHoldNote[chartNote.Item2.holdNextIdx] = curNote;
                            curHoldSegment[chartNote.Item2.holdNextIdx] = curNote;
                            break;
                        case NoteType.HoldMid:
                            curNote = noteInvisible.Instance<Note>();
                            curNote.type = NoteType.HoldMid;
                            curNote.noteIndex = chartNote.Item2.holdIdx;
                            nextHoldNote[chartNote.Item2.holdNextIdx] = curNote;
                            curHoldSegment[chartNote.Item2.holdNextIdx] = curHoldSegment[chartNote.Item2.holdIdx];
                            break;
                        case NoteType.HoldEnd: // TODO: draw end note on cone texture
                            curNote = noteHoldEnd.Instance<Note>();
                            curNote.noteIndex = chartNote.Item2.holdIdx;
                            break;
                        case NoteType.Untimed:
                            curNote = noteUntimed.Instance<Note>();
                            break;
                        case NoteType.SwipeIn:
                            curNote = noteSwipeIn.Instance<Note>();;
                            break;
                        case NoteType.SwipeOut:
                            curNote = noteSwipeOut.Instance<Note>();;
                            break;
                        case NoteType.SwipeCW:
                            curNote = noteSwipeCW.Instance<Note>();;
                            break;
                        case NoteType.SwipeCCW:
                            curNote = noteSwipeCCW.Instance<Note>();;
                            break;
                        default: // invisible modifier notes (aka events)
                            curNote = noteInvisible.Instance<Note>();
                            curNote.type = chartNote.Item2.noteType;
                            curNote.value = (int)chartNote.Item2.value;
                            break;
                    }

                    if (curNote != null && curNote != prevNote)
                    {
                        // curNote.AddChild(noteHitDetection.Instance());
                        curNote.SetPosSize(chartNote.Item2.position, chartNote.Item2.size);
                        noteScroll.AddChild(curNote);

                        curNote.Translation = new Vector3(0, 0, curPos);
                        prevNote = curNote;

                        // update "previous timing" info to place next note/event properly
                        prevTime = curTime;
                        prevPosition = curPos;
                        prevBeat = chartNote.Item1;
                        prevMeasure = measure.Key;

                        // hold notes
                        if (curNote.type == NoteType.HoldMid)
                        {
                            var pos = curNote.GlobalTransform;
                            noteScroll.RemoveChild(curNote);
                            curHoldSegment[curNote.noteIndex].AddChild(curNote);
                            curNote.GlobalTransform = pos;
                        }
                        if (curNote.type == NoteType.HoldEnd)
                        {
                            curHoldSegment[curNote.noteIndex].holdSegment = holdTexture.CreateLongNote(curHoldSegment[curNote.noteIndex], curNote);
                        }

                        if (!totalNotes.ContainsKey(curTime))
                        {
                            totalNotes[curTime] = new List<Note>();
                        }
                        totalNotes[curTime].Add(curNote);
                    }
                }
            }

            foreach (KeyValuePair<float, List<Note>> pair in totalNotes)
            {
                List<Note> chordableNotes = new List<Note>();
                foreach (Note n in pair.Value)
                {
                    if (!n.isEvent && n.type != NoteType.HoldEnd && n.type != NoteType.Untimed)
                        chordableNotes.Add(n);
                }
                if (chordableNotes.Count >= 2)
                {
                    // GD.Print($"chord @ {pair.Key}");
                    // TODO: draw chord indicators "Chordify"
                }
            }

            // Measure Lines //
            // TODO: adapt to tempo changes in the middle of a measure
            // int tempoIdx = 1;
            // int bpmIdx = 1;
            // for (int curMeasure = 0; curMeasure < chart.notes.Count; curMeasure++)
            // {
            //     while (curMeasure >= bpmChangeMeasures[bpmIdx] && bpmIdx < bpmChangeMeasures.Count - 1)
            //         ++bpmIdx;
            //     GD.Print($"{curMeasure}: {bpmIdx}");

            //     // last tempo change / only one tempo change exists
            //     if (tempoIdx == tempoChangeMeasures.Count - 1)
            //     {
            //         float pos = tempoChangePositions[tempoIdx] + Util.NotePosition(curMeasure - tempoChangeMeasures[tempoIdx], 0, tempo.Last(), beatsPerMeasure[bpmIdx]);
            //         var ml = measureLine.Instance<MeasureLine>();
            //         measureScroll.AddChild(ml);
            //         ml.Translation = new Vector3(0, 0, pos);
            //         ml.Text = $"{curMeasure}";
            //     }
            //     else if (tempoIdx < tempoChangeMeasures.Count)
            //     {
            //         // TODO: adapt to key signature changes
            //         while (curMeasure == tempoChangeMeasures[tempoIdx])
            //         {
            //             int measuresToCreate = tempoChangeMeasures[tempoIdx] - tempoChangeMeasures[tempoIdx - 1];
            //             for (int i = 0; i < measuresToCreate; ++i)
            //             {
            //                 int measureNum = tempoChangeMeasures[tempoIdx - 1] + i;
            //                 // GD.Print($"{tempoIdx} / {tempoChangePositions.Count}, {tempo.Count}");
            //                 float pos = Util.InterpFloat(tempoChangePositions[tempoIdx - 1], tempoChangePositions[tempoIdx], (float)i/measuresToCreate);

            //                 var ml = measureLine.Instance<MeasureLine>();
            //                 measureScroll.AddChild(ml);
            //                 ml.Translation = new Vector3(0, 0, pos);
            //                 ml.Text = $"{curMeasure}";
            //             }
            //             tempoIdx = Mathf.Clamp(tempoIdx + 1, 0, tempo.Count - 1);
            //         }
            //     }
            // }

            doneLoading = true;
        }
    }
}