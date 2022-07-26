/**
 * ScoreInfo.cs
 * Show score and combo info in play.
 *
 * by muskit
 * July 25, 2022
 **/
using Godot;
using System;

public class ScoreInfo : Control
{
    private Label lblScore;
    private Label lblCombo;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        lblScore = FindNode("LblScore") as Label;
        lblCombo = FindNode("LblCombo") as Label;

        var scoreKeeper = GetNode<ScoreKeeper>("/root/ScoreKeeper");
        scoreKeeper.Connect(nameof(ScoreKeeper.ScoreUpdated), this, nameof(SetScore));
        scoreKeeper.Connect(nameof(ScoreKeeper.ComboUpdated), this, nameof(SetCombo));
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
//  public override void _Process(float delta)
//  {
//      
//  }
}
