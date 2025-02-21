using UnityEngine;

public static class TimeUtil
{
    public static string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        int milliseconds = Mathf.FloorToInt((timeInSeconds * 100) % 100);

        return $"{minutes:00}:{seconds:00}:{milliseconds:00}";
    }

    public static string FormatRaceTimeForLeaderboard(float time)
    {
        return time == float.MaxValue ? "DID NOT FINISH" : FormatTime(time);
    }
}