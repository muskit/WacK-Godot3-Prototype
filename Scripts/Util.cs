/**
 * Util.cs
 * Various conversion functions.
 *
 * by muskit
 * July 26, 2022
 **/

using Godot;
using System;

namespace WacK
{
    public static class Util
    {
        public static float Seg2Rad(float seg)
        {
            return Mathf.DegToRad(6f * seg);
        }
        public static float Rad2Seg(float angle)
        {
            return Mathf.RadToDeg(angle) / 6f;
        }

        public static int InterpInt(int a, int b, float ratio)
        {
            if (a == 0 && b == 0)
                return 0;
            return (int)Math.Round(a + (b - a) * ratio);
        }

        public static float InterpFloat(float a, float b, float ratio)
        {
            if (a == 0 && b == 0)
                return 0;
            return a + (b - a) * ratio;
        }

        // Returns an equivalent destination angle that's closest to the origin.
        public static float NearestAngle(float origin, float destination)
        {
            float result = destination;

            float plus = destination + 2f * Mathf.Pi;
            float minus = destination - 2f * Mathf.Pi;
            float minusDelta = Mathf.Abs(minus - origin);
            float normDelta = Mathf.Abs(destination - origin);
            float plusDelta = Mathf.Abs(plus - origin);
            if (plusDelta < normDelta)
                result = plus;
            if (minusDelta < normDelta)
                result = minus;

            return result;
        }

        // Return an equivalent minute that's closest to the origin.
        public static float NearestMinute(int origin, int destination)
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
            var resolution = DisplayServer.WindowGetSize();
            var origin = new Vector2(resolution.x / 2 - 1, resolution.y / 2 - 1);

            return Mathf.Atan2(pos.y - origin.y, pos.x - origin.x);
        }

        public static int TouchPosToSegmentInt(Vector2 pos, Vector2 touchResolution)
        {
            var origin = new Vector2(touchResolution.x / 2 - 1, touchResolution.y / 2 - 1);
            var angle = Mathf.Atan2(pos.y - origin.y, pos.x - origin.x);

            if (angle > 0)
                angle = Mathf.Tau - angle;

            return Mathf.FloorToInt(Mathf.Abs(angle) / Mathf.Tau * 60) % 60;
        }

        public static int ScreenPixelToSegmentInt(Vector2 pos)
        {
            var angle = ScreenPixelToRad(pos);
            if (angle > 0)
                angle = Mathf.Tau - angle;

            return Mathf.FloorToInt(Mathf.Abs(angle) / Mathf.Tau * 60) % 60;
        }

        public static float NotePosition(int measure, int beat, float tempo, int beatsPerMeasure)
        {
            if (tempo == 0) return 0; // avoid divide by 0
            return TimeToPosition(60f / tempo * beatsPerMeasure * ((float)measure + (float)beat / 1920f));
        }

        public static float NoteTime(int measure, int beat, float tempo, int beatsPerMeasure)
        {
            if (tempo == 0) return 0; // avoid divide by 0

            return 60f / tempo * beatsPerMeasure * ((float)measure + (float)beat / 1920f);
        }

        public static float TimeToPosition(double time)
        {
            return (float)time * UserSettings.playSpeedMultiplier * UserSettings.SCROLL_MULT;
        }

        public static float PositionToTime(float pos)
        {
            return pos / UserSettings.playSpeedMultiplier / UserSettings.SCROLL_MULT;
        }

        // TODO: notes scale to scroll position instead of strikeline
        // (where calibration offsets can be applied)
        public static Vector3 NoteScale(float zPos, float zOrigin = 0)
        {
            var val = zPos - zOrigin;
            if (val <= Misc.noteDrawDistance)
            {
                var ratio = Mathf.Clamp((Misc.noteDrawDistance - val) / Misc.noteDrawDistance, 0, 1);
                return new Vector3(ratio, ratio, 1);
            }
            return Vector3.Zero;
        }

        public static string DifficultyValueToString(float diffPoint)
        {
            return Mathf.FloorToInt(diffPoint).ToString() + (diffPoint > Mathf.Floor(diffPoint) ? "+" : "");
        }
    }
}