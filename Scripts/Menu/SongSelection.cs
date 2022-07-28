using Godot;
using System;

namespace WacK
{
    public class SongSelection : Control
    {
        [Signal]
        public delegate void ChangeDifficulty(DifficultyLevel diff);

        public static SongSelection instance { get; private set; }
        public static DifficultyLevel currentDifficulty = DifficultyLevel.Expert;


        public SongSelection()
        {
            instance = this;
        }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(float delta)
        {
            
        }
    }
}
