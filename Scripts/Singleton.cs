using Godot;
using System;

public class Singleton : Node
{
    [Signal]
    public delegate void on_pause();
    [Signal]
    public delegate void on_resume();
}
