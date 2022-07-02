/**
 * Background.cs
 * Set various properties of the drawn background.
 *
 * by muskit
 * July 1, 2022
 **/

using Godot;
using System.Collections.Generic;

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
            bgMaterial.DistanceFadeMaxDistance = Misc.cameraOffset;
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

    public void AddSegment(int segment)
    {
        segments[segment].Visible = true;
    }

    public void RemoveSegment(int segment)
    {
        segments[segment].Visible = false;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        
    }
}
