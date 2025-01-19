using UnityEngine;

public class ControladorEfectosSonido : MonoBehaviour
{
    public AudioClip clickSound; // El sonido a reproducir
    private AudioSource audioSource;

    private void Start()
    {
        // Agregar o buscar un componente AudioSource en el GameObject
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlayClickSound()
    {
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound); // Reproducir el sonido
        }
    }
}
