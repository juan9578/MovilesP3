using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Leaderboards;
using UnityEngine;

public class ConsultaRanking : MonoBehaviour
{
    public static ConsultaRanking instance;

    public List<List<string>> nombresRanking = new List<List<string>>(3);
    public List<List<string>> puntuacionesRanking = new List<List<string>>(3);

    public int indiceRankingActual = 0;

    public TMP_Text tituloLaberinto;
    public List<string> titulos;
    public List<TMP_Text> posicionesRanking;

    public int numRankings = 3;

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

    async void Start()
    {
        PrepararListas();
        await CargarRankings();
        ActualizarRanking();
    }

    public void PrepararListas()
    {
        for(int i = 0; i < numRankings; i++)
        {
            nombresRanking.Add(new List<string>());
            puntuacionesRanking.Add(new List<string>());
        }
    }
    public async Task CargarRankings()
    {
        // Cargar las primeras 15 mejores puntuaciones de cada leaderboard
        var puntuacionesL1 = await LeaderboardsService.Instance.GetScoresAsync("Laberinto1",
            new GetScoresOptions { Limit = 15, IncludeMetadata = true });
        var puntuacionesL2 = await LeaderboardsService.Instance.GetScoresAsync("Laberinto2",
            new GetScoresOptions { Limit = 15, IncludeMetadata = true });
        var puntuacionesL3 = await LeaderboardsService.Instance.GetScoresAsync("Laberinto3",
            new GetScoresOptions { Limit = 15, IncludeMetadata = true });

        foreach (var entry in puntuacionesL1.Results)
        {
            // Se deserializan los datos
            Dictionary<string, string> entryData = JsonConvert.DeserializeObject<Dictionary<string, string>>(entry.Metadata);
            // Obtener el nombre del jugador desde el metadata
            entryData.TryGetValue("Nombre", out string playerName);
            // Se añade el nombre a la lista
            nombresRanking[0].Add(playerName);
            // Se añade la puntuación a la lista
            puntuacionesRanking[0].Add(entry.Score.ToString());
        }

        foreach (var entry in puntuacionesL2.Results)
        {
            // Se deserializan los datos
            Dictionary<string, string> entryData = JsonConvert.DeserializeObject<Dictionary<string, string>>(entry.Metadata);
            // Obtener el nombre del jugador desde el metadata
            entryData.TryGetValue("Nombre", out string playerName);
            // Se añade el nombre a la lista
            nombresRanking[1].Add(playerName);
            // Se añade la puntuación a la lista
            puntuacionesRanking[1].Add(entry.Score.ToString());
        }

        foreach (var entry in puntuacionesL3.Results)
        {
            // Se deserializan los datos
            Dictionary<string, string> entryData = JsonConvert.DeserializeObject<Dictionary<string, string>>(entry.Metadata);
            // Obtener el nombre del jugador desde el metadata
            entryData.TryGetValue("Nombre", out string playerName);
            // Se añade el nombre a la lista
            nombresRanking[2].Add(playerName);
            // Se añade la puntuación a la lista
            puntuacionesRanking[2].Add(entry.Score.ToString());
        }
    }

    public void SiguienteRanking()
    {
        indiceRankingActual = (indiceRankingActual + 1) % numRankings;
        ActualizarRanking();
    }

    public void AnteriorRanking()
    {
        indiceRankingActual--;
        if (indiceRankingActual < 0)
        {
            indiceRankingActual = numRankings;
        }
        ActualizarRanking();
    }

    public void ActualizarRanking()
    {
        for(int i = 0; i < posicionesRanking.Count; i++)
        {
            posicionesRanking[i].text = "";
        }

        tituloLaberinto.text = titulos[indiceRankingActual];
        for(int i = 0; i < nombresRanking[indiceRankingActual].Count; i++)
        {
            posicionesRanking[i].text = (i + 1).ToString() + "º " + nombresRanking[indiceRankingActual][i] + "  " + puntuacionesRanking[indiceRankingActual][i];
        }
    }
}
