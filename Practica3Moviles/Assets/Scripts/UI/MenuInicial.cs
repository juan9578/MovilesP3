using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuInicial : MonoBehaviour
{
    public GameObject panelInicio;
    public GameObject panelConfiguracion;
    public GameObject panelPartida;
    public GameObject panelPrivado;
    public GameObject panelClave;
    public GameObject panelLobby;
    public GameObject claveLobby;

    public string codigoInvitacion;

    // Método para cambiar de panel y comenzar a jugar
    public void Comenzar()
    {
        panelInicio.SetActive(false);
        panelPartida.SetActive(true);
    }

    // Método para mostrar el menú de configuración
    public void MostrarConfiguracion()
    {
        panelConfiguracion.SetActive(true);
        panelInicio.SetActive(false);
    }

    // Método para salir del juego
    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    // Método para buscar una partida
    public async void BuscarPartida()
    {
        // Primero se comprueba si hay salas disponibles
        bool lobbyDisponible = await LobbyController.instancia.CheckLobbies();
        // Si hay salas disponibles, se une a la primera que haya
        if (lobbyDisponible)
        {
            await LobbyController.instancia.QuickJoinLobbyAsync();
        }
        else
        {
            // Si no hay ninguna sala, se crea una nueva
            await LobbyController.instancia.CreatePublicLobby();
        }

        panelPartida.SetActive(false);
        panelLobby.SetActive(true);
        claveLobby.SetActive(false);
    }

    // Método para pasar al menú de invitación de amigos
    public void MostrarPanelInvitacion()
    {
        panelPartida.SetActive(false);
        panelPrivado.SetActive(true);
    }

    // Método para crear una sala privada
    public async void CrearSalaPrivada()
    {
        await LobbyController.instancia.CreatePrivateLobby();

        panelPrivado.SetActive(false);
        panelLobby.SetActive(true);
        claveLobby.SetActive(true);

        // Se muestra la clave del lobby en la pantalla
        claveLobby.GetComponent<TMP_Text>().text = "CLAVE DE INVITACIÓN \n" + LobbyController.instancia.lobbyCode;
    }

    // Método para mostrar el panel de contraseña
    public void MostrarPanelClave()
    {
        panelPrivado.SetActive(false);
        panelClave.SetActive(true);
    }

    // Método para introducir la clave de la sala
    public void IntroducirClave(string clave)
    {
        codigoInvitacion = clave;
    }

    // Método para unirse a una sala privada, con una contraseña
    public async void EntrarSalaPrivada()
    {
        bool unidoSala = false;
        await LobbyController.instancia.JoinPrivateLobby(codigoInvitacion, (exito) => unidoSala = exito);
        if (!unidoSala)
        {
            Debug.Log("La clave introducida no es correcta");
        }
        panelClave.SetActive(false);
        panelLobby.SetActive(true);
        claveLobby.SetActive(true);

        // Se muestra la clave del lobby en la pantalla
        claveLobby.GetComponent<TMP_Text>().text = "CLAVE DE INVITACIÓN \n" + LobbyController.instancia.lobbyCode;
    }

    // Método para volver entre pantallas
    public void Volver()
    {
        if(panelConfiguracion.activeSelf)
        {
            panelConfiguracion.SetActive(false);
            panelInicio.SetActive(true);
        }
        else if (panelPartida.activeSelf)
        {
            panelPartida.SetActive(false);
            panelInicio.SetActive(true);
        }
        else if (panelPrivado.activeSelf)
        {
            panelPrivado.SetActive(false);
            panelPartida.SetActive(true);
        }
        else if (panelClave.activeSelf)
        {
            panelClave.SetActive(false);
            panelPrivado.SetActive(true);
        }
    }

    // Método para abandonar el lobby
    public void AbandonarSala()
    {
        panelLobby.SetActive(false);
        panelPartida.SetActive(true);
    }



}
