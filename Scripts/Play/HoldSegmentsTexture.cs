/**
 * HoldSegementsTexture.cs
 * Create Hold notes as 2D texture.
 *
 * by muskit
 * July 12, 2022
 **/

using Godot;
using System;

public class HoldSegmentsTexture : Node2D
{
    public float scrollScale = 1f;
    private float minuteSize;
    private float holdCalibrationOffset = 0;

    private Vector2 textureSize;
    private Node2D scroll2D;
    private static Color color = new Color(255f/255, 236f/255, 26f/255);

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        textureSize = GetViewportRect().Size;
        minuteSize = textureSize.x / 60;
        scrollScale = textureSize.y / Misc.noteDrawDistance;
        scroll2D = GetChild<Node2D>(0).GetChild<Node2D>(0);
    }

    // Each section between START-MID, MID-MID, or MID-END are
    // known as "segments."
    public async void CreateLongNote(Note holdStart, Note holdEnd)
    {
        while (scroll2D == null)
            await ToSignal(GetTree(), "idle_frame");

        // GD.Print($"Creating long note {holdStart.noteIndex} that's {holdEnd.Translation.z - holdStart.Translation.z} long.");
        var holdNote = new Node2D();
        scroll2D.AddChild(holdNote);
        holdNote.Position = new Vector2(0, -holdStart.Translation.z * scrollScale);

        if (holdStart.GetChildCount() > 2)
        {
            // START-MID, MID-MID
            Note lastHold = holdStart;
            float segmentPos = 0;
            for (int i = 2; i < holdStart.GetChildCount(); ++i)
            {
                var curNote = holdStart.GetChild<Note>(i);
                var curLength = scrollScale * (curNote.GlobalTransform.origin.z - lastHold.GlobalTransform.origin.z);
                var segment = CreateSegment(lastHold, curNote);
                holdNote.AddChild(segment);
                segment.Position = new Vector2(0, segmentPos);

                segmentPos -= curLength;
                lastHold = curNote;
            }
            // MID-END
            var finalSegment = CreateSegment(lastHold, holdEnd);
            holdNote.AddChild(finalSegment);
            finalSegment.Position = new Vector2(0, segmentPos);
        }
        else // START-END
        {
            var segment = CreateSegment(holdStart, holdEnd);
            holdNote.AddChild(segment);
        }
    }

    private Polygon2D CreateSegment(Note origin, Note destination)
    {
        var length = scrollScale * (destination.GlobalTransform.origin.z - origin.GlobalTransform.origin.z);
        var verts = new Vector2[4];
        var destPos = Misc.NearestSegment(origin.pos, destination.pos);
        verts[0] = new Vector2(textureSize.x - origin.pos * minuteSize, 0);
        verts[1] = new Vector2(verts[0].x - origin.size * minuteSize, 0);
        verts[2] = new Vector2(textureSize.x - destPos * minuteSize - destination.size * minuteSize, -length);
        verts[3] = new Vector2(textureSize.x - destPos * minuteSize, -length);
        var segment = new Polygon2D();
        segment.Polygon = verts;
        segment.Color = color;

        var originFinalPos = origin.pos + origin.size;
        var destinationFinalPos = destPos + destination.size;
        if (originFinalPos > 60 || destinationFinalPos > 60)
        {
            var subSegment = new Polygon2D();
            subSegment.Polygon = verts;
            subSegment.Color = color;
            subSegment.Translate(new Vector2(60 * minuteSize, 0));
            segment.AddChild(subSegment);
        }
        if (originFinalPos < 60 || destinationFinalPos < 60)
        {
            var subSegment = new Polygon2D();
            subSegment.Polygon = verts;
            subSegment.Color = color;
            subSegment.Translate(new Vector2(-60 * minuteSize, 0));
            segment.AddChild(subSegment);
        }

        return segment;
    }

    public void Scroll(float offset)
    {
        scroll2D.Translate(new Vector2(0, offset * scrollScale));
    }
    public void SetPosition(float zPos)
    {
        scroll2D.Position = new Vector2(0, zPos * scrollScale + holdCalibrationOffset);
    }
}