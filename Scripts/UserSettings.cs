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
    public partial class UserSettings: Node
    {
        public const float SCROLL_MULT = 3f;
        public static float playSpeedMultiplier = 4f;
    }
}
