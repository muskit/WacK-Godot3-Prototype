using Godot;
using System;

public class GEvents : Node
{
    [Signal]
    public delegate void on_pause();
    [Signal]
    public delegate void on_resume();
}
