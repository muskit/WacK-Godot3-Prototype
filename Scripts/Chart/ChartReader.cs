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
    private HoldNotesTexture holdTexture;

    public static bool doneLoading { get; private set; } = false;
    // [zPos] = List<Note> (chords)
    public SortedList<float, List<Note>> totalNotes { get; private set; }

    // Preloaded note types
    private static PackedScene measureLine = GD.Load<PackedScene>("res://Things/TunnelObjects/MeasureLine.tscn");
    private static PackedScene noteTouch = GD.Load<PackedScene>("res://Things/TunnelObjects/Notes/Touch.tscn");
    private static PackedScene noteHoldStart = GD.Load<PackedScene>("res://Things/TunnelObjects/Notes/HoldStart.tscn");
    private static PackedScene noteInvisible = GD.Load<PackedScene>("res://Things/TunnelObjects/Notes/Invisible.tscn");
    private static PackedScene noteHoldEnd = GD.Load<PackedScene>("res://Things/TunnelObjects/Notes/HoldEnd.tscn");
    private static PackedScene noteUntimed = GD.Load<PackedScene>("res://Things/TunnelObjects/Notes/Untimed.tscn");
    private static PackedScene noteSwipeIn = GD.Load<PackedScene>("res://Things/TunnelObjects/Notes/SwipeIn.tscn");
    private static PackedScene noteSwipeOut = GD.Load<PackedScene>("res://Things/TunnelObjects/Notes/SwipeOut.tscn");
    private static PackedScene noteSwipeCW = GD.Load<PackedScene>("res://Things/TunnelObjects/Notes/SwipeCW.tscn");
    private static PackedScene noteSwipeCCW = GD.Load<PackedScene>("res://Things/TunnelObjects/Notes/SwipeCCW.tscn");
    
    private static PackedScene noteHitDetection = GD.Load<PackedScene>("res://Things/TunnelObjects/Notes/HitDetection.tscn");
    private static SpatialMaterial matHoldLine = GD.Load<SpatialMaterial>("res://Materials/HoldLine.tres");

    public ChartReader()
    {
        doneLoading = false;
    }

    public async override void _Ready()
    {
        noteScroll = GetNode<Spatial>(npNoteScroll);
        measureScroll = GetNode<Spatial>(npMeasureScroll);
        holdTexture = GetNode<HoldNotesTexture>(npHoldTexture);

        await ToSignal(holdTexture, "ready");
        doneLoading = false;
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

        // TODO: implement these as Lists
        List<float> tempo = new List<float>();
        List<int> tempoChangeMeasures = new List<int>();
        List<int> beatsPerMeasure = new List<int>();
        List<int> bpmChangeMeasures = new List<int>();

        tempo.Add(0);
        tempoChangeMeasures.Add(0);
        beatsPerMeasure.Add(0);
        bpmChangeMeasures.Add(0);

        float queuedTempo = -1;
        int queuedBPM = -1;

        // timing info of the previous beat
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
                var curPos = prevPosition + Misc.NotePosition(measure.Key - prevMeasure, chartNote.Item1 - prevBeat, tempo.Last<float>(), beatsPerMeasure.Last<int>());

                if (prevMeasure != measure.Key && prevBeat != chartNote.Item1)
                {
                    tempo.Add(queuedTempo);
                    tempoChangeMeasures.Add(measure.Key);

                    beatsPerMeasure.Add(queuedBPM);
                    bpmChangeMeasures.Add(measure.Key);
                }

                // notetype-dependent operations
                switch (chartNote.Item2.noteType)
                {
                    case NoteType.Tempo:
                        if (tempo.Count == 1)
                        {
                            tempo.Add(chartNote.Item2.value);
                            tempoChangeMeasures.Add(measure.Key);
                        }
                        queuedTempo = chartNote.Item2.value;
                        break;
                    case NoteType.BeatsPerMeasure:
                        if (beatsPerMeasure.Count == 1)
                        {
                            beatsPerMeasure.Add((int)chartNote.Item2.value);
                            bpmChangeMeasures.Add(measure.Key);
                        }
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
                    case NoteType.HoldEnd:
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

                    // int key = 1920 * measure.Key + chartNote.Item1;
                    float key = Misc.PositionToTime(curPos);
                    if (!totalNotes.ContainsKey(key))
                    {
                        totalNotes[key] = new List<Note>();
                    }
                    totalNotes[key].Add(curNote);
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
        for (int curMeasure = 0; curMeasure < chart.notes.Count; ++curMeasure)
        {

        }

        doneLoading = true;
    }
}