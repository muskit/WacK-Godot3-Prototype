using Godot;
using System;

namespace WacK
{
    public class LandscapeSongListItem : Control
    {
        [Export]
        private NodePath npLblTitle;
        [Export]
        private NodePath npLblArtist;
        [Export]
        private NodePath npLblLevel;
        [Export]
        private NodePath npLblDifficulty;
        [Export]
        private NodePath npTxtrJacket;
        [Export]
        private NodePath npDiffColor;

        private Label lblTitle;
        private Label lblArtist;
        private Label lblLevel;
        private Label lblDifficulty;
        private TextureRect txtrJacket;
        private ColorRect diffColor;

        public Song song { get; private set; } = null;
        public DifficultyLevel curDiff { get; private set; }

        private bool isReady = false;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            lblTitle = GetNode<Label>(npLblTitle);
            lblArtist = GetNode<Label>(npLblArtist);
            lblLevel = GetNode<Label>(npLblLevel);
            lblDifficulty = GetNode<Label>(npLblDifficulty);
            txtrJacket = GetNode<TextureRect>(npTxtrJacket);
            diffColor = GetNode<ColorRect>(npDiffColor);

            SongSelection.instance.Connect(nameof(SongSelection.ChangeDifficulty), this, nameof(UpdateDifficulty));

            isReady = true;
        }

        public async void SetSong(Song s)
        {
            if (!isReady)
                await ToSignal(this, "ready");
            
            song = s;
            lblTitle.Text = s.name;
            lblArtist.Text = s.artist;
            if (song.jacketTexture != null)
                txtrJacket.Texture = song.jacketTexture;
            
            UpdateDifficulty();
        }

        public async void UpdateDifficulty(DifficultyLevel? dL = null)
        {
            if (!isReady)
                await ToSignal(this, "ready");

            if (song == null) return;

            curDiff = dL ?? SongSelection.currentDifficulty;
            while (song.difficulty[(int)curDiff] == -1)
            {
                curDiff--;
            }

            lblLevel.Text = Util.DiffNumToString(song.difficulty[(int) (curDiff)]);
            lblDifficulty.Text = curDiff.ToString().ToUpper();
            diffColor.Color = Difficulty.diffColor[(int)curDiff];
        }

        public override void _Process(float delta)
        {
        }
    }
    
}
