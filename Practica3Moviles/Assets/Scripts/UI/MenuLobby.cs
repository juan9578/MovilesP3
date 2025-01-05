using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuLobby : MonoBehaviour
{
    public const int MAX_JUGADORES = 4;
    public int NUM_JUGADORES;
    // Lista con los nombres de los jugadores unidos a la sala
    [SerializeField] List<TMP_Text> nombresJugadores;

    void Update()
    {
        MostrarJugadoresLobby();
    }

    // Esta función se encarga de actualizar los recuadros en los que se colocan los nombres de los jugadores continuamente
    private void MostrarJugadoresLobby()
    {
        // Si no se ha unido a ninguna sala, no se ejecuta nada
        if (!LobbyController.instancia.inLobby) return;
        // Para ello, se obtiene toda la información de los jugadores del lobby
        Dictionary<string, List<string>> datosJugadores = LobbyController.instancia.GetPlayersInLobby();
        datosJugadores.TryGetValue("Nombres", out List<string> nombres);

        NUM_JUGADORES = nombres.Count;
        // Se muestra el nombre de los jugadores conectados al lobby
        for (int i = 0; i < NUM_JUGADORES; i++)
        {
            nombresJugadores[i].text = nombres[i];
        }
        // Se limpia el nombre de los huecos sin asignar
        for (int i = NUM_JUGADORES; i < MAX_JUGADORES; i++)
        {
            nombresJugadores[i].text = "----------";
        }

    }
}
