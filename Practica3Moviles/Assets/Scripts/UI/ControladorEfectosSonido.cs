using UnityEngine;

public class ControladorEfectosSonido : MonoBehaviour
{
    public static ControladorEfectosSonido instancia;

    public AudioSource audioSource;
    public bool efectosActivados;
    public AudioClip clickSound;
    public AudioClip collisionSound;

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

    private void Start()
    {
        int activada = PlayerPrefs.GetInt("SoundEffectsEnabled");
        if (activada == 1)
        {
            efectosActivados = true;
        }
        else
        {
            efectosActivados = false;
        }
    }


    public void SonidoClick()
    {
        if (!efectosActivados) return;
        audioSource.clip = clickSound;
        audioSource.loop = false;
        audioSource.Play();
    }

    public void SonidoColision()
    {
        if (!efectosActivados) return;
        audioSource.clip = collisionSound;
        audioSource.loop = false;
        audioSource.Play();
    }
}
