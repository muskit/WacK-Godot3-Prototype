using Godot;
using System;

public class Menu : Control
{
	Directory songDir;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		songDir = new Directory();
		if (songDir.Open("user://songs") != Error.Ok)
		{
			songDir.MakeDir("user://songs");
			if (songDir.Open("user://songs") != Error.Ok)
			{
				GD.PrintErr("Failed to open songs folder!");
				return;
			}
		}

		File f = new File();
		f.Open(songDir.GetCurrentDir() + "/note.txt", File.ModeFlags.Write);
		f.StoreString("Place song folders here. Nested folders for playlists supported.\nHello from Xcode!");
		f.Close();
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
