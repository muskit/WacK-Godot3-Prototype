using Godot;

namespace WacK
{
	public abstract partial class SongListItem : Control
	{
		public Song song { get; protected set; } = null;

		public virtual void SetSong(Song s) {}
	}
}
