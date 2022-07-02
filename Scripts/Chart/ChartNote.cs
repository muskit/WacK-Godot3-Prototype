/**
 * Note.cs
 * A struct representing a note.
 *
 * by muskit
 * July 1, 2022
 **/

public enum NoteType
{
    Touch, HoldStart, HoldMid, HoldEnd, Untimed, SwipeIn, SwipeOut, SwipeCW, SwipeCCW, Tempo, BeatsPerMeasure, BGAdd, BGRem
}
public struct ChartNote
{
    public NoteType noteType { get; private set; }
    public bool isBonus { get; private set; }

    // Radial values in minutes
    public int position { get; private set; }
    public int size { get; private set; } // 1 <= size <= 60
    public float value { get; private set; }

    public ChartNote(int position = 0, int size = 1, float value = 0f, NoteType type = NoteType.Touch, bool bonus = false)
    {
        this.position = position;
        this.size = size;
        this.value = value;
        this.noteType = type;
        this.isBonus = bonus;
    }
}