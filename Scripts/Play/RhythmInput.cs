/**
 * RhythmInput.cs
 * Handle play-related input.
 *
 * by muskit
 * July 5, 2022
 **/

using Godot;
using System.Collections.Generic;

public class RhythmInput : Control
{
    [Export]
    private NodePath npFeedbackCircle;
    private List<FeedbackSegment> feedbackCircle = new List<FeedbackSegment>();
    public static Dictionary<int, Vector2> touches = new Dictionary<int, Vector2>();
    public static Dictionary<int, int> touchedSegments = new Dictionary<int, int>();

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

    private void JustTouched(InputEventScreenTouch touchEv)
    {
        var touchedSeg = Misc.TouchPosToSegmentInt(touchEv.Position, RectSize);

        touches[touchEv.Index] = touchEv.Position;
        touchedSegments[touchEv.Index] = touchedSeg;
        feedbackCircle[touchedSeg].Fire(true);
        // GD.Print($"added {touchEv.Index}");
    }
    private void DragTouch(InputEventScreenDrag dragEv)
    {
        var touchedSeg = Misc.TouchPosToSegmentInt(dragEv.Position, RectSize);

        // TODO: allow slide-cheesing with threshold
        // if (touchedSegments[dragEv.Index] != touchedSeg)
        // {
        //     feedbackCircle[touchedSeg].Fire(true);
        // }

        touches[dragEv.Index] = dragEv.Position;
        touchedSegments[dragEv.Index] = touchedSeg;
    }
    private void Untouch(InputEventScreenTouch touchEv)
    {
        touches.Remove(touchEv.Index);
        touchedSegments.Remove(touchEv.Index);
        // GD.Print($"removed {touchEv.Index}");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        foreach (var touch in touchedSegments)
        {
            // GD.Print(touch.Key);
            feedbackCircle[touch.Value].Fire(false);
        }
    }
}
