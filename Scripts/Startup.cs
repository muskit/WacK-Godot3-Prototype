/**
 * Startup.cs
 * Initial loading.
 *
 * by muskit
 * July 8, 2022
 **/

using Godot;
using System;
using System.Collections.Generic;

namespace WacK
{
	public partial class Startup : Node
	{
		private DirAccess songDir;
		public override void _Ready()
		{
			// TODO: enable loading screen
			songDir = DirAccess.Open($"{Misc.userDirectory}/songs");
			if (songDir == null)
			{
				var userDir = DirAccess.Open(Misc.userDirectory);
				var err = userDir.MakeDir($"songs");
				if (err != Error.Ok)
				{
					Misc.DebugPrintln("Failed to open songs folder!");
					return;
				}
			}
			
			// create note
			var note = "Place song folders here. Nested folders supported for organization.";
			using FileAccess f = FileAccess.Open(songDir.GetCurrentDir() + "/note.txt", FileAccess.ModeFlags.Write);
			Misc.DebugPrintln(f.GetPathAbsolute());
			f.StoreString(note);

			ScanSongs();

			GetTree().ChangeSceneToFile("res://Scenes/Menus/SongSelection.tscn");
		}

		private void ScanSongs()
		{
			Misc.songList = new List<Song>();

			songDir.ListDirBegin();

			var curObj = songDir.GetNext();
			while (curObj != String.Empty)
			{
				if (songDir.DirExists(curObj))
				{
					var iniPath = $"{curObj}/song.ini";
					if (songDir.FileExists(iniPath))
					{
						Misc.songList.Add(new Song($"{songDir.GetCurrentDir()}/{curObj}"));
					}
					else if (songDir.FileExists($"{curObj}/*.mer"))
					{
						GD.Print("Found song folder without .ini");
					}

				}
				curObj = songDir.GetNext();
			}
			// Misc.DebugPrintln("Scanned songs:");
			// foreach (var song in Misc.songList)
			// {
			//     Misc.DebugPrintln(song);
			// }
		}
	}
}
