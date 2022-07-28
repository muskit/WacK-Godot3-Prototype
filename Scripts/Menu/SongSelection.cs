using Godot;
using System;

namespace WacK
{
    public class SongSelection : Control
    {
        [Signal]
        public delegate void ChangeDifficulty(DifficultyLevel diff);

        [Export]
        private NodePath npLandscapeScreen;
        [Export]
        private NodePath npPortraitScreen;

        private Control landscapeScreen;
        private Control portraitScreen;

        public static SongSelection instance { get; private set; }
        public static DifficultyLevel currentDifficulty = DifficultyLevel.Expert;


        public SongSelection()
        {
            instance = this;
        }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            landscapeScreen = GetNode<Control>(npLandscapeScreen);
            portraitScreen = GetNode<Control>(npPortraitScreen);
            OrientationDetect.instance.Connect(nameof(OrientationDetect.OrientationChange), this, nameof(OnOrientationChange));
        }

        private void OnOrientationChange(ScreenOrientation or)
        {
            switch (or)
            {
                case ScreenOrientation.Landscape:
                    landscapeScreen.Visible = true;
                    portraitScreen.Visible = false;
                    break;
                case ScreenOrientation.Portrait:
                    landscapeScreen.Visible = false;
                    portraitScreen.Visible = true;
                    break;
            }
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(float delta)
        {
            
        }
    }
}
