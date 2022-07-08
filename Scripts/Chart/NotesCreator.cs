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
    private Spatial noteScroll;
    private Spatial measureScroll;

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
        Load(new Chart(Misc.currentMer));
    }

    public void Load(Chart chart)
    {
        doneLoading = false;

        List<float> tempos = new List<float>();
        List<int> tempoChangeMeasures = new List<int>();
        List<int> tempoChangeBeats = new List<int>();
        List<float> tempoChangePositions = new List<float>();
        tempos.Add(-1);
        tempoChangeMeasures.Add(0);
        tempoChangeBeats.Add(0);
        tempoChangePositions.Add(0);

        float lastTempoChangePosition = 0;
        float currentTempo = -1;
        int currentBeatsPerMeasure = 4; // TODO: implement changing measures sizes
        int numMeasures = chart.notes.Count;

        Note lastNote = null;
        Note curNote = null;
        var nextHoldNote = new System.Collections.Generic.Dictionary<int, Note>(); // <next hold idx, Note>
        var curHoldSegment = new System.Collections.Generic.Dictionary<int, Note>(); // <next hold idx, HoldStart>

        foreach (var measure in chart.notes) // <measure, List>
        {
            foreach (var chartNote in measure.Value) // List<beat, ChartNote>
            {
                if (curNote != null)
                {
                    lastNote = curNote;
                }
                switch (chartNote.Item2.noteType)
                {
                    // Song-related info //
                    // TODO: beats per measure, tempo change based on relative note position
                    case NoteType.Tempo:
                        curNote = noteInvisible.Instance<Note>();
                        curNote.type = NoteType.Tempo;
                        lastTempoChangePosition += Misc.NotePosition(measure.Key - tempoChangeMeasures.Last<int>(), chartNote.Item1 - tempoChangeBeats.Last<int>(), currentTempo, currentBeatsPerMeasure);
                        tempos.Add(chartNote.Item2.value);
                        tempoChangeMeasures.Add(measure.Key);
                        tempoChangeBeats.Add(chartNote.Item1);
                        tempoChangePositions.Add(lastTempoChangePosition);
                        currentTempo = chartNote.Item2.value;
                        break;
                    // Playable notes //
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
                    default: // invisible modifier notes (ie. Background modifiers)
                        curNote = noteInvisible.Instance<Note>();
                        curNote.type = chartNote.Item2.noteType;
                        curNote.value = (int)chartNote.Item2.value;
                        break;
                }
                if (curNote != lastNote)
                {
                    if (curNote.type == NoteType.Tempo)
                    {
                        curNote.Translation = new Vector3(0, 0, lastTempoChangePosition);
                    }
                    else
                    {
                        noteScroll.AddChild(curNote);
                        curNote.Translation = new Vector3(0, 0, lastTempoChangePosition + Misc.NotePosition(measure.Key - tempoChangeMeasures.Last<int>(), chartNote.Item1 - tempoChangeBeats.Last<int>(), currentTempo, currentBeatsPerMeasure));
                        curNote.SetPosSize(chartNote.Item2.position, chartNote.Item2.size);
                        curNote.AddChild(noteHitDetection.Instance());
                    }

                    // create long note
                    if (curNote.type == NoteType.HoldMid || curNote.type == NoteType.HoldEnd)
                    {
                        // nextHoldNote[curNote.noteIndex].AddChild(CreateLongNote(nextHoldNote[curNote.noteIndex], curNote));

                        var stepGroup = CreateLongNote(nextHoldNote[curNote.noteIndex], curNote);
                        curHoldSegment[curNote.noteIndex].AddChild(stepGroup);
                        stepGroup.GlobalTransform = nextHoldNote[curNote.noteIndex].GlobalTransform;
                        
                        if (curNote.type == NoteType.HoldEnd)
                        {
                            var pos = curNote.GlobalTransform;
                            noteScroll.RemoveChild(curNote);
                            curHoldSegment[curNote.noteIndex].AddChild(curNote);
                            curNote.GlobalTransform = pos;
                        }
                    }

                    if (curNote.type == NoteType.HoldMid)
                        curNote.QueueFree();
                }
            }
        }
        foreach (var elem in tempoChangeMeasures)
        {
            GD.PrintRaw($"{elem} ");
        }
        GD.Print();
        foreach (var elem in tempoChangeBeats)
        {
            GD.PrintRaw($"{elem} ");
        }
        GD.Print();
        foreach (var elem in tempoChangePositions)
        {
            GD.PrintRaw($"{elem} ");
        }
        GD.Print();

        // TODO: adapt to tempo changes
        int tempoChgMeasureIdx = 1;
        for (int curMeasure = 0; curMeasure < numMeasures; ++curMeasure)
        {
            if (tempoChgMeasureIdx == tempoChangeMeasures.Count - 1) // last measure change; fill in the rest
            {
                float pos = tempoChangePositions[tempoChgMeasureIdx] + Misc.NotePosition(curMeasure - tempoChangeMeasures[tempoChgMeasureIdx], 0, tempos.Last(), 4);

                var ml = measureLine.Instance<Spatial>();
                measureScroll.AddChild(ml);
                ml.Translation = new Vector3(0, 0, pos);
                ml.GetChild(2).GetChild<Label>(0).Text = $"{curMeasure}";
                GD.Print($"{tempoChgMeasureIdx} = {ml.Translation}");
            }
            else if (tempoChgMeasureIdx < tempoChangeMeasures.Count)
            {
                while (curMeasure == tempoChangeMeasures[tempoChgMeasureIdx])
                {
                    int measuresToCreate = tempoChangeMeasures[tempoChgMeasureIdx] - tempoChangeMeasures[tempoChgMeasureIdx - 1];
                    for (int i = 0; i < measuresToCreate; ++i)
                    {
                        int measureNum = tempoChangeMeasures[tempoChgMeasureIdx - 1] + i;
                        float pos = Misc.InterpFloat(tempoChangePositions[tempoChgMeasureIdx - 1], tempoChangePositions[tempoChgMeasureIdx], (float)i/measuresToCreate);

                        var ml = measureLine.Instance<Spatial>();
                        measureScroll.AddChild(ml);
                        ml.Translation = new Vector3(0, 0, pos);
                        ml.GetChild(2).GetChild<Label>(0).Text = $"{measureNum}";
                        GD.Print($"{measureNum} = {ml.Translation}");
                    }
                    tempoChgMeasureIdx++;
                }
            }
            GD.Print();
        }
        doneLoading = true;
    }

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
        }
        return result;
    }
}
