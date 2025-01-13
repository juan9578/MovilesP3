using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class MenuLobby : MonoBehaviour
{
    public static MenuLobby instance;

    public const int MAX_JUGADORES = 4;

    // Lista con los nombres de los jugadores unidos a la sala
    [SerializeField] List<TMP_Text> nombresJugadores;

    // Objeto con el temporizador
    public GameObject temporizadorObjeto;
    // Texto del temporizador
    public TMP_Text temporizadorTexto;

    public GameObject panelSeleccion;
    public GameObject panelLobby;

    // Mensaje de error de conexión
    public GameObject mensajeDesconexion;
    // Mensaje de esperando jugadores
    public GameObject mensajeEspera;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(instance);
        }
    }

    void Update()
    {
        MostrarJugadoresLobby();
        ActualizarTemporizador();
    }

    // Esta función se encarga de actualizar los recuadros en los que se colocan los nombres de los jugadores continuamente
    private void MostrarJugadoresLobby()
    {
        // Si no se ha unido a ninguna sala, no se ejecuta nada
        if (!LobbyController.instancia.inLobby) return;
        // Para ello, se obtiene toda la información de los jugadores del lobby
        Dictionary<string, List<string>> datosJugadores = LobbyController.instancia.GetPlayersInLobby();
        datosJugadores.TryGetValue("Nombres", out List<string> nombres);
        datosJugadores.TryGetValue("Characters", out List<string> personajes);
        int numJugadores = nombres.Count;

        // Se muestra el nombre de los jugadores conectados al lobby
        for (int i = 0; i < numJugadores; i++)
        {
            nombresJugadores[i].text = nombres[i] + " : " + personajes[i];
        }
        // Se limpia el nombre de los huecos sin asignar
        for (int i = numJugadores; i < MAX_JUGADORES; i++)
        {
            nombresJugadores[i].text = "----------";
        }
    }

    // Esta función se utiliza para actualizar el temporizador de la cuenta atrás para comenzar el juego
    public void ActualizarTemporizador()
    {
        if (ServerPlayerManager.instance.temporizadorActivo)
        {
            temporizadorObjeto.SetActive(true);
            float tiempoRestante = ServerPlayerManager.instance.TIEMPO_ESPERA - ServerPlayerManager.instance.temporizador;

            int minutos = Mathf.FloorToInt(tiempoRestante / 60); // Obtener minutos
            int segundos = Mathf.FloorToInt(tiempoRestante % 60); // Obtener segundos
            temporizadorTexto.text = $"{minutos:00}:{segundos:00}"; // Formato MM:SS
        }
        else
        {
            temporizadorObjeto.SetActive(false);
        }
    }

    // Función para mostrar el panel de selección de escenario una vez haya comenzado la partida
    public void ComenzarPartida()
    {
        panelSeleccion.SetActive(true);
        panelLobby.SetActive(false);
    }
}
