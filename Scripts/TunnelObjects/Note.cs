using Godot;
using System;

namespace WacK
{    
    public enum Accuracy
    {
        Miss, Good, Great, Marvelous
    }
    public class Note : Spatial
    {
        private bool isReady = false;

        [Export]
        private NodePath npNoteSprite;
        [Export]
        private NodePath npNoteProgressBar;

        [Export]
        public bool isEvent;
        [Export]
        public bool requireJustTouched;
        [Export]
        public NoteType type;
        [Export]
        public Node2D holdSegment = null;
        
        private Sprite3D noteSprite;
        private TextureProgress noteProgress;
        
        public Accuracy curAccuracy = Accuracy.Miss;
        public bool hasBeenProcessed = false;

        public bool isEarly = false;
        public bool noteSwiped = false;

        public int pos = 0;
        public int size = 1;
        public int noteIndex = -1;
        public float value = 0;

        private Color holdMissColor = new Color(0.5f, 0.5f, 0.5f, 0.96f);

        private GEvents gEvents;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            isReady = false;

            gEvents = GetNode<GEvents>("/root/GEvents");
            if (npNoteSprite != null)
                noteSprite = GetNode<Sprite3D>(npNoteSprite);
            if (npNoteProgressBar != null)
                noteProgress = GetNode<TextureProgress>(npNoteProgressBar);

            isReady = true;
        }

        public async void SetPosSize(int pos = 0, int size = 1)
        {
            if (!isReady)
                await ToSignal(this, "ready");

            this.pos = pos;
            this.size = size;
            
            if (noteSprite != null && noteProgress != null)
            {
                if (size >= 3 && size <= 59)
                {
                    noteSprite.Transform = noteSprite.Transform.Rotated(Vector3.Forward, Mathf.Deg2Rad(6f * (pos + 1)));
                    noteProgress.Value = size - 2;
                }
                else
                {
                    noteSprite.Transform = noteSprite.Transform.Rotated(Vector3.Forward, Mathf.Deg2Rad(6f * pos));
                    noteProgress.Value = size;
                }
            }
        }

        public void Miss(bool isEarly = false)
        {
            if (!hasBeenProcessed)
            {
                this.isEarly = isEarly;
                hasBeenProcessed = true;

                // HoldStart
                if (type == NoteType.HoldStart && holdSegment != null)
                {
                    foreach(Polygon2D p2D in holdSegment.GetChildren())
                    {
                        p2D.Color = new Color(0.5f, 0.5f, 0.5f, 0.96f);
                        foreach(Polygon2D p2DOffset in p2D.GetChildren())
                        {
                            p2DOffset.Color = new Color(0.5f, 0.5f, 0.5f, 0.96f);
                        }
                    }
                }

                FullDisable();
                gEvents.EmitSignal(nameof(GEvents.NoteMiss), this);
            }
        }

        public void Hit(Accuracy acc, bool isEarly = false)
        {
            if (!hasBeenProcessed)
            {
                if (acc == Accuracy.Miss) { Miss(isEarly); return; }

                this.isEarly = isEarly;
                hasBeenProcessed = true;
                
                curAccuracy = acc;
                FullDisable();
                gEvents.EmitSignal(nameof(GEvents.NoteHit), this);
            }
        }

        public void Hit(float hitDelta)
        {
            // early good
            if (-Playfield.TIMING_WINDOW_GOOD <= hitDelta && hitDelta < -Playfield.TIMING_WINDOW_GREAT)
            {
                Hit(Accuracy.Good, true);
            }
            // early great
            if (-Playfield.TIMING_WINDOW_GREAT <= hitDelta && hitDelta < -Playfield.TIMING_WINDOW_MARVELOUS)
            {
                Hit(Accuracy.Great, true);
            }

            // marvelous
            if (-Playfield.TIMING_WINDOW_MARVELOUS <= hitDelta && hitDelta <= Playfield.TIMING_WINDOW_MARVELOUS)
                Hit(Accuracy.Marvelous);

            // late great
            if (Playfield.TIMING_WINDOW_MARVELOUS < hitDelta && hitDelta < Playfield.TIMING_WINDOW_GREAT)
                Hit(Accuracy.Great);
            // late good
            if (Playfield.TIMING_WINDOW_GREAT <= hitDelta && hitDelta <= Playfield.TIMING_WINDOW_GOOD)
                Hit(Accuracy.Good);
        }

        private void FullDisable()
        {
            SetProcess(false);
            this.Visible = false;
        }

        public void Enable()
        {
            this.Visible = true;
            SetProcess(true);
        }

        public override void _Process(float delta)
        {
            this.Scale = Util.NoteScale(this.GlobalTransform.origin.z, Misc.strikelineZPos);
        }
    }
}