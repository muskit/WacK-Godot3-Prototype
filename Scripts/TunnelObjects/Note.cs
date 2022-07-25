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
    
    public bool hasBeenInteracted { get; private set; } = false;
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
        if (size >= 3 && size <= 59)
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
        if (!hasBeenInteracted)
        {
            hasBeenInteracted = true;

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
            }

            FullDisable();
            gEvents.EmitSignal(nameof(GEvents.NoteMiss), this);
        }
    }

    public void Hit(Accuracy acc)
    {
        if (!hasBeenInteracted)
        {
            hasBeenInteracted = true;
            if (acc == Accuracy.Miss) { Miss(); return; }
            
            curAccuracy = acc;
            FullDisable();
            gEvents.EmitSignal(nameof(GEvents.NoteHit), this);
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
