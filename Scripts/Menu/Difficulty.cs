using Godot;
using System;

namespace WacK
{
    public enum DifficultyLevel {
        Normal, Hard, Expert, Inferno
    }

    public partial class Difficulty : AspectRatioContainer
    {
        public static Color[] diffColor = {
            new Color(24f/256, 98f/256, 255f/256),
            new Color(253f/256, 183f/256, 9f/256),
            new Color(251f/256, 0f/56, 113f/256),
            new Color(48f/256, 0f/56, 51f/256)
        };

        public void Set(float diffPoint, DifficultyLevel level)
        {
            FindChild("Color Header").GetChild<Label>(0).Text = level.ToString();
            (FindChild("Color Header") as ColorRect).Color = diffColor[(int)level] * 0.7f;
            FindChild("Color Body").GetChild<Label>(0).Text = Util.DifficultyValueToString(diffPoint);
            (FindChild("Color Body") as ColorRect).Color = diffColor[(int)level];

        }
    }
}
