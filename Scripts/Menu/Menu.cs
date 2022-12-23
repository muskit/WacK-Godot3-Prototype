using Godot;
using System;

namespace WacK
{
	public partial class Menu : CanvasLayer
	{
		[Export]
		private NodePath npListSongs;
		[Export]
		private NodePath npPlayButton;

		private ItemList listSongs;
		private Button playButton;

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			listSongs = GetNode<ItemList>(npListSongs);
			playButton = GetNode<Button>(npPlayButton);

			playButton.Connect("pressed",new Callable(this,nameof(OnPlayPressed)));

			foreach (var song in Misc.songList)
			{
				listSongs.AddItem($"{song.artist} - {song.name}");
			}
		}

		private void OnPlayPressed()
		{
			var misc = GetNode<Misc>("/root/Misc");
			misc.LoadSong(Misc.songList[listSongs.GetSelectedItems()[0]].directory.GetCurrentDir());
		}

		public override void _Process(double delta)
		{
			playButton.Disabled = !listSongs.IsAnythingSelected();
		}
	}
}
