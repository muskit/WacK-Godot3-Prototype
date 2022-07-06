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
        note.Transform = note.Transform.Rotated(Vector3.Forward, Mathf.Deg2Rad(6f*pos));
        note.SpinDegrees = 6 * size;
    }

    public void SetPosSize(int pos = 0, int size = 1)
    {
        this.pos = pos;
        this.size = size;
    }
}
