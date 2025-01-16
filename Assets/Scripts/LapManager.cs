using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LapManager : MonoBehaviour
{
    public int totalLaps = 3;

    // UI Elements
    public RaceUI raceUI;

    private int currentLap = 1;
    private float lapStartTime;
    private float totalRaceTime = 0f;
    private List<float> lapTimes = new List<float>();

    private HashSet<GameObject> checkpointsPassed = new HashSet<GameObject>();
    private GameObject[] allCheckpoints;
    private bool lapCompleted = false;
    private const int StartCountdownSeconds = 5;

    private void Start()
    {
        ToggleVehicle(false);
        StartCoroutine(StartRaceCountdown());
        allCheckpoints = GameObject.FindGameObjectsWithTag("Checkpoint");
        raceUI.ToggleRaceFinishedPlaceholder(false);
        raceUI.ToggleCompletedLapTimePlaceholder(false);
        raceUI.ToggleLapLeaderboard(false);
        raceUI.ToggleStartCountdownPlaceholder(true);
        raceUI.UpdateTotalLapsPlaceholder(currentLap, totalLaps);
    }

    private void Update()
    {
        if (currentLap <= totalLaps && !lapCompleted)
        {
            raceUI.UpdateCurrentLapTime($"Time: {TimeUtil.FormatTime(Time.time - lapStartTime)}");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint") && !checkpointsPassed.Contains(other.gameObject))
        {
            checkpointsPassed.Add(other.gameObject);
        }
        else if (other.CompareTag("Finish Checkpoint"))
        {
            HandleFinishCheckpoint();
        }
    }

    private void HandleFinishCheckpoint()
    {
        if (checkpointsPassed.Count == allCheckpoints.Length)
        {
            lapCompleted = true;
            float lapEndTime = Time.time;
            float lapTime = lapEndTime - lapStartTime; // Calculate lap time before resetting lapStartTime
            lapTimes.Add(lapTime);
            totalRaceTime += lapTime;

            if (currentLap < totalLaps)
            {
                StartCoroutine(UpdateCompletedLapTimeUI(lapTime));
                currentLap++;
                lapStartTime = lapEndTime; // Reset the lap timer
                checkpointsPassed.Clear(); // Reset checkpoints for the new lap
                lapCompleted = false;
                raceUI.UpdateTotalLapsPlaceholder(currentLap, totalLaps);
            }
            else
            {
                FinishRace();
            }
        }
        else
        {
            Debug.LogWarning("Finish checkpoint triggered, but not all checkpoints were passed!");
        }
    }

    private void FinishRace()
    {
        raceUI.ToggleLapHud(false);
        ToggleVehicle(false);
        EnableRaceFinishedText();
        StartCoroutine(ShowLeaderboardAfterDelay());
    }

    private void EnableRaceFinishedText()
    {
        raceUI.ToggleRaceFinishedPlaceholder(true);
        raceUI.UpdateTotalRaceTime($"{TimeUtil.FormatTime(totalRaceTime)}");
    }

    private void ToggleVehicle(bool isEnabled)
    {
        var vehicleControllers = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<IVehicle>();

        foreach (var controller in vehicleControllers)
        {
            controller.ToggleEngine(isEnabled);
        }
    }

    private IEnumerator UpdateCompletedLapTimeUI(float lapTime)
    {
        raceUI.ToggleCompletedLapTimePlaceholder(true);

        string timeDifferenceBetweenLaps = "";

        if (lapTimes.Count == 1)
        {
            raceUI.UpdateCompletedLapTimePlaceholderColor(Color.white);
        }
        else if (lapTimes.Count > 1)
        {
            float previousLapTime = lapTimes[lapTimes.Count - 2];
            float timeDifference = lapTime - previousLapTime;

            timeDifferenceBetweenLaps = $" ({(timeDifference > 0 ? "+" : "")}{timeDifference:F2})";

            raceUI.UpdateCompletedLapTimePlaceholderColor(timeDifference < 0 ? Color.green : Color.red);
        }

        raceUI.UpdateCompletedLapTimePlaceholderTime(
            $"Lap Time: {TimeUtil.FormatTime(lapTime)}{timeDifferenceBetweenLaps}");

        yield return new WaitForSeconds(2f);
        raceUI.ToggleCompletedLapTimePlaceholder(false);
    }

    private IEnumerator ShowLeaderboardAfterDelay()
    {
        yield return new WaitForSeconds(3f);

        raceUI.ToggleRaceFinishedPlaceholder(false);

        PopulateLapLeaderboard();
    }

    private IEnumerator StartRaceCountdown()
    {
        raceUI.ToggleLapHud(false);
        for (int i = StartCountdownSeconds; i > 0; i--)
        {
            raceUI.UpdateStartCountdownPlaceholderText(i.ToString());
            yield return new WaitForSeconds(1f);
        }

        raceUI.UpdateStartCountdownPlaceholderText("GO!");
        raceUI.ToggleLapHud(true);
        ToggleVehicle(true);
        lapStartTime = Time.time;
        yield return new WaitForSeconds(1f);
        raceUI.ToggleStartCountdownPlaceholder(false);
    }

    private void PopulateLapLeaderboard()
    {
        raceUI.ToggleLapLeaderboard(true);
        raceUI.ClearLapLeaderboard();
        raceUI.PopulateLapLeaderboard(lapTimes);
    }
}