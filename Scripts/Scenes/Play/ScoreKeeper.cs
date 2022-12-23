/**
 * ScoreKeeper.cs
 * Score system and tracker.
 *
 * by muskit
 * July 25, 2022
 */

using Godot;
using System;

namespace WacK
{
    public partial class ScoreKeeper : Node
    {
        [Signal]
        public delegate void ScoreUpdatedEventHandler(int score);
        [Signal]
        public delegate void ComboUpdatedEventHandler(int combo);
        private Chart curChart;
        private float pointsPerMarv;

        // MAX: 1,000,000
        public int CurrentScore
        {
            get
            {
                return (int) ((pointsPerMarv*marvHits) + (.70f*pointsPerMarv*greatHits) + (.50f*pointsPerMarv*goodHits));
            }
        }
        public int curCombo { get; private set; }
        public int marvHits { get; private set; }
        public int greatHits { get; private set; }
        public int goodHits { get; private set; }
        public int missedHits { get; private set; }
        public int earlyHits { get; private set; }
        public int lateHits { get; private set; }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Reset();
            var gEvents = GetNode<GEvents>("/root/GEvents");
            gEvents.Connect(nameof(GEvents.NoteHitEventHandler),new Callable(this,nameof(OnNoteSignal)));
            gEvents.Connect(nameof(GEvents.NoteMissEventHandler),new Callable(this,nameof(OnNoteSignal)));
        }

        private void OnNoteSignal(Note n)
        {
            switch (n.curAccuracy)
            {
                case Accuracy.Miss:
                    missedHits++;
                    break;
                case Accuracy.Good:
                    goodHits++;
                    break;
                case Accuracy.Great:
                    greatHits++;
                    break;
                case Accuracy.Marvelous:
                    marvHits++;
                    break;
            }
            if (n.curAccuracy != Accuracy.Marvelous && n.curAccuracy != Accuracy.Miss)
            {
                if (n.isEarly)
                    earlyHits++;
                else
                    lateHits++;
            }
            
            if (n.curAccuracy != Accuracy.Miss)
                curCombo++;
            else
                curCombo = 0;

            EmitSignal(nameof(ScoreUpdatedEventHandler), CurrentScore);
            EmitSignal(nameof(ComboUpdatedEventHandler), curCombo);
        }

        private void Reset()
        {
            curCombo = 0;
            marvHits = 0;
            greatHits = 0;
            goodHits = 0;
            missedHits = 0;
            earlyHits = 0;
            lateHits = 0;
            pointsPerMarv = 0;
            curChart = null;
        }

        public void NewChart(Chart chart)
        {
            Reset();
            curChart = chart;
            pointsPerMarv = 1000000f / chart.playableNoteCount;
        }
    }
}
