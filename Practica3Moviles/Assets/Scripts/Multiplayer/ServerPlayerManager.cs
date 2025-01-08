using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ServerPlayerManager : NetworkBehaviour
{
    public static ServerPlayerManager instance;

    public int numJugadores;

    public const int MAX_JUGADORES = 4; // Número de jugadores necesarios para empezar la partida
    public const int MIN_JUGADORES = 2; // Número de jugadores necesarios para empezar la cuenta atrás

    public bool temporizadorActivo = false; // Variable que indica si se ha comenzado la cuenta atrás
    public float temporizador = 0f;
    public float TIEMPO_ESPERA = 120f;

    public bool partidaComenzada = false;
    public bool abandonandoSala = false;

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
    }

    void Update()
    {
        if (IsServer && !partidaComenzada)
        {
            // Se calcula el número de jugadores conectados
            if (!NetworkManager.Singleton.IsConnectedClient) return;
            numJugadores = NetworkManager.Singleton.ConnectedClients.Count;
            
            // En función del número de jugadores conectados, se realiza una acción u otra
            // Si hay más de 2 jugadores, se activa el temporizador, en caso de que no esté ya activo
            if (numJugadores >= MIN_JUGADORES && !temporizadorActivo && !abandonandoSala)
            {
                temporizadorActivo = true;
            }
            // Si hay más de 2 jugadores, se sigue con el temporizador
            if (numJugadores >= MIN_JUGADORES && temporizadorActivo && !abandonandoSala)
            {
                temporizador += Time.deltaTime;
                ActualizarTemporizadorClientRpc(temporizador);
            }
            // Si han pasado los 2 minutos, o los 4 jugadores están listos, se comienza la partida
            if (temporizador >= TIEMPO_ESPERA || numJugadores == MAX_JUGADORES)
            {
                partidaComenzada = true;
                ComenzarPartidaClientRpc();
            }
            // Si hay menos de 2 jugadores y el temporizador estaba activado, quiere decir que algún jugador se ha desconectado, por lo que se reinicia este
            if (numJugadores < MIN_JUGADORES && temporizadorActivo)
            {
                temporizador = 0f;
                temporizadorActivo = false;
                DesactivarTemporizadorClientRpc();
            }
        }
    }

    [ClientRpc]
    public void ComenzarPartidaClientRpc()
    {
        MenuLobby.instance.ComenzarPartida();
    }

    [ClientRpc]
    public void ActualizarTemporizadorClientRpc(float tiempo)
    {
        if (abandonandoSala) return;
        temporizadorActivo = true;
        temporizador = tiempo;
    }

    [ClientRpc]
    public void DesactivarTemporizadorClientRpc()
    {
        temporizador = 0f;
        temporizadorActivo = false;
    }

    public void AbandonarSala()
    {
        if (IsServer)
        {
            // Se indica que se está abandonando la sala, para no actualizar el temporizador
            abandonandoSala = true;
            temporizadorActivo = false;
            temporizador = 0f;
            DesactivarTemporizadorClientRpc();
            NotificarDesconexionClientRpc();
        }
    }

    [ClientRpc]
    public void NotificarDesconexionClientRpc()
    {
        // Se muestra un mensaje de desconexión y después se vuelve al menú anterior, sólo en los clientes
        if (IsHost) return;
        StartCoroutine(DesconexionHost());
    }

    private IEnumerator DesconexionHost()
    {
        MenuLobby.instance.mensajeDesconexion.SetActive(true);
        MenuLobby.instance.mensajeEspera.SetActive(false);
        temporizadorActivo = false;
        abandonandoSala = true;
        yield return new WaitForSeconds(2f);
        MenuLobby.instance.mensajeDesconexion.SetActive(false);
        MenuLobby.instance.mensajeEspera.SetActive(true);
        temporizadorActivo = false;
        abandonandoSala = false;
        MenuInicial.instancia.panelLobby.SetActive(false);
        MenuInicial.instancia.panelPartida.SetActive(true);
    }

}
