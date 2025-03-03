using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] musicTracks;
    private const string MapsPrefix = "Map_";

    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        
        if (sceneName.StartsWith(MapsPrefix) && musicTracks.Length > 0)
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