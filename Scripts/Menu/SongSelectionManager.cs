using Godot;
using System;

namespace WacK
{
    public partial class SongSelectionManager : Control
    {
        [Signal]
        public delegate void ChangeDifficultyEventHandler(DifficultyLevel diff);

        [Export]
        private NodePath npLandscapeScreen;
        [Export]
        private NodePath npPortraitScreen;

        [Export]
        private NodePath npStartButton;
        [Export]
        private NodePath npDifficultyIncButton;
        [Export]
        private NodePath npDifficultyDecButton;
        [Export]
        private NodePath npDifficultyLabel;
        [Export]
        private NodePath npDifficultyLabelBackgroundTexture;
        

        private SongSelection landscapeScreen;
        private SongSelection portraitScreen;

        private Button startButton;
        private Button difficultyIncButton;
        private Button difficultyDecButton;
        private Label difficultyLabel;
        private TextureRect diffLabelBG;

        public static SongSelectionManager instance { get; private set; }
        public static DifficultyLevel currentDifficulty = DifficultyLevel.Expert;
        private static Song _currentSong = null;
        public static Song CurrentSong {
            get
            {
                return _currentSong;
            }
            set
            {
                _currentSong = value;
                instance?.landscapeScreen.songScrollContainer.GoToSong(_currentSong);
                instance?.portraitScreen.songScrollContainer.GoToSong(_currentSong);
            }
        }


        public SongSelectionManager()
        {
            instance = this;
        }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            landscapeScreen = GetNode<SongSelection>(npLandscapeScreen);
            portraitScreen = GetNode<SongSelection>(npPortraitScreen);
            landscapeScreen.songScrollContainer.Connect(nameof(SongScrollContainer.SongSelectedEventHandler),new Callable(this,nameof(OnContainerSongSelected)));
            portraitScreen.songScrollContainer.Connect(nameof(SongScrollContainer.SongSelectedEventHandler),new Callable(this,nameof(OnContainerSongSelected)));
            
            startButton = GetNode<Button>(npStartButton);
            difficultyIncButton = GetNode<Button>(npDifficultyIncButton);
            difficultyDecButton = GetNode<Button>(npDifficultyDecButton);
            difficultyLabel = GetNode<Label>(npDifficultyLabel);
            diffLabelBG = GetNode<TextureRect>(npDifficultyLabelBackgroundTexture);
            startButton.Connect("pressed",new Callable(this,nameof(OnStartPressed)));
            difficultyIncButton.Connect("pressed",new Callable(this,nameof(OnDifficultyIncPressed)));
            difficultyDecButton.Connect("pressed",new Callable(this,nameof(OnDifficultyDecPressed)));
            
            OrientationDetect.instance.Connect(nameof(OrientationDetect.OrientationChangeEventHandler),new Callable(this,nameof(OnOrientationChange)));

            if (Misc.songList.Count > 0 && CurrentSong == null)
            {
                CurrentSong = Misc.songList[0];
                startButton.Disabled = false;
            }

            UpdateDifficulty();
        }

        private void OnContainerSongSelected(Song song)
        {
            if (song != null && song != _currentSong)
            {
                _currentSong = song;
            }
            UpdateDifficulty();
        }

        private void OnStartPressed()
        {
            var m = GetNode<Misc>("/root/Misc");
            m.LoadSong(CurrentSong.directory.GetCurrentDir(), currentDifficulty);
        }

        private void OnDifficultyIncPressed()
        {
            if (currentDifficulty > CurrentSong.MaxDifficulty || currentDifficulty < CurrentSong.MinDifficulty)
                currentDifficulty = CurrentSong.MaxDifficulty;

            if (currentDifficulty < CurrentSong.MaxDifficulty)
                ++currentDifficulty;
            UpdateDifficulty();
        }

        private void OnDifficultyDecPressed()
        {
            if (currentDifficulty > CurrentSong.MaxDifficulty || currentDifficulty < CurrentSong.MinDifficulty)
                currentDifficulty = CurrentSong.MaxDifficulty;

            if (currentDifficulty > CurrentSong.MinDifficulty)
                --currentDifficulty;
            UpdateDifficulty();
        }

        private void UpdateDifficulty()
        {
            difficultyLabel.Text = currentDifficulty.ToString().ToUpper();

            var g = new Gradient();
            g.SetColor(0, Difficulty.diffColor[(int)currentDifficulty]);
            g.SetColor(1, new Color(0, 0, 0, 0));

            var g2D = new GradientTexture2D();
            g2D.Gradient = g;
            g2D.FillFrom = Vector2.Down;
            g2D.FillTo = Vector2.Zero;
            diffLabelBG.Texture = g2D;

            difficultyDecButton.Visible = currentDifficulty > CurrentSong?.MinDifficulty;
            difficultyIncButton.Visible = currentDifficulty < CurrentSong?.MaxDifficulty;

            EmitSignal(SignalName.ChangeDifficulty, (int)currentDifficulty);
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

        public override void _Process(double delta)
        {
            startButton.Disabled = _currentSong == null;
            if (IsQueuedForDeletion())
            {
                instance = null;
                return;
            }
        }
    }
}
