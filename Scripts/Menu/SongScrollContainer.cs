using Godot;
using System.Diagnostics;

namespace WacK
{    
    // TODO: automatically appropriately resize to fit child list
    public class SongScrollContainer : Container
    {
        [Signal]
        public delegate void SongSelected(Song song);

        [Export]
        private bool isPortrait;

        public bool isReady { get; private set; } = false;

        private Control songList;
        private int _songIndex = -1;
        private bool lockedSelection = false;

        private float uiCenterPoint;
        private float uiFirstSongScrollPoint
        {
            get
            {
                if (songList.GetChildCount() > 0)
                {
                    var s = songList.GetChild<Control>(0);
                    var pos = s.RectPosition;
                    var size = s.RectSize;
                    return uiCenterPoint - (isPortrait ? (pos.y + size.y/2) : (pos.x + size.x/2));
                }
                return 0;
            }
        }
        private float uiLastSongScrollPoint
        {
            get
            {
                if (songList.GetChildCount() > 0)
                {
                    var s = songList.GetChild<Control>(songList.GetChildCount() - 1);
                    var pos = s.RectPosition;
                    var size = s.RectSize;
                    return uiCenterPoint - (isPortrait ? (pos.y + size.y/2) : (pos.x + size.x/2));
                }
                return 0;
            }
        }

        private Tween tween;
        private Vector2 lastInput = Vector2.Zero;
        private Vector2 followingLastInput = Vector2.Zero;
        private bool isTouching = false;
        private const float FLICK_DECELERATION = 10000f;
        private float flickVelocity = 0f;
        private float FlickSpeed
        {
            get
            {
                return Mathf.Abs(flickVelocity);
            }
        }

        public int SongIndex
        {
            get
            {
                return _songIndex;
            }
            private set
            {
                _songIndex = value;
                EmitSignal(nameof(SongSelected), isPortrait ?
                        songList.GetChild<PortraitSongListItem>(_songIndex).song :
                        songList.GetChild<LandscapeSongListItem>(_songIndex).song);
            }    
        }

        // Return index of song object closest to uiCenterPoint based on position of songList.
        public int NearestSong
        {
            get
            {
                float curDelta = float.MaxValue;
                int curNearest = -1;
                
                // find song list item closest to centerpos
                foreach (Control song in songList.GetChildren())
                {
                    float songPos = isPortrait ?
                        (song.RectGlobalPosition.y + song.RectSize.y/2) :
                        (song.RectGlobalPosition.x + song.RectSize.x/2);

                    float delta = Mathf.Abs(songPos - uiCenterPoint);

                    if (curNearest == -1 || delta < curDelta)
                    {
                        curNearest = song.GetIndex();
                        curDelta = delta;
                    }
                }
                return curNearest;
            }
        }

        public override void _Ready()
        {
            isReady = false;

            // a container of SongListItems should be our only child
            songList = FindNode("SongList") as Control;
            tween = new Tween();
            AddChild(tween);

            Connect("resized", this, nameof(OnResize));
            OnResize();
    
            isReady = true;
        }

        public override void _GuiInput(InputEvent @event)
        {
            if (@event is InputEventScreenTouch evTouch)
            {
                if (evTouch.IsPressed()) // just touched
                {
                    isTouching = true;

                    tween.RemoveAll();
                    lockedSelection = false;
                    flickVelocity = 0;
                    lastInput = followingLastInput = evTouch.Position;
                }
                else // just released
                {
                    isTouching = false;

                    var delta = evTouch.Position - followingLastInput;
                    flickVelocity = Engine.GetFramesPerSecond() *
                        (float)(isPortrait ? delta.y : delta.x);

                    if (FlickSpeed == 0)
                        GoToSong(NearestSong);
                    
                    followingLastInput = Vector2.Zero;
                    lastInput = Vector2.Zero;
                }
            }
            else if (@event is InputEventScreenDrag evDrag)
            {
                var delta = evDrag.Position - lastInput;
                followingLastInput = lastInput;
                lastInput = evDrag.Position;

                var p = songList.RectPosition;
                if (isPortrait)
                    p.y += delta.y;
                else
                    p.x += delta.x;
                songList.RectPosition = p;
            }
        }

        public void OnResize()
        {
            uiCenterPoint = isPortrait ? RectSize.y/2 : RectSize.x/2;
            GoToSong(SongSelectionManager.CurrentSong, true);
        }

        public async void GoToSong(int idx, bool snapTo = false)
        {
            if (!isReady)
                await ToSignal(this, "ready");
            if (idx < 0 || idx >= songList.GetChildCount()) return;

            flickVelocity = 0;
            lockedSelection = true;

            SongIndex = idx;
            var song = songList.GetChild<Control>(idx);

            float songPos = isPortrait ?
                (song.RectPosition.y + song.RectSize.y/2) :
                (song.RectPosition.x + song.RectSize.x/2);

            var scrollPos = uiCenterPoint - songPos;

            var listPos = songList.RectPosition;

            // apply calculated values
            if (isPortrait)
            {
                listPos.y = scrollPos;
            }
            else
            {
                listPos.x = scrollPos;
            }

            tween.RemoveAll();
            if (snapTo)
            {
                songList.RectPosition = listPos;
            }
            else
            {
                tween.InterpolateProperty(songList, "rect_position", songList.RectPosition, listPos, 0.3f, Tween.TransitionType.Quart, Tween.EaseType.Out);
                tween.Start();
            }
        }

        public void GoToSong(Song song, bool snapTo = false)
        {
            foreach (SongListItem ch in songList.GetChildren())
            {
                if (ch.song == song)
                {
                    GoToSong(ch.GetIndex(), snapTo);
                }
            }
        }

        public override void _Process(float delta)
        {
            // Misc.DebugPrintln($"[{isPortrait}]\n{(isPortrait ? songList.RectPosition.y : songList.RectPosition.x)}\nMin: {uiFirstSongScrollPoint}\nMax: {uiLastSongScrollPoint}\n");

            flickVelocity = flickVelocity > 0 ?
                Mathf.Clamp(flickVelocity - FLICK_DECELERATION*delta, 0, flickVelocity) :
                Mathf.Clamp(flickVelocity + FLICK_DECELERATION*delta, flickVelocity, 0);

            if (!lockedSelection && !isTouching)
            {
                var r = songList.RectPosition;
                // out of bounds
                if (isPortrait)
                {
                    if (r.y < uiLastSongScrollPoint || r.y > uiFirstSongScrollPoint)
                    {
                        flickVelocity = 0;
                    }
                }
                else
                {
                    if (r.x < uiLastSongScrollPoint || r.x > uiFirstSongScrollPoint)
                    {
                        flickVelocity = 0;
                    }
                }

                if (FlickSpeed < 50f)
                {
                    GoToSong(NearestSong);
                }
                else // still moving
                {
                    if (isPortrait)
                    {
                        r.y += flickVelocity*delta;
                    }
                    else
                    {
                        r.x += flickVelocity*delta;
                    }
                    songList.RectPosition = r;
                }
            }
        }
    }
}
