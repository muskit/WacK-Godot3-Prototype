/**
 * OrientationDetect.cs
 * Emits signal when window is resized.
 *
 * by muskit
 * July 9, 2022
 **/

using Godot;

namespace WacK
{
    public enum ScreenOrientation
    {
        Landscape, Portrait
    }

    public class OrientationDetect : Node
    {
        [Signal]
        public delegate void OrientationChange(ScreenOrientation orientation);

        public static OrientationDetect instance { get; private set; }
        public static ScreenOrientation curOrientation { get; private set; }
        public static float scaleX { get; private set; }
        public static float scaleY { get; private set; }

        public static Vector2 minResolution = new Vector2(1024, 600);

        public OrientationDetect()
        {
            instance = this;
        }

        public async override void _Ready()
        {
            GetTree().Root.Connect("size_changed",  this, nameof(OnScreenResize));
            
            await ToSignal(GetTree().Root, "ready");
            OnScreenResize();
        }

        private void OnScreenResize()
        {
            var s = OS.WindowSize;

            if (s.x > s.y)
                curOrientation = ScreenOrientation.Landscape;
            else
                curOrientation = ScreenOrientation.Portrait;
            
            scaleX = s.x / minResolution.x;
            scaleY = s.y / minResolution.y;
            
            EmitSignal(nameof(OrientationChange), curOrientation);
        }
    }
}