using System.Collections;
using System.Collections.Generic;
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
    private bool isRaceFinished = false;
    private const int StartCountdownSeconds = 5;
    private IVehicle vehicleController;

    private void Awake()
    {
        vehicleController = GetComponent<IVehicle>();
        allCheckpoints = GameObject.FindGameObjectsWithTag("Checkpoint");

        raceUI.ToggleRaceFinishedPlaceholder(false);
        raceUI.ToggleCompletedLapTimePlaceholder(false);
        raceUI.ToggleLapLeaderboard(false);
        raceUI.ToggleStartCountdownPlaceholder(false);
        raceUI.ToggleLapHud(false);
        if (IsPlayer())
        {
            raceUI.UpdateTotalLapsPlaceholder(currentLap, totalLaps);
        }

        vehicleController.ToggleEngine(false);
    }

    private void Start()
    {
        StartCoroutine(StartRaceCountdown());
    }

    private void Update()
    {
        if (!isRaceFinished && currentLap <= totalLaps && !lapCompleted && IsPlayer())
        {
            raceUI.UpdateCurrentLapTime($"Time: {TimeUtil.FormatTime(Time.time - lapStartTime)}");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isRaceFinished) return;

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
        if (isRaceFinished || checkpointsPassed.Count != allCheckpoints.Length)
        {
            return;
        }

        lapCompleted = true;
        float lapEndTime = Time.time;
        float lapTime = lapEndTime - lapStartTime;

        if (IsPlayer())
        {
            lapTimes.Add(lapTime);
            totalRaceTime += lapTime;
            StartCoroutine(UpdateCompletedLapTimeUI(lapTime));
        }

        if (currentLap >= totalLaps)
        {
            FinishRace();
            return;
        }

        currentLap++;
        lapStartTime = lapEndTime;
        checkpointsPassed.Clear();
        lapCompleted = false;
        if (IsPlayer())
        {
            raceUI.UpdateTotalLapsPlaceholder(currentLap, totalLaps);
        }
    }

    private void FinishRace()
    {
        isRaceFinished = true;
        vehicleController.ToggleEngine(false);
        if (IsPlayer())
        {
            raceUI.ToggleLapHud(false);
            EnableRaceFinishedText();
            StartCoroutine(ShowLeaderboard());
        }
    }

    private void EnableRaceFinishedText()
    {
        raceUI.ToggleRaceFinishedPlaceholder(true);
        raceUI.UpdateTotalRaceTime($"{TimeUtil.FormatTime(totalRaceTime)}");
    }

    private IEnumerator UpdateCompletedLapTimeUI(float lapTime)
    {
        raceUI.ToggleCompletedLapTimePlaceholder(true);

        string timeDifferenceBetweenLaps = "";
        Color textColor = Color.white;

        if (lapTimes.Count > 1)
        {
            float previousLapTime = lapTimes[lapTimes.Count - 2];
            float timeDifference = lapTime - previousLapTime;
            timeDifferenceBetweenLaps = $" ({(timeDifference > 0 ? "+" : "")}{timeDifference:F2})";
            textColor = timeDifference < 0 ? Color.green : Color.red;
        }

        raceUI.UpdateCompletedLapTimePlaceholderColor(textColor);
        raceUI.UpdateCompletedLapTimePlaceholderTime(
            $"Lap Time: {TimeUtil.FormatTime(lapTime)}{timeDifferenceBetweenLaps}");

        yield return new WaitForSeconds(2f);
        raceUI.ToggleCompletedLapTimePlaceholder(false);
    }

    private IEnumerator ShowLeaderboard()
    {
        yield return new WaitForSeconds(3f);
        raceUI.ToggleRaceFinishedPlaceholder(false);
        PopulateLapLeaderboard();
    }

    private IEnumerator StartRaceCountdown()
    {
        raceUI.ToggleLapHud(false);
        raceUI.ToggleStartCountdownPlaceholder(true);

        for (int i = StartCountdownSeconds; i > 0; i--)
        {
            raceUI.UpdateStartCountdownPlaceholderText(i.ToString());
            yield return new WaitForSeconds(1f);
        }

        raceUI.UpdateStartCountdownPlaceholderText("GO!");

        yield return new WaitForSeconds(0.5f);
        raceUI.ToggleStartCountdownPlaceholder(false);
        raceUI.ToggleLapHud(true);
        vehicleController.ToggleEngine(true);
        lapStartTime = Time.time;
    }

    private void PopulateLapLeaderboard()
    {
        raceUI.ToggleLapLeaderboard(true);
        raceUI.ClearLapLeaderboard();
        raceUI.PopulateLapLeaderboard(lapTimes);
    }

    private bool IsPlayer()
    {
        return gameObject.CompareTag("Player");
    }
}