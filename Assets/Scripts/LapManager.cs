using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LapManager : MonoBehaviour
{
    public int totalLaps = 3;
    public GameObject player;
    public TextMeshProUGUI totalLapsPlaceholder;
    public TextMeshProUGUI currentLapTimePlaceholder;
    public TextMeshProUGUI totalRaceTimePlaceholder;
    public TextMeshProUGUI raceFinishedLabel;
    public TextMeshProUGUI completedLapTimePlaceholder;

    private int currentLap = 1;
    private float lapStartTime;
    private float totalRaceTime = 0f;
    private List<float> lapTimes = new List<float>();

    private HashSet<GameObject> checkpointsPassed = new HashSet<GameObject>();
    private GameObject[] allCheckpoints;
    private bool lapCompleted = false;

    private void Start()
    {
        allCheckpoints = GameObject.FindGameObjectsWithTag("Checkpoint");
        lapStartTime = Time.time;
        totalRaceTimePlaceholder.enabled = false;
        raceFinishedLabel.enabled = false;
        completedLapTimePlaceholder.enabled = false;
        UpdateTotalLapsUI();
    }
    private void Update()
    {
        if (currentLap <= totalLaps && !lapCompleted)
        {
            UpdateCurrentLapTimeUI();
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
                UpdateTotalLapsUI();
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
        raceFinishedLabel.enabled = true;
        totalRaceTimePlaceholder.enabled = true;
        totalRaceTimePlaceholder.text = $"{FormatTime(totalRaceTime)}";
        Debug.Log("Race Completed! Lap Times:");
        for (int i = 0; i < lapTimes.Count; i++)
        {
            Debug.Log($"Lap {i + 1}: {FormatTime(lapTimes[i])}");
        }
        DisableVehicleControls();
    }

    private void DisableVehicleControls()
    {
        var vehicleController = player.GetComponent<VehicleController>();
        if (vehicleController != null)
        {
            vehicleController.StopVehicle();
        }
        else
        {
            Debug.LogWarning("VehicleController script not found on player object!");
        }
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        float seconds = time % 60;

        return $"{minutes:00}:{seconds:00.00}";
    }

    private void UpdateTotalLapsUI()
    {
        totalLapsPlaceholder.text = $"Laps: {currentLap}/{totalLaps}";
    }

    private void UpdateCurrentLapTimeUI()
    {
        currentLapTimePlaceholder.text = $"Time: {FormatTime(Time.time - lapStartTime)}";
    }

    private IEnumerator UpdateCompletedLapTimeUI(float lapTime)
    {
        completedLapTimePlaceholder.enabled = true;


        if (lapTimes.Count == 1)
        {
            completedLapTimePlaceholder.color = Color.white;
        }
        else if (lapTimes.Count > 1 && lapTime < lapTimes[lapTimes.Count - 2])
        {
            completedLapTimePlaceholder.color = Color.green;
        }
        else
        {
            completedLapTimePlaceholder.color = Color.red;
        }

        completedLapTimePlaceholder.text = $"Lap Time: {FormatTime(lapTime)}";
        yield return new WaitForSeconds(2f);
        completedLapTimePlaceholder.enabled = false;
    }
}