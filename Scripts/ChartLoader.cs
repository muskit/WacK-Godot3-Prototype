/**
 * ChartLoader.cs
 * Code for a test scene that loads a .mer chart.
 *
 * by muskit
 * July 1, 2022
 **/

using Godot;
using System;

public class ChartLoader : Control
{
    [Export]
    private NodePath npPath;
    [Export]
    private NodePath npLevelSelect;

    private LineEdit textPath;
    private OptionButton dropLevelSelect;

    public override void _Ready()
    {
        textPath = GetNode<LineEdit>(npPath);
        dropLevelSelect = GetNode<OptionButton>(npLevelSelect);

        dropLevelSelect.AddItem("NORMAL");
        dropLevelSelect.AddItem("HARD");
        dropLevelSelect.AddItem("EXPERT");
        dropLevelSelect.AddItem("INFERNO");
    }

    public void SelDirBtnPressed()
    {
        FileDialog fd = new FileDialog();
        AddChild(fd);
        fd.Mode = FileDialog.ModeEnum.OpenDir;
        fd.Access = FileDialog.AccessEnum.Filesystem;
        fd.Connect("dir_selected", this, "SetTextPath");
        fd.SetPosition(this.RectSize / 2);
        fd.Resizable = true;
        fd.Visible = true;
    }

    public void SetTextPath(string path)
    {
        this.textPath.Text = path;
    }

    public void PlayBtnPressed()
    {
        string chartPath = textPath.Text + $"/{dropLevelSelect.Selected}.mer";
        string audioPath = textPath.Text + "/music.mp3";

        var chart = new File();
        var audio = new File();
        Error errChart = chart.Open(chartPath, File.ModeFlags.Read);
        Error errAudio = audio.Open(audioPath, File.ModeFlags.Read);

        if (errChart != Error.Ok)
        {
            GD.PrintErr($"Trouble loading {chartPath}!\n{errChart}");
            return;
        }
        if (errAudio != Error.Ok)
        {
            GD.PrintErr($"Trouble loading {audioPath}!\n{errAudio}");
            return;
        }
        Misc.currentMer = chart.GetAsText();
        Misc.currentAudio = new AudioStreamMP3();
        Misc.currentAudio.Data = audio.GetBuffer((long)audio.GetLen());
        GetTree().ChangeScene("res://Scenes/Play.tscn");
    }
}
