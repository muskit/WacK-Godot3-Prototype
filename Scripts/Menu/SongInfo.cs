using Godot;
using System;

public class SongInfo : Panel
{
    [Export]
    private NodePath npSongTitle;
    [Export]
    private NodePath npSongArtist;
    [Export]
    private NodePath npTempo;
    [Export]
    private NodePath npCharter;
    [Export]
    private NodePath npDifficulties;
    [Export]
    private NodePath npSongList;

    private Label songTitle;
    private Label songArtist;
    private Label tempo;
    private Label charter;
    private Node difficulties;

    private PackedScene difficulty = GD.Load<PackedScene>("res://Things/Menu/Difficulty.tscn");

    public override void _Ready()
    {
        songTitle = GetNode<Label>(npSongTitle);
        songArtist = GetNode<Label>(npSongArtist);
        tempo = GetNode<Label>(npTempo);
        charter = GetNode<Label>(npCharter);
        difficulties = GetNode(npDifficulties);

        GetNode(npSongList).Connect("item_selected", this, nameof(OnSongSelected));
    }

    private void OnSongSelected(int idx)
    {
        SetSong(Misc.songList[idx]);
    }

    public void SetSong(Song song)
    {
        songTitle.Text = song.name;
        songArtist.Text = song.artist;
        tempo.Text = song.tempo.ToString();

        foreach (Node elem in difficulties.GetChildren())
        {
            elem.QueueFree();
        }
        for(int i = 0; i < song.difficulty.Length; ++i)
        {
            if (song.difficulty[i] != -1)
            {
                var diff = difficulty.Instance<Difficulty>();
                difficulties.AddChild(diff);
                diff.Set(song.difficulty[i], (DifficultyLevel)i);
            }
        }
    }
}
