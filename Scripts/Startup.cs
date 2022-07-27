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
	public class Startup : Node
	{
		private Directory songDir;
		public override void _Ready()
		{
			// TODO: enable loading screen

			songDir = new Directory();
			if (songDir.Open("user://songs") != Error.Ok)
			{
				songDir.MakeDir("user://songs");
				if (songDir.Open("user://songs") != Error.Ok)
				{
					Misc.DebugPrintln("Failed to open songs folder!");
					return;
				}
			}
			
			// create note
			var note = "Place song folders here. Nested folders supported for organization.";
			File f = new File();
			f.Open(songDir.GetCurrentDir() + "/note.txt", File.ModeFlags.Write);
			f.StoreString(note);
			f.Close();

			ScanSongs();

			GetTree().ChangeScene("res://Scenes/Menus/SongSelection.tscn");
		}

		private void ScanSongs()
		{
			Misc.songList = new List<Song>();

			songDir.ListDirBegin(true);

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
