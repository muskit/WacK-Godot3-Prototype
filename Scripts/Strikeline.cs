/**
 * Strikeline.cs
 * Handle notes passing through.
 *
 * by muskit
 * July 3, 2022
 **/

using Godot;
using System.Collections.Generic;

public class Strikeline : Spatial
{
    [Signal]
    delegate void Miss();
    [Signal]
    delegate void Hit(Accuracy acc);

    [Export]
    private bool hideNotesAfterMiss;
    private AudioStreamPlayer tickPlayer;
    private Area zoneMarvelous;
    private Area zoneGreat;
    private Area zoneGood;
    private Area zoneError;
    public Queue<Note> noteQueue { get; private set; } = new Queue<Note>();

    int i = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.Scale = new Vector3(1, 1, 10*PlaySettings.speedMultiplier);

        zoneMarvelous = GetChild<Area>(0);
        zoneGreat = GetChild<Area>(1);
        zoneGood = GetChild<Area>(2);
        zoneError = GetChild<Area>(3);

        zoneMarvelous.Connect("body_entered", this, nameof(OnMarvelousEnter));
        zoneGreat.Connect("body_entered", this, nameof(OnGreatEnter));
        zoneGood.Connect("body_entered", this, nameof(OnGoodEnter));
        zoneError.Connect("body_entered", this, nameof(OnErrorEnter));
        
        zoneMarvelous.Connect("body_exited", this, nameof(OnMarvelousExit));
        zoneGreat.Connect("body_exited", this, nameof(OnGreatExit));
        zoneGood.Connect("body_exited", this, nameof(OnGoodExit));
    }
    
    private void OnErrorEnter(Node obj) // EARLY MISS
    {
        var note = obj.GetParent() as Note;
        note.curAccuracy = Accuracy.Miss;
        noteQueue.Enqueue(note);
    }
    private void OnGoodEnter(Node obj)
    {
        var note = obj.GetParent() as Note;
        note.curAccuracy = Accuracy.Good;
    }
    private void OnGreatEnter(Node obj)
    {
        var note = obj.GetParent() as Note;
        note.curAccuracy = Accuracy.Great;
    }
    private void OnMarvelousEnter(Node obj)
    {
        var note = obj.GetParent() as Note;
        note.curAccuracy = Accuracy.Marvelous;
    }
    //
    private void OnMarvelousExit(Node obj)
    {
        var note = obj.GetParent() as Note;
        note.curAccuracy = Accuracy.Great;
    }
    private void OnGreatExit(Node obj)
    {
        var note = obj.GetParent() as Note;
        note.curAccuracy = Accuracy.Good;
    }
    private void OnGoodExit(Node obj) // LATE MISS
    {
        // we're done with the collision
        foreach (CollisionShape shape in obj.GetChildren())
        {
            shape.Disabled = true;
        }

        var note = obj.GetParent() as Note;

        // don't hide hold note if its Start is missed
        if (note.type == NoteType.HoldStart)
        {
            foreach (Node longSegment in note.GetChildren())
            {
                foreach (var step in longSegment.GetChildren())
                {
                    if(step is CSGPolygon n)
                    {
                        ((SpatialMaterial)n.Material).AlbedoColor = new Color(.4f, .4f, .4f);
                        break;
                    }
                }
            }

            if (noteQueue.Contains(note))
                noteQueue.Dequeue();
            EmitSignal(nameof(Miss));
            return;
        }

        if (note.type == NoteType.HoldEnd)
        {
            // TODO: handle end of hold note
            if (noteQueue.Contains(note))
                noteQueue.Dequeue();
            // note.GetParent().QueueFree();
            note.GetParent<Spatial>().Visible = !hideNotesAfterMiss;
            return;
        }
        
        // all other notes
        if (noteQueue.Contains(note))
            noteQueue.Dequeue();
        // note.QueueFree();
        note.Visible = !hideNotesAfterMiss;
        EmitSignal(nameof(Miss));
    }
}
