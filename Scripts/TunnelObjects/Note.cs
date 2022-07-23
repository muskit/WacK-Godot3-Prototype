using Godot;
using System;

public enum Accuracy
{
    Miss, Good, Great, Marvelous
}
public class Note : Spatial
{
    [Export]
    public NoteType type;
    [Export]
    public Node2D holdSegment = null;
    
    private CSGPolygon notePoly;
    
    public Accuracy curAccuracy = Accuracy.Miss;
    public int pos = 0;
    public int size = 1;
    public int noteIndex = -1;
    public float value = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        notePoly = GetChild<CSGPolygon>(0);
    }

    public async void SetPosSize(int pos = 0, int size = 1)
    {
        await ToSignal(this, "ready");

        this.pos = pos;
        this.size = size;
        notePoly.Transform = notePoly.Transform.Rotated(Vector3.Forward, Mathf.Deg2Rad(6f * pos));
        notePoly.SpinDegrees = 6 * size;
    }

    public void Miss()
    {
        Misc.debugStr = $"{this.Name} missed";

        // HoldStart
        if (type == NoteType.HoldStart && holdSegment != null)
        {
            foreach(Polygon2D p2D in holdSegment.GetChildren())
            {
                p2D.Color = new Color(0.5f, 0.5f, 0.5f, 0.96f);
                foreach(Polygon2D p2DOffset in p2D.GetChildren())
                {
                    p2DOffset.Color = new Color(0.5f, 0.5f, 0.5f, 0.96f);
                }
            }
            return;
        }

        FullDisable();
    }

    private void FullDisable()
    {
        SetProcess(false);
        this.Visible = false;
    }

    public void Enable()
    {
        this.Visible = true;
        SetProcess(true);
    }

    public override void _Process(float delta)
    {
        this.Scale = Misc.NoteScale(this.GlobalTransform.origin.z, Misc.strikelineZPos);
    }
}
