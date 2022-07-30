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
    public class Song : Godot.Object
    {
        public Directory directory;
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
            directory = new Directory();
            if (directory.Open(path) != Error.Ok) return;

            if (directory.FileExists("song.ini"))
            {
                File fileIni = new File();
                if (fileIni.Open($"{directory.GetCurrentDir()}/song.ini", File.ModeFlags.Read) != Error.Ok)
                    return;
                
                var songIni = new ConfigFile();
                songIni.Load(fileIni.GetPath());

                name = songIni.GetValue("SongInfo", "name", "???") as string;
                artist = songIni.GetValue("SongInfo", "artist", "???") as string;
                category = songIni.GetValue("SongInfo", "category", "???") as string;
                
                var iTempo = songIni.GetValue("SongInfo", "tempo", -1f);
                tempo = iTempo is Int32 ? (Int32)iTempo : (float)iTempo;

                if (directory.FileExists("0.mer"))
                {
                    var iNormal = songIni.GetValue("ChartInfo", "normal", 0);
                    difficulty[0] = iNormal is Int32 ? (Int32)iNormal : (float)iNormal;
                }

                if (directory.FileExists("1.mer"))
                {
                    var iHard = songIni.GetValue("ChartInfo", "hard", 0);
                    difficulty[1] = iHard is Int32 ? (Int32)iHard : (float)iHard;
                }

                if (directory.FileExists("2.mer"))
                {
                    var iExpert = songIni.GetValue("ChartInfo", "expert", 0);
                    difficulty[2] = iExpert is Int32 ? (Int32)iExpert : (float)iExpert;
                }

                if (directory.FileExists("3.mer"))
                {
                    var iInferno = songIni.GetValue("ChartInfo", "inferno", 0);
                    difficulty[3] = iInferno is Int32 ? (Int32)iInferno : (float)iInferno;
                }

                directory.ListDirBegin();
                var curFile = "a";
                while (!curFile.Empty())
                {
                    curFile = directory.GetNext();
                    if (curFile.BeginsWith("jacket"))
                    {
                        var img = new Image();
                        var err = img.Load($"{directory.GetCurrentDir()}/{curFile}");
                        if (err != Error.Ok)
                            continue;
                        else
                        {
                            jacketTexture = new ImageTexture();
                            jacketTexture.CreateFromImage(img);
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