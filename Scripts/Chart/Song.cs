/**
 * Song.cs
 * Represent a song's charts and metadata.
 *
 * by muskit
 * July 8, 2022
 **/

using Godot;
using System;

namespace WacK
{
    public partial class Song : Godot.Object
    {
        public DirAccess directory;
        public string name;
        public string artist;
        public string category;
        public float tempo;
        public ImageTexture jacketTexture;

        // difficulties: -1 = chart doesn't exist; 0 = no diff info avail.
        public float[] difficulty = {-1, -1, -1, -1};
        public DifficultyLevel MaxDifficulty
        {
            get
            {
                var result = DifficultyLevel.Inferno;
                while (difficulty[(int)result] == -1)
                    --result;
                return result;
            }
        }
        public DifficultyLevel MinDifficulty
        {
            get
            {
                {
                    var result = DifficultyLevel.Normal;
                    while (difficulty[(int)result] == -1)
                        ++result;
                    return result;
                }
            }
        }

        /// Summary:
        /// path: path to song folder (must contain song.ini)
        public Song(string path)
        {
            directory = DirAccess.Open(path);
            if (directory == null) return;

            if (directory.FileExists("song.ini"))
            {
                using FileAccess fileIni = FileAccess.Open($"{directory.GetCurrentDir()}/song.ini", FileAccess.ModeFlags.Read);
                if (fileIni == null)
                    return;
                
                var songIni = new ConfigFile();
                songIni.Load(fileIni.GetPath());

                name = (string)songIni.GetValue("SongInfo", "name", "???");
                artist = (string)songIni.GetValue("SongInfo", "artist", "???");
                category = (string)songIni.GetValue("SongInfo", "category", "???");
                
                var iTempo = songIni.GetValue("SongInfo", "tempo", -1f);
                tempo = (float)iTempo;

                if (directory.FileExists("0.mer"))
                {
                    var iNormal = songIni.GetValue("ChartInfo", "normal", 0);
                    difficulty[0] = (float)iNormal;
                }

                if (directory.FileExists("1.mer"))
                {
                    var iHard = songIni.GetValue("ChartInfo", "hard", 0);
                    difficulty[1] = (float)iHard;
                }

                if (directory.FileExists("2.mer"))
                {
                    var iExpert = songIni.GetValue("ChartInfo", "expert", 0);
                    difficulty[2] = (float)iExpert;
                }

                if (directory.FileExists("3.mer"))
                {
                    var iInferno = songIni.GetValue("ChartInfo", "inferno", 0);
                    difficulty[3] = (float)iInferno;
                }

                directory.ListDirBegin();
                var curFile = "a";
                while (curFile != "")
                {
                    curFile = directory.GetNext();
                    if (curFile.StartsWith("jacket"))
                    {
                        var img = new Image();
                        var err = img.Load($"{directory.GetCurrentDir()}/{curFile}");
                        if (err != Error.Ok)
                            continue;
                        else
                        {
                            jacketTexture = ImageTexture.CreateFromImage(img);
                            break;
                        }
                    }
                }
            }
        }

        public override string ToString()
        {
            return $"{artist} - {name} ({tempo} BPM): {difficulty[0]}, {difficulty[1]}, {difficulty[2]}, {difficulty[3]}";
        }
    }
}