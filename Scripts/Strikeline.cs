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

    private AudioStreamPlayer tickPlayer;
    private Area zoneMarvelous;
    private Area zoneGreat;
    private Area zoneGood;
    private Area zoneError;
    private Stack<Note> noteStack = new Stack<Note>();

    int i = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.Scale = new Vector3(1, 1, 10*PlaySettings.speedMultiplier);

        zoneMarvelous = GetChild<Area>(0);
        zoneGreat = GetChild<Area>(1);
        zoneGood = GetChild<Area>(2);
        zoneError = GetChild<Area>(3);

        zoneMarvelous.Connect("body_entered", this, "OnMarvelousEnter");
        zoneGreat.Connect("body_entered", this, "OnGreatEnter");
        zoneGood.Connect("body_entered", this, "OnGoodEnter");
        zoneError.Connect("body_entered", this, "OnErrorEnter");
        
        zoneMarvelous.Connect("body_exited", this, nameof(OnMarvelousExit));
        zoneGreat.Connect("body_exited", this, nameof(OnGreatExit));
        zoneGood.Connect("body_exited", this, nameof(OnGoodExit));
    }
    
    private void OnErrorEnter(Node obj) // EARLY MISS
    {
        var note = obj.GetParent() as Note;
        note.curAccuracy = Accuracy.Miss;
        noteStack.Push(note);
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
        noteStack.Pop();
        obj.GetParent().QueueFree();
        EmitSignal(nameof(Miss));
    }
}
