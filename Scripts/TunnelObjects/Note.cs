using Godot;
using System;

public enum Accuracy
{
    Miss, Good, Great, Marvelous
}
public class Note : Spatial
{
    [Export]
    public bool isEvent;
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

    private Color holdMissColor = new Color(0.5f, 0.5f, 0.5f, 0.96f);

    private GEvents gEvents;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        notePoly = GetChild<CSGPolygon>(0);
        gEvents = GetNode<GEvents>("/root/GEvents");
    }

    public async void SetPosSize(int pos = 0, int size = 1)
    {
        await ToSignal(this, "ready");

        this.pos = pos;
        this.size = size;
        if (type != NoteType.HoldStart && type != NoteType.HoldEnd && size >= 3 && size <= 59)
        {
            notePoly.Transform = notePoly.Transform.Rotated(Vector3.Forward, Mathf.Deg2Rad(6f * (pos + 1)));
            notePoly.SpinDegrees = 6f * (size - 2);
        }
        else
        {
            notePoly.Transform = notePoly.Transform.Rotated(Vector3.Forward, Mathf.Deg2Rad(6f * pos));
            notePoly.SpinDegrees = 6f * size;
        }
    }

    public void Miss()
    {
        gEvents.EmitSignal(nameof(GEvents.NoteMiss), this);

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

    public void Hit(Accuracy acc)
    {
        if (acc == Accuracy.Miss) { Miss(); return; }

        if (curAccuracy == Accuracy.Miss)
        {
            curAccuracy = acc;
            gEvents.EmitSignal(nameof(GEvents.NoteHit), this);
            FullDisable();
        }
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
