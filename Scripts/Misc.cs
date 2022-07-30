/**
 * Misc.cs
 * Global values that are changed throughout the game's lifetime.
 *
 * by muskit
 * July 1, 2022
 **/
using Godot;
using System;
using System.Collections.Generic;

namespace WacK
{
    public class Misc : Node
    {
        public static float cameraOffset = 0;
        public static float strikelineZPos = 0;
        public static float noteDrawDistance = 10;

        public static string currentMer = "";
        public static Chart currentChart;
        public static AudioStreamMP3 currentAudio;
        public static AudioStreamPlayer songPlayer;
        public static bool paused = false;
        public static List<Song> songList;
        public static string debugStr { get; private set; } = "";

        public static void DebugPrintln(params object[] list)
        {
            string result = String.Empty;
            foreach (var obj in list)
            {
                result += obj?.ToString();
            }
            debugStr += $"{result}\n";
            GD.Print(result);
        }
        public static void DebugClear()
        {
            debugStr = "";
        }
        public static void DebugSetStr(params object[] list)
        {
            debugStr = "";
            DebugPrintln(list);
        }

        public void LoadSong(string path, DifficultyLevel difficulty = DifficultyLevel.Normal)
        {
            var chart = new File();
            var audio = new File();

            var chartPath = path + $"/{(int)difficulty}.mer";
            var audioPath = path + "/music.mp3";
            Error errChart = chart.Open(chartPath, File.ModeFlags.Read);
            Error errAudio = audio.Open(audioPath, File.ModeFlags.Read);

            if (errChart != Error.Ok)
            {
                DebugPrintln($"Trouble loading {chartPath}!\n{errChart}");
                return;
            }
            if (errAudio != Error.Ok)
            {
                DebugPrintln($"Trouble loading {audioPath}!\n{errAudio}");
                return;
            }
            Misc.currentMer = chart.GetAsText();
            Misc.currentAudio = new AudioStreamMP3();
            Misc.currentAudio.Data = audio.GetBuffer((long)audio.GetLen());
            GetTree().ChangeScene("res://Scenes/Play.tscn");
        }

        public static bool NoteIsInSegmentRegion(Note n, int segment)
        {
            int a = n.pos;
            int b = n.pos + n.size;
            return IsInSegmentRegion(a, b, segment);
        }

        public static bool IsInSegmentRegion(int a, int b, int segment)
        {
            if (b > 59)
            {
                int c = 0;
                int d = b % 60;
                b = 59;
                return ((a <= segment && segment <= b) || (c <= segment && segment <= d));
            }
            else
            {
                return (a <= segment && segment <= b);
            }
        }
    }
}
