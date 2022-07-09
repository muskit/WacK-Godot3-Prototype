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
    [Export]
    private NodePath npFeedbackCircle;
    private List<FeedbackSegment> feedbackCircle = new List<FeedbackSegment>();
    private Dictionary<int, Vector2> touches = new Dictionary<int, Vector2>();
    private Dictionary<int, int> touchedSegments = new Dictionary<int, int>();

    public override void _Ready()
    {
        var feedbackSegmentsNode = GetNode(npFeedbackCircle);
        foreach (FeedbackSegment seg in feedbackSegmentsNode.GetChildren())
        {
            seg.Visible = false;
            feedbackCircle.Add(seg);
        }
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
        GD.Print($"{touchEv.Position} / {OS.WindowSize}");
        var touchedSeg = Misc.ScreenPixelToSegmentInt(touchEv.Position);
        touches[touchEv.Index] = touchEv.Position;
        touchedSegments[touchEv.Index] = touchedSeg;
        feedbackCircle[touchedSeg].Fire();
    }
    private void DragTouch(InputEventScreenDrag dragEv)
    {
        touches[dragEv.Index] = dragEv.Position;
        touchedSegments[dragEv.Index] = Misc.ScreenPixelToSegmentInt(dragEv.Position);
    }
    private void Untouch(InputEventScreenTouch touchEv)
    {
        touches.Remove(touchEv.Index);
        touchedSegments.Remove(touchEv.Index);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        foreach (var touch in touchedSegments)
        {
            feedbackCircle[touch.Value].Fire(false);
        }
    }
}
