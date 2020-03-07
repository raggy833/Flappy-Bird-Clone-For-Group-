using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    #region Static instance
    private static AudioManager instance;
    public static AudioManager Instance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<AudioManager>();
                if (instance == null) {
                    instance = new GameObject("Spawned Audio Manager", typeof(AudioManager)).GetComponent<AudioManager>();
                }
            }
            return instance;
        }
        private set {
            instance = value;
        }
    }
    #endregion

    #region Fields
    private AudioSource musicSource;
    private AudioSource sfxSource;

    #endregion

    private void Awake() {
        // Make sure not to destroy this instance
        DontDestroyOnLoad(this.gameObject);

        // Create audio sources, and save them as references
        musicSource = this.gameObject.AddComponent<AudioSource>();
        sfxSource = this.gameObject.AddComponent<AudioSource>();

        // Loop the music
        musicSource.loop = true;

    }


    public void PlayMusic(AudioClip musicClip) {

        musicSource.clip = musicClip;
        musicSource.volume = 0.5f;
        musicSource.Play();
    }

    public void StopMusic(AudioClip musicClip) {

        musicSource.volume = 0.5f;
        musicSource.Stop();
    }

    public void PauseMusic(AudioClip musicClip) {

        musicSource.Pause();
    }

}
