/**
 * AccuracyDisplay.cs
 * Display accuracy of processed notes.
 *
 * by muskit
 * July 25, 2022
 */

using Godot;
using System;

namespace WacK
{
    public partial class AccuracyDisplay : Control
    {
        private Label lblAcc;
        private Label lblOffset;
        private Tween tween;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            var gEvents = GetNode<GEvents>("/root/GEvents");
            gEvents.Connect(nameof(GEvents.NoteHitEventHandler),new Callable(this,nameof(OnNoteInteract)));
            gEvents.Connect(nameof(GEvents.NoteMissEventHandler),new Callable(this,nameof(OnNoteInteract)));

            lblAcc = FindChild("LblAccuracy") as Label;
            lblOffset = FindChild("LblOffset") as Label;
            lblAcc.Modulate = new Color();
            lblOffset.Modulate = new Color();
            tween = CreateTween();
        }

        // 2/60 frame fade in (.033s)
        // 5/60 frame delay (.083s)
        // 11/60 frame fade out (.183s)
        private void OnNoteInteract(Note n)
        {
            tween.Kill();
            tween = CreateTween();
            
            lblAcc.Text = n.curAccuracy.ToString();
            lblAcc.Modulate = new Color(1, 1, 1);
            tween.TweenProperty(lblAcc, new NodePath("modulate:a"), 0f, 0.18f).SetDelay(0.1f);
            if (n.curAccuracy != Accuracy.Miss && n.curAccuracy != Accuracy.Marvelous)
            {
                lblOffset.Text = n.isEarly ? "Fast" : "Slow";
                lblOffset.Modulate = new Color(1, 1, 1);
                tween.TweenProperty(lblOffset, new NodePath("modulate:a"), 0f, 0.18f).SetDelay(0.1f);
            }
            tween.Play();
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
            
        }
    }
}
