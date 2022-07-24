/**
 * Misc.cs
 * Global values that are changed throughout the application's lifetime.
 *
 * by muskit
 * July 1, 2022
 **/
using Godot;
using System;
using System.Collections.Generic;

public class Misc : Node
{
    public static float cameraOffset = 0;
    public static float strikelineZPos = 0;
    public static float noteDrawDistance = 10;

    public static string currentMer = "";
    public static AudioStreamMP3 currentAudio;
    public static AudioStreamPlayer songPlayer;
    public static bool paused = false;
    public static List<Song> songList;
    public static string debugStr = ""; // MAX 5 LINES CAN BE SHOWN AT A TIME

    public static float Seg2Rad(float seg)
    {
        return Mathf.Deg2Rad(6f * seg);
    }

    public static float Rad2Seg(float angle)
    {
        return Mathf.Rad2Deg(angle)/6f;
    }

    public static int InterpInt(int a, int b, float ratio)
    {
        if (a == 0 && b == 0)
            return 0;
        return (int) Math.Round(a + (b-a)*ratio);
    }

    public static float InterpFloat(float a, float b, float ratio)
    {
        if (a == 0 && b == 0)
            return 0;
        return a + (b-a)*ratio;
    }

    public static float NearestAngle(float origin, float destination)
    {
        float result = destination;

        float plus = destination + 2f*Mathf.Pi;
        float minus = destination - 2f*Mathf.Pi;
        float minusDelta = Mathf.Abs(minus - origin);
        float normDelta = Mathf.Abs(destination - origin);
        float plusDelta = Mathf.Abs(plus - origin);
        if (plusDelta < normDelta)
            result = plus;
        if (minusDelta < normDelta)
            result = minus;

        return result;
    }

    public static float NearestSegment(int origin, int destination)
    {
        int result = destination % 60;

        int plus = destination + 60;
        int minus = destination - 60;
        int minusDelta = Math.Abs(minus - origin);
        int normDelta = Math.Abs(destination - origin);
        int plusDelta = Math.Abs(plus - origin);
        if (plusDelta < normDelta)
            result = plus;
        if (minusDelta < normDelta)
            result = minus;

        return result;
    }

    public static float ScreenPixelToRad(Vector2 pos)
    {
        var resolution = OS.WindowSize;
        var origin = new Vector2(resolution.x/2 - 1, resolution.y/2 - 1);

        return Mathf.Atan2(pos.y - origin.y, pos.x - origin.x);
    }

    public static int TouchPosToSegmentInt(Vector2 pos, Vector2 touchResolution)
    {
        var origin = new Vector2(touchResolution.x/2 - 1, touchResolution.y/2 - 1);
        var angle = Mathf.Atan2(pos.y - origin.y, pos.x - origin.x);

        if (angle > 0)
            angle = Mathf.Tau - angle;

        return Mathf.FloorToInt(Mathf.Abs(angle)/Mathf.Tau * 60) % 60;
    }

    public static int ScreenPixelToSegmentInt(Vector2 pos)
    {
        var angle = ScreenPixelToRad(pos);
        if (angle > 0)
            angle = Mathf.Tau - angle;

        return Mathf.FloorToInt(Mathf.Abs(angle)/Mathf.Tau * 60) % 60;
    }

    public static float NotePosition(int measure, int beat, float tempo, int beatsPerMeasure)
    {
        if (tempo == 0) return 0; // avoid divide by 0
        return TimeToPosition(60f/tempo * beatsPerMeasure * ((float)measure + (float)beat/1920f));
    }

    // TODO: notes scale to scroll position instead of strikeline
    // (where calibration offsets can be applied)
    public static Vector3 NoteScale(float zPos, float zOrigin = 0)
    {
        var val = zPos - zOrigin;
        if (val <=  Misc.noteDrawDistance)
        {
            var ratio = Mathf.Clamp((Misc.noteDrawDistance - val) / Misc.noteDrawDistance, 0, 1);
            return new Vector3(ratio, ratio, 1);
        }
        return Vector3.Zero;
    }

    public static float TimeToPosition(float time)
    {
        return time * PlaySettings.speedMultiplier * PlaySettings.SCROLL_MULT;
    }

    public static float PositionToTime(float pos)
    {
        return pos / PlaySettings.speedMultiplier / PlaySettings.SCROLL_MULT;
    }    

    public void LoadSong(string path, int difficulty = 0)
    {
        var chart = new File();
		var audio = new File();

        var chartPath = path + $"/{difficulty}.mer";
        var audioPath = path + "/music.mp3";
		Error errChart = chart.Open(chartPath, File.ModeFlags.Read);
		Error errAudio = audio.Open(audioPath, File.ModeFlags.Read);

		if (errChart != Error.Ok)
		{
			GD.PrintErr($"Trouble loading {chartPath}!\n{errChart}");
			return;
		}
		if (errAudio != Error.Ok)
		{
			GD.PrintErr($"Trouble loading {audioPath}!\n{errAudio}");
			return;
		}
		Misc.currentMer = chart.GetAsText();
		Misc.currentAudio = new AudioStreamMP3();
		Misc.currentAudio.Data = audio.GetBuffer((long)audio.GetLen());
		GetTree().ChangeScene("res://Scenes/Play.tscn");
    }

    public static bool IsInSegmentRegion(Note n, int segment)
    {
        int a = n.pos;
        int b = n.pos + n.size;
        if (b > 59)
        {
            b = 59;
            int c = 0;
            int d = b % 60;
            return (a <= segment && segment <= b && c <= segment && segment <= d);
        }
        else
        {
            return (a <= segment && segment <= b);
        }
    }
}
