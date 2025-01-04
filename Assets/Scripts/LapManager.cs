using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LapManager : MonoBehaviour
{
    public int totalLaps = 3;
    public GameObject player;

    // UI Elements
    public TextMeshProUGUI startCountdownPlaceholder;
    public TextMeshProUGUI totalLapsPlaceholder;
    public TextMeshProUGUI currentLapTimeLabel;
    public TextMeshProUGUI currentLapTimePlaceholder;
    public TextMeshProUGUI totalRaceTimePlaceholder;
    public TextMeshProUGUI raceFinishedLabel;
    public TextMeshProUGUI completedLapTimePlaceholder;
    public List<TextMeshProUGUI> lapNumbersLeaderboardList;
    public List<TextMeshProUGUI> lapTimesLeaderboardList;
    public GameObject lapLeaderboard;

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
        DisableVehicleControls();
        StartCoroutine(StartRaceCountdown());
        allCheckpoints = GameObject.FindGameObjectsWithTag("Checkpoint");
        totalRaceTimePlaceholder.enabled = false;
        raceFinishedLabel.enabled = false;
        completedLapTimePlaceholder.enabled = false;
        lapLeaderboard.SetActive(false);
        startCountdownPlaceholder.enabled = true;
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
        DisableLapHUD();
        DisableVehicleControls();
        EnableRaceFinishedText();
        StartCoroutine(ShowLeaderboardAfterDelay());
    }

    private void DisableLapHUD()
    {
        currentLapTimePlaceholder.enabled = false;
        currentLapTimeLabel.enabled = false;
        totalLapsPlaceholder.enabled = false;
    }

    private void EnableRaceFinishedText()
    {
        raceFinishedLabel.enabled = true;
        totalRaceTimePlaceholder.enabled = true;
        totalRaceTimePlaceholder.text = $"{FormatTime(totalRaceTime)}";
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

    private void EnableVehicleControls()
    {
        var vehicleController = player.GetComponent<VehicleController>();
        if (vehicleController != null)
        {
            vehicleController.StartVehicle();
        }
        else
        {
            Debug.LogWarning("VehicleController script not found on player object!");
        }
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

    private IEnumerator ShowLeaderboardAfterDelay()
    {
        yield return new WaitForSeconds(3f);

        raceFinishedLabel.enabled = false;
        totalRaceTimePlaceholder.enabled = false;

        PopulateLapLeaderboard();
    }

    private IEnumerator StartRaceCountdown()
    {
        for (int i = StartCountdownSeconds; i > 0; i--)
        {
            startCountdownPlaceholder.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        startCountdownPlaceholder.text = "GO!";
        EnableVehicleControls();
        lapStartTime = Time.time;
        yield return new WaitForSeconds(1f);
        startCountdownPlaceholder.enabled = false;
    }

    private void PopulateLapLeaderboard()
    {
        lapLeaderboard.SetActive(true);

        foreach (Transform child in lapNumbersLeaderboardList[0].transform.parent)
        {
            if (child.gameObject != lapNumbersLeaderboardList[0].gameObject)
                Destroy(child.gameObject);
        }
        foreach (Transform child in lapTimesLeaderboardList[0].transform.parent)
        {
            if (child.gameObject != lapTimesLeaderboardList[0].gameObject)
                Destroy(child.gameObject);
        }

        for (int i = 0; i < lapTimes.Count; i++)
        {
            var lapNumberInstance = Instantiate(lapNumbersLeaderboardList[0], lapNumbersLeaderboardList[0].transform.parent);
            lapNumberInstance.text = $"Lap {i + 1}";
            lapNumberInstance.gameObject.SetActive(true);

            var lapTimeInstance = Instantiate(lapTimesLeaderboardList[0], lapTimesLeaderboardList[0].transform.parent);
            lapTimeInstance.text = FormatTime(lapTimes[i]);
            lapTimeInstance.gameObject.SetActive(true);
        }
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        float seconds = time % 60;

        return $"{minutes:00}:{seconds:00.00}";
    }
}