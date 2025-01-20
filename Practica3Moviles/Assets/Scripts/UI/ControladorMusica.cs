using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControladorMusica : MonoBehaviour
{
    public static ControladorMusica instancia;
    public bool musicaActivada;
    public AudioSource audioSource;
    public AudioClip clipVictoria;

    private void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        int activada = PlayerPrefs.GetInt("MusicEnabled", 1);
        if (activada == 1)
        {
            musicaActivada = true;
        }
        else
        {
            musicaActivada = false;
        }
    }

    void Update()
    {
        if (!musicaActivada)
        {
            audioSource.mute = true;
        }
        else
        {
            audioSource.mute = false;
        }
    }

    public void MusicaVictoria()
    {
        if (musicaActivada)
        {
            audioSource.Stop();
            audioSource.clip = clipVictoria;
            audioSource.Play();
        }
    }
}
