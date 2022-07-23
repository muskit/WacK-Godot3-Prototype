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
    private CSGPolygon note;
    public int pos = 0;
    public int size = 1;
    public int noteIndex = -1;
    public float value = 0;
    public Accuracy curAccuracy = Accuracy.Miss;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        note = GetChild<CSGPolygon>(0);
    }

    public async void SetPosSize(int pos = 0, int size = 1)
    {
        await ToSignal(this, "ready");

        this.pos = pos;
        this.size = size;
        note.Transform = note.Transform.Rotated(Vector3.Forward, Mathf.Deg2Rad(6f * pos));
        note.SpinDegrees = 6 * size;
    }

    public void Miss()
    {
        // HoldStart
        if (type == NoteType.HoldStart)
        {
            foreach (Node longSegment in note.GetChildren())
            {
                foreach (var step in longSegment.GetChildren())
                {
                    if(step is CSGPolygon n)
                    {
                        ((SpatialMaterial)n.Material).AlbedoColor = new Color(.4f, .4f, .4f);
                        break;
                    }
                }
            }
            return;
        }

        // FullDisable();
    }

    private void FullDisable()
    {
        SetProcess(false);
        // this.Visible = false;
    }

    public void Enable()
    {
        this.Visible = true;

        // Re-enable physics
        foreach (Node n in GetChildren())
        {
            if (n is CollisionShape shape)
                shape.Disabled = false;
        }
        SetProcess(true);
    }

    public override void _Process(float delta)
    {
        this.Scale = Misc.NoteScale(this.GlobalTransform.origin.z, Misc.strikelineZPos);
    }
}
