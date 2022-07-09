/**
 * OrientationDetect.cs
 * Emit orientation-related signal when window is resized.
 *
 * by muskit
 * July 9, 2022
 **/

using Godot;

public class OrientationDetect : Node
{
    [Signal]
    public delegate void size_changed(string orientation);

    public string curOrientation { get; private set; }
    public override void _Ready()
    {
        GetTree().Root.Connect("size_changed",  this, nameof(OnScreenResize));
        OnScreenResize();
    }

    private void OnScreenResize()
    {
        var s = OS.WindowSize;

        if (s.x > s.y)
            curOrientation = "landscape";
        else
            curOrientation = "portrait";
        
        EmitSignal(nameof(size_changed), curOrientation);
    }
}