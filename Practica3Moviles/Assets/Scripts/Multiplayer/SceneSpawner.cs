using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSpawner : NetworkBehaviour
{
    public static SceneSpawner instance;

    private string escenaAnterior = "Inicio";

    // Puntos de aparición de los jugadores
    public Vector3[] spawnsLaberinto1;
    public Vector3[] spawnsLaberinto2;
    public Vector3[] spawnsLaberinto3;

    // Prefab del jugador
    public GameObject playerPrefab;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        // Se obtiene el nombre de la escena actual
        string escenaActual = SceneManager.GetActiveScene().name;
        // Si no se ha cambiado de escena, no se hace nada
        if (escenaActual == escenaAnterior) return;
        switch(escenaActual)
        {
            case "Laberinto1":
                SpawnJugadores(0);
                break;
            case "Laberinto2":
                SpawnJugadores(1);
                break;
            case "Laberinto3":
                SpawnJugadores(2);
                break;
        }
        escenaAnterior = escenaActual;
    }

    public void SpawnJugadores(int idLaberinto)
    {
        switch (idLaberinto)
        {
            case 0:
                if (IsServer)
                {
                    for(int i = 0; i < NetworkManager.Singleton.ConnectedClientsIds.Count; i++)
                    {
                        // Instancia el objeto en el servidor, en la posición adecuada
                        GameObject playerInstance = Instantiate(playerPrefab, spawnsLaberinto1[i], Quaternion.identity);
                        // Se spawnea en red y se le asigna al cliente corresponediente
                        NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
                        networkObject.SpawnAsPlayerObject(NetworkManager.Singleton.ConnectedClientsIds[i], true);
                    }
                }
                break;
            case 1:
                if (IsServer)
                {
                    for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsIds.Count; i++)
                    {
                        // Instancia el objeto en el servidor, en la posición adecuada
                        GameObject playerInstance = Instantiate(playerPrefab, spawnsLaberinto2[i], Quaternion.identity);
                        // Se spawnea en red y se le asigna al cliente corresponediente
                        NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
                        networkObject.SpawnAsPlayerObject(NetworkManager.Singleton.ConnectedClientsIds[i], true);
                    }
                }
                break;
            case 2:
                if (IsServer)
                {
                    for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsIds.Count; i++)
                    {
                        // Instancia el objeto en el servidor, en la posición adecuada
                        GameObject playerInstance = Instantiate(playerPrefab, spawnsLaberinto3[i], Quaternion.identity);
                        // Se spawnea en red y se le asigna al cliente corresponediente
                        NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
                        networkObject.SpawnAsPlayerObject(NetworkManager.Singleton.ConnectedClientsIds[i], true);
                    }
                }
                break;
        }
    }



}
