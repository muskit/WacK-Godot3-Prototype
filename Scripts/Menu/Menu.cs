using Godot;
using System;

public class Menu : CanvasLayer
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

		playButton.Connect("pressed", this, nameof(OnPlayPressed));

		foreach (var song in Misc.songList)
		{
			listSongs.AddItem($"{song.artist} - {song.name}");
		}
	}

	private void OnPlayPressed()
	{
		var misc = GetNode<Misc>("/root/Misc");
		misc.LoadSong(Misc.songList[listSongs.GetSelectedItems()[0]].directory.GetCurrentDir(), 2);
	}

	public override void _Process(float delta)
	{
		playButton.Disabled = !listSongs.IsAnythingSelected();
	}
}
