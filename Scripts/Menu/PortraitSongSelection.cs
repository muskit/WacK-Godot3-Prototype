/**
 * PortraitSongSelection.cs
 * The song select screen for portait orientation.
 *
 * by muskit
 * July 26, 2022
 **/

using Godot;
using System;

namespace WacK
{
    public class PortraitSongSelection : Control
    {
        [Export]
        private NodePath npSongList;
        [Export]
        private NodePath npSongInfo;

        private Control songList;
        private SongInfo songInfo;

        public static DifficultyLevel curDifficulty = DifficultyLevel.Expert;

        private static PackedScene songItem = GD.Load<PackedScene>("res://Things/2D/Menu/SongSelection/PortraitSongListItem.tscn");

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            songList = GetNode<Control>(npSongList);
            songInfo = GetNode<SongInfo>(npSongInfo);
            ResetList();
        }

        public void ResetList()
        {
            foreach (Node songItem in songList.GetChildren())
            {
                songItem.QueueFree();
            }

            foreach (Song song in Misc.songList)
            {
                var sItem = songItem.Instance<PortraitSongListItem>();
                songList.AddChild(sItem);
                sItem.SetSong(song);
            }
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(float delta)
        {
            
        }
    }
}
