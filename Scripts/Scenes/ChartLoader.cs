/**
 * ChartLoader.cs
 * Code for a test scene that loads a .mer chart.
 *
 * by muskit
 * July 1, 2022
 **/

using Godot;
using System;

namespace WacK
{
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

			dropLevelSelect.Selected = 2;
		}

		public void SelDirBtnPressed()
		{
			FileDialog fd = new FileDialog();
			AddChild(fd);
			fd.Mode = FileDialog.ModeEnum.OpenDir;
			fd.Access = FileDialog.AccessEnum.Filesystem;
			fd.Connect("dir_selected", this, "SetTextPath");
			fd.SetPosition(this.RectSize / 2);
			fd.SetSize(new Vector2(400, 300));
			fd.CurrentDir = Misc.userDirectory;
			fd.Resizable = true;
			fd.Visible = true;
		}

		public void SetTextPath(string path)
		{
			this.textPath.Text = path;
		}

		public void PlayBtnPressed()
		{
			var misc = GetNode<Misc>("/root/Misc");
			misc.LoadSong(textPath.Text, (DifficultyLevel)dropLevelSelect.Selected);
		}
	}
}
