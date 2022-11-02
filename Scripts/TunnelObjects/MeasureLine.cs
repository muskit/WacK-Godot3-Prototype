using Godot;

namespace WacK
{
    public class MeasureLine : Spatial
    {
        public override void _Process(float delta)
        {
            this.Scale = Util.NoteScale(this.GlobalTransform.origin.z, Misc.strikelineZPos);
        }
    }
}
