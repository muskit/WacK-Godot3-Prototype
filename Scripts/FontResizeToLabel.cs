using Godot;
using System;
using System.Diagnostics;

public class FontResizeToLabel : Label
{
    public override void _Ready()
    {
        Connect("resized", this, nameof(OnResize));
    }

    private async void OnResize()
    {
        await ToSignal(GetTree(), "idle_frame"); // avoid hanging game temporarily

        var parentName = GetParent().GetParent().GetParent().Name;
        var font = GetFont("font") as DynamicFont;
        font.Size = Mathf.FloorToInt(this.RectSize.y);

        // var swTotal = new Stopwatch();
        // var swStep = new Stopwatch();
        // swTotal.Start();
        // GD.Print($"{parentName}: {this.Name} resizing to {this.RectSize}!");

        // FIXME: when Label JUST spawned, GetStringSize appears to hang game for some time. used await at beginning to avoid.
        while (font.GetStringSize(this.Text).y > this.RectSize.y || font.GetStringSize(this.Text).x > this.RectSize.x)
        {
            font.Size -= 1;

            // swStep.Stop();
            // GD.Print($"{font.Size}: {font.GetStringSize("0").y} > {this.RectSize.y} || {font.GetStringSize(this.Text).x} > {this.RectSize.x}");
            // GD.Print($"Total: {swTotal.ElapsedMilliseconds}ms");
            // GD.Print($"Step: {swStep.ElapsedMilliseconds}ms");
            // swStep.Restart();
        }

        // swTotal.Stop();
        // GD.Print($"{parentName}: {this.Name} completed resizing after {swTotal.ElapsedMilliseconds}ms");
    }
}
