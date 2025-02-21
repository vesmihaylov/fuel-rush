using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class RaceUI : MonoBehaviour
{
    public TextMeshProUGUI startCountdownPlaceholder;
    public TextMeshProUGUI totalLapsPlaceholder;
    public TextMeshProUGUI currentLapTimeLabel;
    public TextMeshProUGUI currentLapTimePlaceholder;
    public TextMeshProUGUI totalRaceTimePlaceholder;
    public TextMeshProUGUI raceFinishedLabel;
    public TextMeshProUGUI completedLapTimePlaceholder;
    public List<TextMeshProUGUI> playerNamesLeaderboardList;
    public List<TextMeshProUGUI> playerTotalTimeLeaderboardList;
    public GameObject lapLeaderboard;

    public void UpdateTotalLapsPlaceholder(int currentLap, int totalLaps)
    {
        totalLapsPlaceholder.text = $"Laps: {currentLap}/{totalLaps}";
    }

    public void UpdateTotalRaceTime(string time)
    {
        totalRaceTimePlaceholder.text = time;
    }

    public void UpdateCurrentLapTime(string time)
    {
        currentLapTimePlaceholder.text = time;
    }

    public void UpdateCompletedLapTimePlaceholderColor(Color color)
    {
        completedLapTimePlaceholder.color = color;
    }

    public void UpdateCompletedLapTimePlaceholderTime(string time)
    {
        completedLapTimePlaceholder.text = time;
    }

    public void UpdateStartCountdownPlaceholderText(string text)
    {
        startCountdownPlaceholder.text = text;
    }

    public void ToggleRaceFinishedPlaceholder(bool isEnabled)
    {
        raceFinishedLabel.enabled = isEnabled;
        totalRaceTimePlaceholder.enabled = isEnabled;
    }

    public void ToggleCompletedLapTimePlaceholder(bool isEnabled)
    {
        completedLapTimePlaceholder.enabled = isEnabled;
    }

    public void ToggleStartCountdownPlaceholder(bool isEnabled)
    {
        startCountdownPlaceholder.enabled = isEnabled;
    }

    public void ToggleLapLeaderboard(bool isEnabled)
    {
        lapLeaderboard.SetActive(isEnabled);
    }

    public void ToggleLapHud(bool isEnabled)
    {
        currentLapTimePlaceholder.enabled = isEnabled;
        currentLapTimeLabel.enabled = isEnabled;
        totalLapsPlaceholder.enabled = isEnabled;
    }

    public void ClearLapLeaderboard()
    {
        foreach (Transform child in playerNamesLeaderboardList[0].transform.parent)
        {
            if (child.gameObject != playerNamesLeaderboardList[0].gameObject)
                Destroy(child.gameObject);
        }

        foreach (Transform child in playerNamesLeaderboardList[0].transform.parent)
        {
            if (child.gameObject != playerNamesLeaderboardList[0].gameObject)
                Destroy(child.gameObject);
        }
    }

    public void PopulateLapLeaderboard(List<(string name, string totalTime)> results)
    {
        for (int i = 0; i < results.Count; i++)
        {
            var nameInstance =
                Instantiate(playerNamesLeaderboardList[0], playerNamesLeaderboardList[0].transform.parent);
            nameInstance.text = results[i].name;
            nameInstance.gameObject.SetActive(true);

            var timeInstance = Instantiate(playerTotalTimeLeaderboardList[0],
                playerTotalTimeLeaderboardList[0].transform.parent);
            timeInstance.text = results[i].totalTime;
            timeInstance.gameObject.SetActive(true);
        }
    }
}