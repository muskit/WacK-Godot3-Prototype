/**
 * ScoreInfo.cs
 * Show score and combo info in play.
 *
 * by muskit
 * July 25, 2022
 **/
using Godot;
using System;

namespace WacK
{
    public partial class ScoreInfo : Control
    {
        private Label lblScore;
        private Label lblCombo;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            lblScore = FindChild("LblScore") as Label;
            lblCombo = FindChild("LblCombo") as Label;

            var scoreKeeper = GetNode<ScoreKeeper>("/root/ScoreKeeper");
            scoreKeeper.Connect(nameof(ScoreKeeper.ScoreUpdatedEventHandler),new Callable(this,nameof(SetScore)));
            scoreKeeper.Connect(nameof(ScoreKeeper.ComboUpdatedEventHandler),new Callable(this,nameof(SetCombo)));
            SetScore(scoreKeeper.CurrentScore);
            SetCombo(scoreKeeper.curCombo);
        }

        public void SetScore(int score)
        {
            lblScore.Text = score.ToString("D7");
        }

        public void SetCombo(int combo)
        {
            lblCombo.Text = combo > 1 ? combo.ToString() : "";
        }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(double delta)
    //  {
    //      
    //  }
    }
}
