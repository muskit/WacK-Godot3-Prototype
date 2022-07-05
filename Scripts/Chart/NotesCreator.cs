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
    private NodePath npScroll;
    private Spatial scroll;

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
        scroll = GetNode<Spatial>(npScroll);
        Load(new Chart(Misc.currentMer));
    }

    public void Load(Chart chart)
    {
        doneLoading = false;

        float currentTempo = -1;
        float currentBeatsPerMeasure = 4;

        Note lastNote = null;
        Note curNote = null;
        var nextHoldNote = new System.Collections.Generic.Dictionary<int, Note>(); // <idx of next hold note, Note>

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
                    // TODO: beats per measure
                    case NoteType.Tempo:
                        currentTempo = chartNote.Item2.value;
                        break;
                    // Playable notes //
                    case NoteType.Touch:
                        curNote = noteTouch.Instance<Note>();
                        break;
                    case NoteType.HoldStart:
                        curNote = noteHoldStart.Instance<Note>();
                        curNote.noteIndex = chartNote.Item2.holdIndex;
                        nextHoldNote[chartNote.Item2.holdNext] = curNote;
                        break;
                    case NoteType.HoldMid:
                        curNote = noteInvisible.Instance<Note>();
                        curNote.type = NoteType.HoldMid;
                        curNote.noteIndex = chartNote.Item2.holdIndex;
                        nextHoldNote[chartNote.Item2.holdNext] = curNote;
                        break;
                    case NoteType.HoldEnd:
                        curNote = noteHoldEnd.Instance<Note>();
                        curNote.noteIndex = chartNote.Item2.holdIndex;
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
                        if (curNote.type == NoteType.BGAdd || curNote.type == NoteType.BGRem)
                        {
                            GD.Print("Background note added!");
                        }
                        break;
                }
                if (curNote != lastNote)
                {
                    // add notes to scroll
                    scroll.AddChild(curNote);
                    curNote.Translation = new Vector3(0, 0, PlaySettings.speedMultiplier * 10f * 60f/currentTempo * currentBeatsPerMeasure * (measure.Key + (float)chartNote.Item1/1920f));
                    curNote.SetPosSize(chartNote.Item2.position, chartNote.Item2.size);
                    curNote.AddChild(noteHitDetection.Instance());

                    // create long note
                    if (curNote.type == NoteType.HoldMid || curNote.type == NoteType.HoldEnd)
                    {
                        nextHoldNote[curNote.noteIndex].AddChild(CreateLongNote(nextHoldNote[curNote.noteIndex], curNote));
                    }
                }
            }
        }
        // TODO: adapt to tempo changes
        for (int i = 0; i < 100; ++i)
        {
            var ml = measureLine.Instance<Spatial>();
            scroll.AddChild(ml);
            ml.Translation = new Vector3(0, 0, PlaySettings.speedMultiplier * 10f * 60f / currentTempo * currentBeatsPerMeasure * (float)i);
            ml.GetChild(2).GetChild<Label>(0).Text = $"{i}";
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
        float plus = destPosRad + 2f*Mathf.Pi;
        float minus = destPosRad - 2f*Mathf.Pi;
        float minusDelta = Mathf.Abs(minus - originPosRad);
        float normDelta = Mathf.Abs(destPosRad - originPosRad);
        float plusDelta = Mathf.Abs(plus - originPosRad);
        if (plusDelta < normDelta)
            destPosRad = plus;
        if (minusDelta < normDelta)
            destPosRad = minus;

        float noteDepth = destination.Translation.z - origin.Translation.z;

        float stepResolution = 200; // steps per player-scaled second
        float curveResolution = 1; // how many verts per curve segment
        int steps = (int) (Math.Ceiling(noteDepth)/10/PlaySettings.speedMultiplier*stepResolution);

        for(int step = 0; step < steps; ++step)
        {
            float stepRatio = (float)step/steps;
            float stepRatioOne = (step+1f)/steps;
            float curSize = Misc.FloatInterp(origin.size, destination.size, stepRatio);
            // float curSize = 4;
            float curRot = Misc.FloatInterp(originPosRad, destPosRad, stepRatio);

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
            poly.Mode = CSGPolygon.ModeEnum.Depth;
            poly.Polygon = totalVerts.ToArray<Vector2>();
            poly.Depth = noteDepth/steps;
            poly.Material = matHoldLine;
            poly.Translation = new Vector3(0, 0, noteDepth*stepRatio);
            poly.RotateY(Mathf.Pi);
            poly.RotateZ(curRot);
        }
        return result;
    }
}
