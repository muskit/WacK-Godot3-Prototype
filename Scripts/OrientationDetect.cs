/**
 * OrientationDetect.cs
 * Emits signal when window is resized.
 *
 * by muskit
 * July 9, 2022
 **/

using Godot;

public class OrientationDetect : Node
{
    [Signal]
    public delegate void size_changed(string orientation);

    public static string curOrientation { get; private set; }
    public static float scaleX { get; private set; }
    public static float scaleY { get; private set; }

    public static Vector2 minResolution = new Vector2(1024, 600);

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
        
        scaleX = s.x / minResolution.x;
        scaleY = s.y / minResolution.y;
        
        EmitSignal(nameof(size_changed), curOrientation);
    }
}