/**
 * NotesCreator.cs
 * Code for Node in Play scene that handles placing notes on the Scroll from a Chart.
 *
 * by muskit
 * July 1, 2022
 **/

using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Collections.Generic;

public class NotesCreator : Node
{
    [Export]
    private NodePath npNoteScroll;
    [Export]
    private NodePath npMeasureScroll;
    [Export]
    private NodePath npHoldTexture;

    private Spatial noteScroll;
    private Spatial measureScroll;
    private HoldSegmentsTexture holdTexture;

    public static bool doneLoading { get; private set; } = false;

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


    public override void _Ready()
    {
        noteScroll = GetNode<Spatial>(npNoteScroll);
        measureScroll = GetNode<Spatial>(npMeasureScroll);
        holdTexture = GetNode<HoldSegmentsTexture>(npHoldTexture);
        
        Load(new Chart(Misc.currentMer));
    }

    // place notes and events relative to the previous
    public void Load(Chart chart)
    {
        doneLoading = false;

        // TODO: implement these as Lists
        float curTempo = -1;
        int curBeatsPerMeasure = -1;

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

        GD.Print($"Beginning prevPosition: {prevPosition}");

        foreach (var measure in chart.notes) // <measure, List>
        {
            GD.Print(measure.Key);
            foreach (var chartNote in measure.Value) // List<beat, ChartNote>
            {
                var curPos = prevPosition + Misc.NotePosition(measure.Key - prevMeasure, chartNote.Item1 - prevBeat, curTempo, curBeatsPerMeasure);
                GD.Print($"{chartNote.Item1}: {chartNote.Item2.noteType} ({(int)chartNote.Item2.value})");

                if (prevMeasure != measure.Key && prevBeat != chartNote.Item1)
                {
                    curTempo = queuedTempo;
                    curBeatsPerMeasure = queuedBPM;
                }

                // notetype-dependent operations
                switch (chartNote.Item2.noteType)
                {
                    case NoteType.Tempo:
                        if (curTempo == -1)
                            curTempo = chartNote.Item2.value;
                        queuedTempo = chartNote.Item2.value;
                        break;
                    case NoteType.BeatsPerMeasure:
                        if (curBeatsPerMeasure == -1)
                            curBeatsPerMeasure = (int)chartNote.Item2.value;
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
                    curNote.AddChild(noteHitDetection.Instance());
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
                        holdTexture.CreateLongNote(curHoldSegment[curNote.noteIndex], curNote);
                        // TODO: associate HoldStart with holdSegment
                    }
                }
            }
        }

        doneLoading = true;
    }

    // 3D laggy method to create long notes
    private Spatial CreateLongNote(Note origin, Note destination)
    {
        Spatial result = new Spatial();

        var radius = .573f;
        var originPos = new Vector2(radius, 0);
        var originPosRad = -Misc.Seg2Rad(origin.pos);
        var destPosRad = -Misc.Seg2Rad(destination.pos);

        // look for closest destination angle to work towards
        destPosRad = Misc.NearestAngle(originPosRad, destPosRad);

        float noteDepth = destination.Translation.z - origin.Translation.z;

        float stepResolution = 200; // steps per player-scaled second
        float curveResolution = 1; // how many verts per curve segment
        int steps = (int) (Math.Ceiling(noteDepth)/10/PlaySettings.speedMultiplier*stepResolution);

        var mat = matHoldLine.Duplicate() as SpatialMaterial;

        for(int step = 0; step < steps; ++step)
        {
            float stepRatio = (float)step/steps;
            float stepRatioOne = (step+1f)/steps;
            float curSize = Misc.InterpFloat(origin.size, destination.size, stepRatio);
            // float curSize = 4;
            float curRot = Misc.InterpFloat(originPosRad, destPosRad, stepRatio);

            int numPoints = (int)Math.Ceiling(curSize * curveResolution);
            var innerVerts = new Array<Vector2>();
            // GD.Print($"curSize: {curSize}");
            innerVerts.Add(originPos);
            for(int radSeg = 1; radSeg < numPoints; ++radSeg)
            {
                innerVerts.Add(originPos.Rotated(Misc.Seg2Rad((float)radSeg/(numPoints - 1) * curSize)));
            }

            var outerVerts = new Array<Vector2>();
            for (int i = numPoints - 1; i >= 0; --i)
            {
                var vec = new Vector2(innerVerts[i]);
                outerVerts.Add(vec * 1.001f);
            }
            var totalVerts = innerVerts + outerVerts;

            CSGPolygon poly = new CSGPolygon();
            result.AddChild(poly);
            poly.Name = "LongBoi";
            poly.Mode = CSGPolygon.ModeEnum.Depth;
            poly.Polygon = totalVerts.ToArray<Vector2>();
            poly.Depth = noteDepth/steps;
            poly.Material = mat;
            poly.Translation = new Vector3(0, 0, noteDepth*stepRatio);
            poly.RotateY(Mathf.Pi);
            poly.RotateZ(curRot);
            poly.SetScript(GD.Load("res://Scripts/TunnelObjects/HoldStep.cs"));
        }
        return result;
    }
}