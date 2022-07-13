using Godot;
using System;

public class Play : Node
{
    [Export]
    private NodePath npAudioPlayer;
    [Export]
    private NodePath npPauseText;
    
    private GEvents gEvents;
    private Label pauseText;
    private float pauseTime;
    
    public override void _Ready()
    {
        Misc.songPlayer = GetNode<AudioStreamPlayer>(npAudioPlayer);
        Misc.songPlayer.Stream = Misc.currentAudio;

        pauseText = GetNode<Label>(npPauseText);

        gEvents = GetNode<GEvents>("/root/GEvents");
        gEvents.Connect(nameof(GEvents.on_pause), this, nameof(OnPauseEv));
        gEvents.Connect(nameof(GEvents.on_resume), this, nameof(OnUnpauseEv));

        gEvents.SetPause(true);
    }

    private async void CountdownStart()
    {
        // countdown start
        var t = new Timer();
        AddChild(t);
        t.WaitTime = 1;
        t.OneShot = true;
        t.Start();
        
        Misc.debugStr = "5";
        await ToSignal(t, "timeout");
        Misc.debugStr = "4";
        t.Start();
        await ToSignal(t, "timeout");
        Misc.debugStr = "3";
        t.Start();
        await ToSignal(t, "timeout");
        Misc.debugStr = "2";
        t.Start();
        await ToSignal(t, "timeout");
        Misc.debugStr = "1";
        t.Start();
        await ToSignal(t, "timeout");
        gEvents.SetPause(false);
        Misc.debugStr = "";

        t.QueueFree();
    }

    private void OnPauseEv()
    {
        pauseTime = Misc.songPlayer.GetPlaybackPosition();
        Misc.songPlayer.Stop();
    }

    private void OnUnpauseEv()
    {
        Misc.songPlayer.Play(pauseTime);
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("pause"))
        {
            gEvents.TogglePause();
        }
        if (Input.IsActionJustPressed("reset"))
        {
            Misc.songPlayer.Seek(0);
        }

        pauseText.Visible = Misc.paused;
    }
}
