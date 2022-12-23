/**
 * TextureCone.cs
 * For texture of the tunnel cone.
 *
 * by muskit
 * July 12, 2022
 **/

using Godot;
using System;

namespace WacK
{

    public partial class TextureCone : Node2D
    {
        private float holdCalibrationOffset = 0;
        private float minuteSize;
        public float scrollScale;
        private float startPos;

        private Vector2 textureSize;
        private Node2D scroll2D;
        private Camera2D cam2D;
        private static Color color = new Color(255f/255, 218f/255, 0f/255, 0.96f);

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            textureSize = GetViewportRect().Size;
            minuteSize = textureSize.x / 60;
            scrollScale = textureSize.y / Misc.noteDrawDistance;
            startPos = textureSize.y;

            scroll2D = FindChild("Scroll") as Node2D;
            cam2D = FindChild("Camera2D") as Camera2D;
            SetPosition(0);
        }

        // Each section between START-MID, MID-MID, or MID-END are
        // known as "segments."
        public Node2D CreateLongNote(Note holdStart, Note holdEnd)
        {
            // GD.Print($"Creating long note {holdStart.noteIndex} that's {holdEnd.Position.z - holdStart.Position.z} long.");
            var holdNote = new Node2D();
            scroll2D.AddChild(holdNote);
            holdNote.Position = new Vector2(0, -holdStart.Position.z * scrollScale);

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
            return holdNote;
        }

        private Polygon2D CreateSegment(Note origin, Note destination)
        {
            var length = scrollScale * (destination.GlobalTransform.origin.z - origin.GlobalTransform.origin.z);
            var verts = new Vector2[4];

            int originPos;
            int originSize;
            int destPos;
            int destSize;
            if (3 <= origin.size && origin.size <= 59)
            {
                originPos = (origin.pos + 1)%60;
                originSize = origin.size - 2;
            }
            else
            {
                originPos = origin.pos;
                originSize = origin.size;
            }

            if (3 <= destination.size && destination.size <= 59)
            {
                destPos = (destination.pos + 1)%60;
                destSize = destination.size - 2;
            }
            else
            {
                destPos = destination.pos;
                destSize = destination.size;
            }

            destPos = (int)Util.NearestMinute(originPos, destPos);
            verts[0] = new Vector2(textureSize.x - originPos * minuteSize, 0);
            verts[1] = new Vector2(verts[0].x - originSize * minuteSize, 0);
            verts[2] = new Vector2(textureSize.x - destPos * minuteSize - destSize * minuteSize, -length);
            verts[3] = new Vector2(textureSize.x - destPos * minuteSize, -length);
            var segment = new Polygon2D();
            segment.Polygon = verts;
            segment.Color = color;

            // draw overflow
            var originFinalPos = originPos + originSize;
            var destinationFinalPos = destPos + destSize;
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
            cam2D.Translate(new Vector2(0, -offset * scrollScale));
        }
        public void SetPosition(float zPos)
        {
            cam2D.Position = new Vector2(0, -(zPos * scrollScale + holdCalibrationOffset + startPos));
        }
    }
}
