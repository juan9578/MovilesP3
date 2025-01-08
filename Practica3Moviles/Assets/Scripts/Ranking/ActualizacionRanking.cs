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

    // Una vez se termina la partida, se envía el resultado al ranking correspondiente
    public async void EnviarPuntuacionRanking(float tiempoResultado)
    {
        // Enviar la puntuación al leaderboard
        // Se utiliza el código de la escena como identificador del ranking
        string rankingId = SceneManager.GetActiveScene().name;
        await LeaderboardsService.Instance.AddPlayerScoreAsync(rankingId,
            tiempoResultado,
            new AddPlayerScoreOptions
            {
                Metadata = new Dictionary<string, string>() {
                // Se envía el nombre del jugador en la entrada del ranking
                { "Nombre", ControladorPersonalizacion.instancia.nombreJugador},
            }
            });
    }

}
