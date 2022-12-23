using Godot;

namespace WacK
{
    public partial class MeasureLine : Node3D
    {
        public override void _Process(double delta)
        {
            this.Scale = Util.NoteScale(this.GlobalTransform.origin.z, Misc.strikelineZPos);
        }
    }
}
