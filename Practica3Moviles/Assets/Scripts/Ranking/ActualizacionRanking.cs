using System.Collections;
using System.Collections.Generic;
using Unity.Services.Leaderboards;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ActualizacionRanking : MonoBehaviour
{
    public static ActualizacionRanking instancia;

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

    // Una vez se termina la partida, se env�a el resultado al ranking correspondiente
    public async void EnviarPuntuacionRanking(float tiempoResultado)
    {
        // Enviar la puntuaci�n al leaderboard
        // Se utiliza el c�digo de la escena como identificador del ranking
        string rankingId = SceneManager.GetActiveScene().name;
        await LeaderboardsService.Instance.AddPlayerScoreAsync(rankingId,
            tiempoResultado,
            new AddPlayerScoreOptions
            {
                Metadata = new Dictionary<string, string>() {
                // Se env�a el nombre del jugador en la entrada del ranking
                { "Nombre", ControladorPersonalizacion.instancia.nombreJugador},
            }
            });
    }

}
