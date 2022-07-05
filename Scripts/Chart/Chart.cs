/**
 * Chart.cs
 * Representation of a chart, constructed from a .mer file.
 *
 * by muskit
 * July 1, 2022
 **/

using Godot;
using System;
using System.Collections.Generic;

public class Chart
{
    /// <summary>
    /// All playable Notes in this Chart.
    /// 
    /// HIERARCHY:
    /// Key is measure.
    /// Value is List of (beat/1920, Notes) tuples.
    /// </summary>
    public Dictionary<int, List<(int, ChartNote)>> notes = new Dictionary<int, List<(int, ChartNote)>>();

    /// <summary>
    /// Construct Chart from contents of .mer file.
    /// </summary>
    /// <param name="mer">Contents of a .mer file.</param>    
    public Chart(string mer)
    {
        if (mer.Empty()) return;
        List<string> lines = new List<string>(mer.Split('\n'));
        foreach (var line in lines)
        {
            List<string> tokens = new List<string>(line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
            if (tokens.Count == 0) continue;
            if (tokens[0][0] == '#') continue;

            int currentMeasure = int.Parse(tokens[0]);
            int currentBeat = int.Parse(tokens[1]);

            if (!notes.ContainsKey(currentMeasure))
            {
                notes[currentMeasure] = new List<(int, ChartNote)>();
            }

            switch (tokens[2])
            {
                case "1": // common note types
                    switch(tokens[3])
                    {
                        case "1": // touch
                            notes[currentMeasure].Add((currentBeat, new ChartNote(int.Parse(tokens[5]), int.Parse(tokens[6]), type: NoteType.Touch)));
                            break;
                        case "2": // touch w/ bonus
                            notes[currentMeasure].Add((currentBeat, new ChartNote(int.Parse(tokens[5]), int.Parse(tokens[6]), type: NoteType.Touch, bonus: true)));
                            break;
                        case "20": // touch w/ bonus (+ "big effect")
                            notes[currentMeasure].Add((currentBeat, new ChartNote(int.Parse(tokens[5]), int.Parse(tokens[6]), type: NoteType.Touch, bonus: true)));
                            break;
                        case "16": // untimed
                            notes[currentMeasure].Add((currentBeat, new ChartNote(int.Parse(tokens[5]), int.Parse(tokens[6]), type: NoteType.Untimed)));
                            break;
                        case "26": // untimed w/ bonus
                            notes[currentMeasure].Add((currentBeat, new ChartNote(int.Parse(tokens[5]), int.Parse(tokens[6]), type: NoteType.Untimed, bonus: true)));
                            break;
                        case "9": // hold start
                            notes[currentMeasure].Add((currentBeat, new ChartNote(int.Parse(tokens[5]), int.Parse(tokens[6]), holdIndex: int.Parse(tokens[4]), holdNext: int.Parse(tokens[8]), type: NoteType.HoldStart)));
                            break;
                        case "25": // hold start (w/ bonus)
                            notes[currentMeasure].Add((currentBeat, new ChartNote(int.Parse(tokens[5]), int.Parse(tokens[6]), holdIndex: int.Parse(tokens[4]), holdNext: int.Parse(tokens[8]), type: NoteType.HoldStart, bonus: true)));
                            break;
                        case "10": // hold middle
                            notes[currentMeasure].Add((currentBeat, new ChartNote(int.Parse(tokens[5]), int.Parse(tokens[6]), holdIndex: int.Parse(tokens[4]), holdNext: int.Parse(tokens[8]), type: NoteType.HoldMid)));
                            break;
                        case "11": // hold end
                            notes[currentMeasure].Add((currentBeat, new ChartNote(int.Parse(tokens[5]), int.Parse(tokens[6]), holdIndex: int.Parse(tokens[4]), type: NoteType.HoldEnd)));
                            break;
                        case "3": // swipe in (red)
                            notes[currentMeasure].Add((currentBeat, new ChartNote(int.Parse(tokens[5]), int.Parse(tokens[6]), type: NoteType.SwipeIn)));
                            break;
                        case "21": // swipe in w/ bonus
                            notes[currentMeasure].Add((currentBeat, new ChartNote(int.Parse(tokens[5]), int.Parse(tokens[6]), type: NoteType.SwipeIn, bonus: true)));
                            break;
                        case "4": // swipe out (blue)
                            notes[currentMeasure].Add((currentBeat, new ChartNote(int.Parse(tokens[5]), int.Parse(tokens[6]), type: NoteType.SwipeOut)));
                            break;
                        case "22": // swipe out w/ bonus
                            notes[currentMeasure].Add((currentBeat, new ChartNote(int.Parse(tokens[5]), int.Parse(tokens[6]), type: NoteType.SwipeOut, bonus: true)));
                            break;
                        case "7": // swipe CCW
                            notes[currentMeasure].Add((currentBeat, new ChartNote(int.Parse(tokens[5]), int.Parse(tokens[6]), type: NoteType.SwipeCCW)));
                            break;
                        case "8": // swipe CCW w/ bonus
                            notes[currentMeasure].Add((currentBeat, new ChartNote(int.Parse(tokens[5]), int.Parse(tokens[6]), type: NoteType.SwipeCCW, bonus: true)));
                            break;
                        case "5": // swipe CW
                            notes[currentMeasure].Add((currentBeat, new ChartNote(int.Parse(tokens[5]), int.Parse(tokens[6]), type: NoteType.SwipeCW)));
                            break;
                        case "6": // swipe CW w/ bonus
                            notes[currentMeasure].Add((currentBeat, new ChartNote(int.Parse(tokens[5]), int.Parse(tokens[6]), type: NoteType.SwipeCW, bonus: true)));
                            break;
                        case "12": // BG add
                            notes[currentMeasure].Add((currentBeat, new ChartNote(int.Parse(tokens[5]), int.Parse(tokens[6]), type: NoteType.BGAdd)));
                            GD.Print("Found BGAdd in chart file!");
                            break;
                        case "13": // BG rem
                            notes[currentMeasure].Add((currentBeat, new ChartNote(int.Parse(tokens[5]), int.Parse(tokens[6]), type: NoteType.BGRem)));
                            GD.Print("Found BGRem in chart file!");
                            break;
                        
                    }
                    break;
                case "2": // tempo
                    notes[currentMeasure].Add((currentBeat, new ChartNote(value: float.Parse(tokens[3]), type: NoteType.Tempo)));
                    break;
            }
        }
    }
}
