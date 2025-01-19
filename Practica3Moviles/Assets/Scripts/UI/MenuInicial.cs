using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuInicial : MonoBehaviour
{
    public static MenuInicial instancia;

    public bool tutorialIniciado = false;

    public GameObject panelBienvenida;
    public GameObject panelTutorial;
    public GameObject panelInicio;
    public GameObject panelConfiguracion;
    public GameObject panelPartida;
    public GameObject panelRanking;
    public GameObject panelSkins;
    public GameObject panelPrivado;
    public GameObject panelClave;
    public GameObject panelLobby;
    public GameObject claveLobby;
    public GameObject panelSeleccion; // Nuevo panel de selecci�n

    public GameObject botonTutorial;
    public GameObject jugadorTutorial;
    public Vector3 posicionJugadorTutorial;

    public TMP_Dropdown dropdownPersonaje; // Dropdown para seleccionar personaje
    public TMP_Dropdown dropdownMapa; // Dropdown para seleccionar mapa

    public string codigoInvitacion;

    public Toggle musicToggle; // Toggle para la m�sica
    public Toggle soundEffectsToggle; // Toggle para los efectos de sonido

    public int indicePantallaAnterior = 0; // Indice para manejar qu� funci�n utilizar al seleccionar el personaje

    private const string MusicPrefKey = "MusicEnabled"; // Clave para guardar el estado de la m�sica
    private const string SoundEffectsPrefKey = "SoundEffectsEnabled"; // Clave para guardar el estado de los efectos de sonido

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
        if (GestorEscenas.instance.juegoComenzado)
        {
            botonTutorial.SetActive(true);
            panelLobby.SetActive(true);
            switch (GestorEscenas.instance.modoMultijugador)
            {
                case 0:
                    claveLobby.SetActive(false);
                    break;
                case 1:
                    claveLobby.SetActive(true);
                    claveLobby.GetComponent<TMP_Text>().text = "CLAVE DE INVITACI�N \n" + LobbyController.instancia.lobbyCode;
                    break;
                case 2:
                    claveLobby.SetActive(true);
                    claveLobby.GetComponent<TMP_Text>().text = "CLAVE DE INVITACI�N \n" + LobbyController.instancia.lobbyCode;
                    break;
            }
        }
        else
        {
            string nombreJugador = PlayerPrefs.GetString("Nombre_Jugador");
            if (nombreJugador == "")
            {
                panelBienvenida.SetActive(true);
            }
            else
            {
                ControladorPersonalizacion.instancia.nombreJugador = nombreJugador;
                panelInicio.SetActive(true);
                botonTutorial.SetActive(true);
            }
        }

        // Inicializa los toggles seg�n los valores guardados
        if (musicToggle != null)
        {
            bool isMusicEnabled = PlayerPrefs.GetInt(MusicPrefKey, 1) == 1; // Por defecto, la m�sica est� activada
            musicToggle.isOn = isMusicEnabled;

            musicToggle.onValueChanged.AddListener(ToggleMusic);
        }

        if (soundEffectsToggle != null)
        {
            bool areSoundEffectsEnabled = PlayerPrefs.GetInt(SoundEffectsPrefKey, 1) == 1; // Por defecto, los efectos de sonido est�n activados
            soundEffectsToggle.isOn = areSoundEffectsEnabled;

            soundEffectsToggle.onValueChanged.AddListener(ToggleSoundEffects);
        }
    }

    // Activar/desactivar m�sica
    public void ToggleMusic(bool isOn)
    {
        ControladorMusica.instancia.musicaActivada = isOn;
        // Guardar el estado de la m�sica
        PlayerPrefs.SetInt(MusicPrefKey, isOn ? 1 : 0);
        PlayerPrefs.Save();
        ControladorEfectosSonido.instancia.SonidoClick();
    }

    // Activar/desactivar efectos de sonido
    public void ToggleSoundEffects(bool isOn)
    {
        ControladorEfectosSonido.instancia.efectosActivados = isOn;
        // Guardar el estado de los efectos de sonido
        PlayerPrefs.SetInt(SoundEffectsPrefKey, isOn ? 1 : 0);
        PlayerPrefs.Save();
        ControladorEfectosSonido.instancia.SonidoClick();
    }

    // M�todo para iniciar el tutorial
    public void IniciarTutorial(bool primeraVez)
    {
        if (ControladorPersonalizacion.instancia.nombreJugador == "") return;
        tutorialIniciado = true;
        panelInicio.SetActive(false);
        panelBienvenida.SetActive(false);
        panelTutorial.SetActive(true);
        ControladorEfectosSonido.instancia.SonidoClick();
    }

    // M�todo para terminar el tutorial y pasar al menu
    public void IrMenu()
    {
        tutorialIniciado = false;
        panelInicio.SetActive(true);
        panelTutorial.SetActive(false);
        jugadorTutorial.transform.position = posicionJugadorTutorial;
        ControladorEfectosSonido.instancia.SonidoClick();
    }

    // M�todo para cambiar de panel y mostrar el panel de selecci�n
    public void Comenzar()
    {
        panelInicio.SetActive(false);
        panelPartida.SetActive(true);
        ControladorEfectosSonido.instancia.SonidoClick();
    }

    // M�todo para mostrar el men� de configuraci�n
    public void MostrarConfiguracion()
    {
        panelConfiguracion.SetActive(true);
        panelInicio.SetActive(false);
        ControladorEfectosSonido.instancia.SonidoClick();
    }

    // M�todo para salir del juego
    public void QuitGame()
    {
        ControladorEfectosSonido.instancia.SonidoClick();
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    public void MostrarRanking()
    {
        ControladorEfectosSonido.instancia.SonidoClick();
        panelPartida.SetActive(false);
        panelRanking.SetActive(true);
    }

    public void MostrarPanelSkins(int panelAnterior)
    {
        ControladorEfectosSonido.instancia.SonidoClick();
        indicePantallaAnterior = panelAnterior;
        panelPartida.SetActive(false);
        panelPrivado.SetActive(false);
        panelClave.SetActive(false);
        panelSkins.SetActive(true);
    }

    // M�todo para buscar una partida
    public async Task BuscarPartida()
    {
        ControladorEfectosSonido.instancia.SonidoClick();
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

        panelSkins.SetActive(false);
        panelLobby.SetActive(true);
        claveLobby.SetActive(false);
    }

    // M�todo para pasar al men� de invitaci�n de amigos
    public void MostrarPanelInvitacion()
    {
        ControladorEfectosSonido.instancia.SonidoClick();
        panelPartida.SetActive(false);
        panelPrivado.SetActive(true);
    }

    // M�todo para crear una sala privada
    public async Task CrearSalaPrivada()
    {
        ControladorEfectosSonido.instancia.SonidoClick();
        await LobbyController.instancia.CreatePrivateLobby();

        panelSkins.SetActive(false);
        panelLobby.SetActive(true);
        claveLobby.SetActive(true);

        // Se muestra la clave del lobby en la pantalla
        claveLobby.GetComponent<TMP_Text>().text = "CLAVE DE INVITACI�N \n" + LobbyController.instancia.lobbyCode;
    }

    // M�todo para mostrar el panel de contrase�a
    public void MostrarPanelClave()
    {
        ControladorEfectosSonido.instancia.SonidoClick();
        panelPrivado.SetActive(false);
        panelClave.SetActive(true);
    }

    // M�todo para introducir la clave de la sala
    public void IntroducirClave(string clave)
    {
        codigoInvitacion = clave;
    }

    // M�todo para unirse a una sala privada, con una contrase�a
    public async Task EntrarSalaPrivada()
    {
        ControladorEfectosSonido.instancia.SonidoClick();
        bool unidoSala = false;
        await LobbyController.instancia.JoinPrivateLobby(codigoInvitacion, (exito) => unidoSala = exito);
        if (!unidoSala)
        {
            Debug.Log("La clave introducida no es correcta");
        }
        panelSkins.SetActive(false);
        panelLobby.SetActive(true);
        claveLobby.SetActive(true);

        // Se muestra la clave del lobby en la pantalla
        claveLobby.GetComponent<TMP_Text>().text = "CLAVE DE INVITACI�N \n" + codigoInvitacion;
    }

    // M�todo para volver entre pantallas
    public void Volver()
    {
        ControladorEfectosSonido.instancia.SonidoClick();
        if (panelConfiguracion.activeSelf)
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
        else if (panelRanking.activeSelf)
        {
            panelRanking.SetActive(false);
            panelPartida.SetActive(true);
        }
        else if (panelSkins.activeSelf)
        {
            panelSkins.SetActive(false);
            panelPartida.SetActive(true);
        }
    }

    // M�todo para abandonar el lobby
    public void AbandonarSala()
    {
        ControladorEfectosSonido.instancia.SonidoClick();
        // Se avisa a los clientes de que el Host ha abandonado la sala
        ServerPlayerManager.instance.AbandonarSala();
        LobbyController.instancia.LeaveLobby();
        panelLobby.SetActive(false);
        panelPartida.SetActive(true);
    }

}
