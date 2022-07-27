/**
 * UserSettings.cs
 * Configuration related to current play.
 *
 * by muskit
 * July 2, 2022
 **/

using Godot;
using System;

namespace WacK
{
    public class UserSettings: Node
    {
        public const float SCROLL_MULT = 3.5f;
        public static float playSpeedMultiplier = 4f;
    }
}
