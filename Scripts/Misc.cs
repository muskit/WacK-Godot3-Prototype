/**
 * Misc.cs
 * Global values that are changed throughout the application's lifetime.
 *
 * by muskit
 * July 1, 2022
 **/
using Godot;
using System;

public static class Misc
{
    public static float cameraOffset = 0;
    public static string currentMer = "";
    public static AudioStreamMP3 currentAudio;
    public static AudioStreamPlayer songPlayer;

    public static float Seg2Rad(float seg)
    {
        return Mathf.Deg2Rad(6f * seg);
    }
}
