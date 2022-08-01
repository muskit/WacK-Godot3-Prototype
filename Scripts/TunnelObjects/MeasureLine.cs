using Godot;

namespace WacK
{
    public class MeasureLine : Spatial
    {
        private string _str = "";
        private Label3D label;
        public string Text {
            get
            {
                return label?.Text ?? _str;
            }
            set
            {
                if (label == null)
                {
                    _str = value;
                }
                else
                    label.Text = value;
            }
        }

        public override void _Ready()
        {
            label = GetChild<Label3D>(1);
            label.Text = _str;
        }

        public override void _Process(float delta)
        {
            this.Scale = Util.NoteScale(this.GlobalTransform.origin.z, Misc.strikelineZPos);
        }
    }
}
