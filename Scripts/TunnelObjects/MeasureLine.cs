using Godot;
using System;

public class MeasureLine : Spatial
{
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        this.Scale = Misc.NoteScale(this.GlobalTransform.origin.z, Misc.strikelineZPos);
    }
}
