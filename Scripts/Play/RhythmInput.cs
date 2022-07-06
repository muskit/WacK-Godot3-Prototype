/**
 * RhythmInput.cs
 * Handle play-related input.
 *
 * by muskit
 * July 5, 2022
 **/

using Godot;
using System.Collections.Generic;

public class RhythmInput : Node
{
    private Dictionary<int, Vector2> touches = new Dictionary<int, Vector2>();

    public override void _Ready()
    {
    }
    public override void _Input(InputEvent inputEvent)
    {
        if (inputEvent is InputEventScreenTouch touchEv)
        {
            if (touchEv.IsPressed()) // begin touch
            {
                JustTouched(touchEv);
            }
            else // release touch
            {
                Untouch(touchEv);
            }
        }
        if (inputEvent is InputEventScreenDrag dragEv) // dragging touch
        {
            DragTouch(dragEv);
        }
    }

    // for Touch and HoldStart notes
    private void JustTouched(InputEventScreenTouch touchEv)
    {
        GD.Print($"JustTouched {touchEv.Index} @ {Misc.ScreenPixelToRad(touchEv.Position)} ({Misc.ScreenPixelToSegmentInt(touchEv.Position)})");
        touches[touchEv.Index] = touchEv.Position;
    }
    private void DragTouch(InputEventScreenDrag dragEv)
    {
        GD.Print($"Dragging {dragEv.Index} @ {Misc.ScreenPixelToRad(dragEv.Position)}");
        touches[dragEv.Index] = dragEv.Position;
    }
    private void Untouch(InputEventScreenTouch touchEv)
    {
        GD.Print($"Untouch {touchEv.Index} @ {Misc.ScreenPixelToRad(touchEv.Position)}");
        touches.Remove(touchEv.Index);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        
    }
}
