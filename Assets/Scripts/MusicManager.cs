using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] musicTracks;
    private const string RaceManagerScene = "Race_Manager";

    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        
        if (sceneName == RaceManagerScene && musicTracks.Length > 0)
        {
            PlayRandomTrack();
        }
    }
    
    void PlayRandomTrack()
    {
        AudioClip randomTrack = musicTracks[Random.Range(0, musicTracks.Length)];
        audioSource.clip = randomTrack;
        audioSource.Play();
    }
}