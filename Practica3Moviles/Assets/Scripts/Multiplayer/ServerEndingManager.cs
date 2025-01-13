using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerEndingManager : NetworkBehaviour
{
    public int numJugadores;
    public bool seleccionEnviada = false;
    public int numSelecciones = 0;

    public GameObject textoEspera;

    void Update()
    {
        if(IsServer)
        {
            numJugadores = NetworkManager.Singleton.ConnectedClients.Count;
        }
    }

    public void VolverMenu()
    {
        if (seleccionEnviada) return;
        seleccionEnviada = true;
        textoEspera.SetActive(true);
        VolverMenuServerRpc();
    }

    [ServerRpc (RequireOwnership = false)]
    public void VolverMenuServerRpc()
    {
        numSelecciones++;
        if (numSelecciones == numJugadores)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Inicio", LoadSceneMode.Single);
        }
    }
}
