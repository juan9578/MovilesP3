using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class ServerSceneLoader : NetworkBehaviour
{
    public static ServerSceneLoader Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(Instance);
        }
    }

    public void CargarEscenario(int escenarioElegido)
    {
        // Se carga una escena u otra en función de lo escogido
        switch (escenarioElegido)
        {
            case 0:
                NetworkManager.Singleton.SceneManager.LoadScene("Laberinto1", LoadSceneMode.Single);
                break;
            case 1:
                NetworkManager.Singleton.SceneManager.LoadScene("Laberinto2", LoadSceneMode.Single);
                break;
            case 2:
                NetworkManager.Singleton.SceneManager.LoadScene("Laberinto3", LoadSceneMode.Single);
                break;
        }
    }
}
