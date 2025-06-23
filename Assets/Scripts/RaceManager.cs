using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class RaceManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public List<GameObject> aiPrefabs;
    public int aiCount = 3;
    public float aiSpacing = 1f;
    private Transform playerSpawnPoint;
    private GameObject playerInstance;
    public RaceUI raceUI;
    private List<AIVehicleController> aiControllers = new List<AIVehicleController>();
    public IReadOnlyList<AIVehicleController> AIControllers => aiControllers;

    void Start()
    {
        StartCoroutine(InitializeRaceSequence());
    }

    private IEnumerator InitializeRaceSequence()
    {
        yield return new WaitUntil(() => raceUI.IsReady());

        StartCoroutine(LoadRaceTrackAndSpawn());
    }

    IEnumerator LoadRaceTrackAndSpawn()
    {
        string trackName = PlayerPrefs.GetString("SelectedTrack", "Map_Arctic_Rush");

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(trackName, LoadSceneMode.Additive);
        yield return new WaitUntil(() => asyncLoad.isDone);

        playerSpawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPosition")?.transform;
        Transform[] waypoints = FindWaypoints();
        SpawnPlayer();
        List<AIVehicleController> aiVehicles = SpawnAI();
        foreach (var aiVehicle in aiVehicles)
        {
            aiVehicle.SetWaypoints(waypoints);
        }
    }

    Transform[] FindWaypoints()
    {
        GameObject waypointsParent = GameObject.Find("Waypoints");

        return waypointsParent.GetComponentsInChildren<Transform>()
            .Where(t => t != waypointsParent.transform)
            .OrderBy(obj => ExtractNumber(obj.name)) // Sort by the extracted number
            .Select(obj => obj.transform)
            .ToArray();
    }

    int ExtractNumber(string waypointName)
    {
        Match match = Regex.Match(waypointName, @"\d+");
        return match.Success ? int.Parse(match.Value) : int.MaxValue;
    }

    void SpawnPlayer()
    {
        if (playerPrefab != null && playerSpawnPoint != null)
        {
            playerInstance = Instantiate(playerPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
            if (playerInstance == null)
            {
                Debug.LogError("Spawned playerInstance is NULL!");
                return;
            }

            LapManager lapManager = playerInstance.GetComponentInChildren<LapManager>();
            if (lapManager == null)
            {
                Debug.LogError("LapManager script is missing on the spawned player or its children!");
            }
            else
            {
                lapManager.enabled = true;
                if (raceUI != null)
                {
                    lapManager.Initialize(raceUI);
                }
                else
                {
                    Debug.LogError("RaceUI reference is missing in RaceManager!");
                }

                StartCoroutine(EnsureLapManagerEnabled(lapManager));
            }
        }
    }

    private IEnumerator EnsureLapManagerEnabled(LapManager lapManager)
    {
        yield return null;
        if (lapManager != null)
        {
            lapManager.enabled = true;
        }
    }

    List<AIVehicleController> SpawnAI()
    {
        if (aiPrefabs == null || aiPrefabs.Count == 0 || playerSpawnPoint == null) return null;

        WaypointManager waypointManager = FindFirstObjectByType<WaypointManager>();
        if (waypointManager == null)
        {
            Debug.LogError("WaypointManager component not found in the track!");
            return null;
        }

        for (int i = 0; i < aiCount; i++)
        {
            Vector3 aiSpawnPoint = playerSpawnPoint.position - playerSpawnPoint.right * (i + 1) * aiSpacing;
            GameObject randomPrefab = aiPrefabs[Random.Range(0, aiPrefabs.Count)];
            GameObject aiInstance = Instantiate(randomPrefab, aiSpawnPoint, playerSpawnPoint.rotation);
            AIVehicleController aiController = aiInstance.GetComponent<AIVehicleController>();
            if (aiController != null)
            {
                aiController.enabled = true;
                aiController.waypointManager = waypointManager;
                aiController.ToggleEngine(false);
                aiControllers.Add(aiController);
            }
        }

        return aiControllers;
    }
}