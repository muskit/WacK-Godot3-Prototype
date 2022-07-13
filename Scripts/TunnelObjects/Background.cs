/**
 * Background.cs
 * Set various properties of the drawn background.
 *
 * by muskit
 * July 1, 2022
 **/

using Godot;
using System.Collections.Generic;

public enum DrawDirection {
    CounterClockwise, Clockwise, Center
}
public class Background : Node
{
    private bool isReady = false;
    private float _drawLength;
    private List<Spatial> segments = new List<Spatial>();
    private SpatialMaterial bgMaterial;
    [Export]
    public float DrawLength
    {
        set
        {
            _drawLength = value;
            if (!isReady) return;

            bgMaterial.DistanceFadeMinDistance = Misc.cameraOffset + _drawLength;
            bgMaterial.DistanceFadeMaxDistance = 0;
            foreach (Node segment in segments)
            {
                segment.GetChild<Spatial>(1).Scale = new Vector3(1, _drawLength, 1);
            }
        }
        get { return _drawLength; }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        foreach (Spatial segment in GetChildren())
        {
            segments.Add(segment);
        }
        bgMaterial = (SpatialMaterial) segments[0].GetChild<CSGPolygon>(1).Material;

        isReady = true;
        DrawLength = DrawLength;
    }

    // draw in 6/60 frames (0.1s)
    public async void SetSegments(int pos, int size, bool state, DrawDirection direction)
    {
        // GD.Print($"{direction} = {state}. Even? {size % 2 == 0}");

        float timer = 0;
        float time = 0.1f;

        int centerSeg = pos + size/2;
        while (timer < 0.1f)
        {
            timer = Mathf.Clamp(timer + GetProcessDeltaTime(), 0, time);
            float timerRatio = timer / time;
            int steps = Mathf.CeilToInt((float)size*timerRatio);

            switch(direction)
            {
                case DrawDirection.CounterClockwise:
                    for (int i = 0; i < steps; ++i)
                    {
                        segments[(i + pos)%60].Visible = state;
                    }
                    break;
                case DrawDirection.Center: // add: center to edge. rem: edge to center.
                    for (int i = centerSeg; i < Misc.InterpInt(centerSeg, pos+size, timerRatio); ++i)
                    {
                        segments[i % 60].Visible = state;
                    }
                    for (int i = centerSeg; i >= Misc.InterpInt(centerSeg, pos, timerRatio); --i)
                    {
                        segments[i % 60].Visible = state;
                    }
                    break;
                case DrawDirection.Clockwise:
                    for (int i = 0; i < steps; ++i)
                    {
                        segments[(pos + size - i - 1)%60].Visible = state;
                    }
                    break;
            }
            await ToSignal(GetTree(), "idle_frame");
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        
    }
}
