using Godot;
using System;

public enum DifficultyLevel {
    Normal, Hard, Expert, Inferno
}

public class Difficulty : AspectRatioContainer
{
    private static Color[] diffColor = {
        new Color(24f/256, 98f/256, 255f/256),
        new Color(253f/256, 183f/256, 9f/256),
        new Color(251f/256, 0f/56, 113f/256),
        new Color(48f/256, 0f/56, 51f/256)
    };

    public void Set(float diffPoint, DifficultyLevel level)
    {
        FindNode("Color Header").GetChild<Label>(0).Text = level.ToString();
        (FindNode("Color Header") as ColorRect).Color = diffColor[(int)level] * 0.7f;
        FindNode("Color Body").GetChild<Label>(0).Text = $"{Mathf.FloorToInt(diffPoint)}{(diffPoint > Mathf.Floor(diffPoint) ? "+" : "")}";
        (FindNode("Color Body") as ColorRect).Color = diffColor[(int)level];

    }
}
