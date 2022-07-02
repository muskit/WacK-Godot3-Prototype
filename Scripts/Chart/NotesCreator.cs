/**
 * NotesCreator.cs
 * Code for object in Play scene that handles placing notes on the Scroll from a Chart.
 *
 * by muskit
 * July 1, 2022
 **/

using Godot;
using Godot.Collections;
using System;

public class NotesCreator : Node
{
    [Export]
    private NodePath npScroll;
    private Spatial scroll;

    // Preloaded note types
    PackedScene measureLine = GD.Load<PackedScene>("res://Things/TunnelObjects/MeasureLine.tscn");
    PackedScene noteTouch = GD.Load<PackedScene>("res://Things/TunnelObjects/Notes/Touch.tscn");
    PackedScene noteHoldStart = GD.Load<PackedScene>("res://Things/TunnelObjects/Notes/HoldStart.tscn");
    PackedScene noteHoldMid = GD.Load<PackedScene>("res://Things/TunnelObjects/Notes/HoldMid.tscn");
    PackedScene noteHoldEnd = GD.Load<PackedScene>("res://Things/TunnelObjects/Notes/HoldEnd.tscn");
    PackedScene noteUntimed = GD.Load<PackedScene>("res://Things/TunnelObjects/Notes/Untimed.tscn");
    PackedScene noteSwipeIn = GD.Load<PackedScene>("res://Things/TunnelObjects/Notes/SwipeIn.tscn");
    PackedScene noteSwipeOut = GD.Load<PackedScene>("res://Things/TunnelObjects/Notes/SwipeOut.tscn");
    PackedScene noteSwipeCW = GD.Load<PackedScene>("res://Things/TunnelObjects/Notes/SwipeCW.tscn");
    PackedScene noteSwipeCCW = GD.Load<PackedScene>("res://Things/TunnelObjects/Notes/SwipeCCW.tscn");
    SpatialMaterial matHoldLine = GD.Load<SpatialMaterial>("res://Materials/HoldLine.tres");


    public override void _Ready()
    {
        scroll = GetNode<Spatial>(npScroll);
        Load(new Chart(Misc.currentMer));
    }

    public void Load(Chart chart)
    {
        float currentTempo = -1;
        float currentBeatsPerMeasure = 4;

        Note lastHoldNote = null;
        Note lastNote = null;
        Note curNote = null;
        foreach (var measure in chart.notes) // <measure, List>
        {
            foreach (var note in measure.Value) // List<beat, Note>
            {
                if (curNote != null)
                {
                    lastNote = curNote;
                }

                switch (note.Item2.noteType)
                {
                    // Song-related info //
                    // TODO: beats per measure
                    case NoteType.Tempo:
                        currentTempo = note.Item2.value;
                        break;
                    // Background control //
                    case NoteType.BGAdd:
                        break;
                    case NoteType.BGRem:
                        break;
                    // Playable notes //
                    case NoteType.Touch:
                        curNote = noteTouch.Instance<Note>();
                        break;
                    case NoteType.HoldStart: // draw hold lines at radius 0.58
                        curNote = noteHoldStart.Instance<Note>();
                        lastHoldNote = curNote;
                        break;
                    case NoteType.HoldMid:
                        curNote = noteHoldMid.Instance<Note>();
                        break;
                    case NoteType.HoldEnd:
                        curNote = noteHoldEnd.Instance<Note>();
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
                }
                if (curNote != lastNote)
                {
                    // add notes to scroll
                    scroll.AddChild(curNote);
                    curNote.Translation = new Vector3(0, 0, PlayerPrefs.speedMultiplier * 10f * 60f/currentTempo * currentBeatsPerMeasure * (measure.Key + (float)note.Item1/1920f));
                    curNote.SetPosSize(note.Item2.position, note.Item2.size);

                    if (curNote.type == NoteType.HoldMid || curNote.type == NoteType.HoldEnd) // draw hold lines (END or MID ONLY)
                    {
                        GD.Print("Drawing hold line!");
                        var originA = new Vector2(0.58f, 0);
                        originA = originA.Rotated(Misc.Seg2Rad(lastHoldNote.pos));
                        var originB = new Vector2(0.58f, 0);
                        originB = originB.Rotated(Misc.Seg2Rad(lastHoldNote.pos + lastHoldNote.size));
                        
                        var destA = new Vector2(0.58f, 0);
                        destA = destA.Rotated(Misc.Seg2Rad(curNote.pos));
                        var destB = new Vector2(0.58f, 0);
                        destB = originB.Rotated(Misc.Seg2Rad(curNote.pos + curNote.size));

                        var originA3D = new Vector3(originA.x, originA.y, 0);
                        var originB3D = new Vector3(originB.x, originB.y, 0);
                        var destA3D = new Vector3(destA.x, destA.y, curNote.Translation.z - lastHoldNote.Translation.z);
                        var destB3D = new Vector3(destB.x, destB.y, curNote.Translation.z - lastHoldNote.Translation.z);

                        Vector3[] verts = { originA3D, originB3D, destB3D, destA3D };
                        var arrMesh = new ArrayMesh();
                        var vertArrs = new Godot.Collections.Array();
                        vertArrs.Resize(9); // values taken from ArrayMesh docs
                        vertArrs[0] = vertArrs;
                        arrMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, vertArrs);
                        var mi = new MeshInstance();
                        mi.Mesh = arrMesh;
                        mi.MaterialOverride = matHoldLine;
                        scroll.AddChild(mi);
                        mi.Translation = new Vector3(0, 0, curNote.Translation.z);
                        lastHoldNote = curNote;
                    }
                }
            }
        }
        // TODO: adapt to tempo changes
        for (int i = 0; i < 100; ++i)
        {
            var ml = measureLine.Instance<Spatial>();
            scroll.AddChild(ml);
            ml.Translation = new Vector3(0, 0, PlayerPrefs.speedMultiplier * 10f * 60f / currentTempo * currentBeatsPerMeasure * (float)i);
            ml.GetChild(2).GetChild<Label>(0).Text = $"{i}";
        }
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
