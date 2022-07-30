/**
 * SongSelection.cs
 * The contents of song select interactables.
 *
 * by muskit
 * July 27, 2022
 **/

using Godot;
using System;

namespace WacK
{
    public class SongSelection : Control
    {
        [Export]
        private bool isPortrait;
        [Export]
        private NodePath npSongScrollContainer;
        [Export]
        private NodePath npSongList;

        public SongScrollContainer songScrollContainer { get; private set; }
        private Control songList;

        public static DifficultyLevel curDifficulty = DifficultyLevel.Hard;

        private static PackedScene landscapeSongItem = GD.Load<PackedScene>("res://Things/2D/Menu/SongSelection/LandscapeSongListItem.tscn");
        private static PackedScene portraitSongItem = GD.Load<PackedScene>("res://Things/2D/Menu/SongSelection/PortraitSongListItem.tscn");

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            songScrollContainer = GetNode<SongScrollContainer>(npSongScrollContainer);
            songList = GetNode<Control>(npSongList);
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
                var sItem = isPortrait ? portraitSongItem.Instance<SongListItem>() : landscapeSongItem.Instance<SongListItem>();
                songList.AddChild(sItem);
                sItem.SetSong(song);
            }
        }

        public void SetSong(Song song)
        {
            songScrollContainer.GoToSong(song, true);
        }
    }
}
