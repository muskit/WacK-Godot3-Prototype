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
    private Node2D scroll;
    private static Color color = new Color(255f/255, 236f/255, 26f/255);

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        textureSize = GetViewportRect().Size;
        minuteSize = textureSize.x / 60;
        scrollScale = textureSize.y / Misc.noteDrawDistance;
        scroll = GetChild<Node2D>(0).GetChild<Node2D>(0);
    }

    public async void CreateLongNote(Note holdStart, Note holdEnd)
    {
        while (scroll == null)
            await ToSignal(GetTree(), "idle_frame");
        
        var length = scrollScale * (holdEnd.Translation.z - holdStart.Translation.z);
        // GD.Print($"Creating long note {holdStart.noteIndex} that's {holdEnd.Translation.z - holdStart.Translation.z} long.");
        var holdNote = new Node2D();
        scroll.AddChild(holdNote);
        holdNote.Position = new Vector2(0, -holdStart.Translation.z * scrollScale);

        for (int i = 2; i < holdNote.GetChildCount(); ++i)
        {
            // TODO: Create segments
        }
        // REMOVE when done

        var verts = new Vector2[4];
        verts[0] = new Vector2(textureSize.x - holdStart.pos * minuteSize, 0);
        verts[1] = new Vector2(verts[0].x - holdStart.size * minuteSize, 0);
        verts[2] = new Vector2(textureSize.x - holdEnd.pos * minuteSize - holdEnd.size * minuteSize, -length);
        verts[3] = new Vector2(textureSize.x - holdEnd.pos * minuteSize, -length);
        var segment = new Polygon2D();
        segment.Polygon = verts;
        segment.Color = color;
        holdNote.AddChild(segment);
    }

    public void Scroll(float offset)
    {
        scroll.Translate(new Vector2(0, offset * scrollScale));
    }
    public void SetPosition(float zPos)
    {
        scroll.Position = new Vector2(0, zPos * scrollScale + holdCalibrationOffset);
    }
}
