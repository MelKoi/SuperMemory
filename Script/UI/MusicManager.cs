using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    public List<AudioClip> GameBGM = new List<AudioClip>();
    AudioSource audioSource;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Menu")
        {
            PlayMusic(GameBGM[0]);
        }
        else if (scene.name == "Room")
        {
            PlayMusic(GameBGM[1]);
        }
        else if(scene.name == "GreenHand")
        {
            PlayMusic(GameBGM[2]);
        }
    }
    private void PlayMusic(AudioClip clip)
    {
        if (audioSource.clip != clip)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
    // Update is called once per frame
    void Update()
    {
       
    }
}
