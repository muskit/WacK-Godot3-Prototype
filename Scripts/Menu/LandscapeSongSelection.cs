/**
 * LandscapeSongSelection.cs
 * The song select screen for landscape orientation.
 *
 * by muskit
 * July 27, 2022
 **/

using Godot;
using System;

namespace WacK
{
    public class LandscapeSongSelection : Control
    {
        [Export]
        private NodePath npSongList;
        [Export]
        private NodePath npListScroll;

        private Control songList;
        private ScrollContainer listScroll;

        public static DifficultyLevel curDifficulty = DifficultyLevel.Hard;

        private static PackedScene songItem = GD.Load<PackedScene>("res://Things/2D/Menu/SongSelection/LandscapeSongListItem.tscn");

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            songList = GetNode<Control>(npSongList);
            listScroll = GetNode<ScrollContainer>(npListScroll);
            listScroll.GetHScrollbar().Visible = false;
            ResetList();
        }

        public void ResetList()
        {
            // foreach (Node songItem in songList.GetChildren())
            // {
            //     songItem.QueueFree();
            // }

            foreach (Song song in Misc.songList)
            {
                var sItem = songItem.Instance<LandscapeSongListItem>();
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
