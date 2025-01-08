using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuInicial : MonoBehaviour
{
    public static MenuInicial instancia;

    public GameObject panelInicio;
    public GameObject panelConfiguracion;
    public GameObject panelPartida;
    public GameObject panelPrivado;
    public GameObject panelClave;
    public GameObject panelLobby;
    public GameObject claveLobby;
    public GameObject panelSeleccion; // Nuevo panel de selección

    public TMP_Dropdown dropdownPersonaje; // Dropdown para seleccionar personaje
    public TMP_Dropdown dropdownMapa; // Dropdown para seleccionar mapa

    public string codigoInvitacion;

    public AudioSource musicAudioSource; // Fuente de audio para la música
    public AudioSource soundEffectsAudioSource; // Fuente de audio para los efectos de sonido
    public Toggle musicToggle; // Toggle para la música
    public Toggle soundEffectsToggle; // Toggle para los efectos de sonido

    private void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
        }
        else
        {
            Destroy(instancia);
        }
    }

    void Start()
    {
        // Este código solo se ejecuta en el cliente
        if (Application.platform == RuntimePlatform.LinuxServer) return;
        // Inicializa los toggles según el estado actual del audio
        if (musicToggle != null && musicAudioSource != null)
        {
            musicToggle.isOn = !musicAudioSource.mute;
            musicToggle.onValueChanged.AddListener(ToggleMusic);
        }

        if (soundEffectsToggle != null && soundEffectsAudioSource != null)
        {
            soundEffectsToggle.isOn = !soundEffectsAudioSource.mute;
            soundEffectsToggle.onValueChanged.AddListener(ToggleSoundEffects);
        }
    }

    // Activar/desactivar música
    public void ToggleMusic(bool isOn)
    {
        if (musicAudioSource != null)
        {
            musicAudioSource.mute = !isOn;
        }
    }

    // Activar/desactivar efectos de sonido
    public void ToggleSoundEffects(bool isOn)
    {
        if (soundEffectsAudioSource != null)
        {
            soundEffectsAudioSource.mute = !isOn;
        }
    }

    // Método para cambiar de panel y mostrar el panel de selección
    public void Comenzar()
    {
        panelInicio.SetActive(false);
        // panelSeleccion.SetActive(true); // Muestra el nuevo panel
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
        // Se avisa a los clientes de que el Host ha abandonado la sala
        ServerPlayerManager.instance.AbandonarSala();
        LobbyController.instancia.LeaveLobby();
        panelLobby.SetActive(false);
        panelPartida.SetActive(true);
    }

}
