using Godot;
using System;

namespace WacK
{
    public partial class PortraitSongListItem : SongListItem
    {
        [Export]
        private NodePath npLblTitle;
        [Export]
        private NodePath npLblArtist;
        [Export]
        private NodePath npLblDifficulty;
        [Export]
        private NodePath npTxtrJacket;
        [Export]
        private NodePath npTxtrGradient;

        private Label lblTitle;
        private Label lblArtist;
        private Label lblDiffNum;
        private TextureRect txtrJacket;
        private TextureRect txtrGradient;

        public DifficultyLevel curDiff { get; private set; }

        private bool isReady = false;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            lblTitle = GetNode<Label>(npLblTitle);
            lblArtist = GetNode<Label>(npLblArtist);
            lblDiffNum = GetNode<Label>(npLblDifficulty);
            txtrJacket = GetNode<TextureRect>(npTxtrJacket);
            txtrGradient = GetNode<TextureRect>(npTxtrGradient);

            SongSelectionManager.instance.Connect(nameof(SongSelectionManager.ChangeDifficultyEventHandler),new Callable(this,nameof(UpdateDifficulty)));

            isReady = true;
        }

        public override async void SetSong(Song s)
        {
            if (!isReady)
                await ToSignal(this, "ready");
            
            song = s;
            lblTitle.Text = s.name;
            lblArtist.Text = s.artist;
            if (song.jacketTexture != null)
                txtrJacket.Texture2D = song.jacketTexture;
            
            UpdateDifficulty();
        }

        public void UpdateDifficulty(DifficultyLevel? dL = null)
        {
            if (song == null) return;

            curDiff = dL ?? SongSelectionManager.currentDifficulty;
            while (song.difficulty[(int)curDiff] == -1)
            {
                curDiff--;
            }

            lblDiffNum.Text = Util.DifficultyValueToString(song.difficulty[(int) (curDiff)]);

            // TODO: update gradient based on high score
            Gradient g = new Gradient();
            g.SetColor(0, Difficulty.diffColor[(int)curDiff]);
            g.SetColor(1, new Color(0, 0, 0));
            GradientTexture2D gt = new GradientTexture2D();
            gt.Width = 256;
            gt.Gradient = g;
            txtrGradient.Texture2D = gt;
        }
    }
}
