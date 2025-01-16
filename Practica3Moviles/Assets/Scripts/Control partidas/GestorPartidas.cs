using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GestorPartidas : NetworkBehaviour
{
    public static GestorPartidas instance;

    public int idCliente = -1;

    public bool juegoComenzado = false;
    public bool juegoTerminado = false;

    public GameObject panelPartida;
    public GameObject panelEspera;
    public GameObject panelResultados;

    public TMP_Text puntuacion;
    public List<TMP_Text> posiciones;

    public int puntuacionAsignada;

    public TMP_Text avisoJugadorTexto;
    public GameObject avisoJugadorFondo;
    public TMP_Text temporizadorTexto;
    public float temporizador = 0;
    public TMP_Text resultadoEsperaTexto;

    public int numJugadoresMeta = 0;

    public HashSet<int> jugadoresAñadidos = new HashSet<int>();
    public int[] jugadoresLlegados = new int[4];
    public float[] tiemposJugadores = new float[4];
    public int[] puntuacionesJugadores = new int[4];
    public int[] puntuacionesFijadas;

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

    private void Update()
    {
        if (!juegoComenzado || juegoTerminado) return;

        temporizador += Time.deltaTime;
        ActualizarTemporizador();
        /*
        if (IsServer)
        {
            // Comprueba si han llegado todos los jugadores a la meta, a excepción del último
            if (numJugadoresMeta == NetworkManager.Singleton.ConnectedClients.Count - 1)
            {

            }
        }
        */

    }

    public void ActualizarTemporizador()
    {
        int minutos = Mathf.FloorToInt(temporizador / 60); // Obtener minutos
        int segundos = Mathf.FloorToInt(temporizador % 60); // Obtener segundos
        temporizadorTexto.text = $"{minutos:00}:{segundos:00}"; // Formato MM:SS
    }


    public void JugadorMeta(int clientID, float tiempo)
    {
        if (jugadoresAñadidos.Contains(clientID)) return;
        jugadoresAñadidos.Add(clientID);

        jugadoresLlegados[numJugadoresMeta] = clientID;
        tiemposJugadores[numJugadoresMeta] = tiempo;
        numJugadoresMeta++;
        // Comprueba si han llegado todos los jugadores a la meta, a excepción del último
        if (numJugadoresMeta == NetworkManager.Singleton.ConnectedClients.Count)
        {
            CalcularPuntuaciones();
            MostrarResultadosClientRpc(jugadoresLlegados, tiemposJugadores, puntuacionesJugadores);
        }
        else
        {
            MostrarEsperaClientRpc(clientID);
        }

    }

    [ClientRpc] 
    public void MostrarEsperaClientRpc(int clientID)
    {
        if (idCliente == clientID)
        {
            EsperaJugador();
        }
        else
        {
            StartCoroutine(NotificarJugadorMeta(clientID));
        }
    }

    public void EsperaJugador()
    {
        juegoTerminado = true;
        panelPartida.SetActive(false);
        panelEspera.SetActive(true);
        int minutos = Mathf.FloorToInt(temporizador / 60); // Obtener minutos
        int segundos = Mathf.FloorToInt(temporizador % 60); // Obtener segundos
        resultadoEsperaTexto.text = $"{minutos:00}:{segundos:00}"; // Formato MM:SS
    }

    public IEnumerator NotificarJugadorMeta(int clientID)
    {
        avisoJugadorFondo.SetActive(true);
        avisoJugadorTexto.text = "El jugador " + (clientID + 1).ToString() + " ha llegado a la meta";
        yield return new WaitForSeconds(3f);
        avisoJugadorTexto.text = "";
        avisoJugadorFondo.SetActive(false);
    }

    public void CalcularPuntuaciones()
    {
        int desfase = puntuacionesFijadas.Length - NetworkManager.Singleton.ConnectedClients.Count;
        int contador = 0;
        for (int i = desfase; i < puntuacionesFijadas.Length; i++)
        {
            puntuacionesJugadores[contador] = puntuacionesFijadas[i];
            contador++;
        }
    }

    [ClientRpc]
    public void MostrarResultadosClientRpc(int[] jugadores, float[] tiempos, int[] puntuaciones)
    {
        juegoTerminado = true;
        MostrarResultados(jugadores, tiempos, puntuaciones);
    }
    public void MostrarResultados(int[] jugadores, float[] tiempos, int[] puntuaciones)
    {
        panelPartida.SetActive(false);
        panelEspera.SetActive(false);
        panelResultados.SetActive(true);

        // Se obtienen los nombres de los jugadores para mostrarlos en los resultados
        Dictionary<string, List<string>> datosJugadores = LobbyController.instancia.GetPlayersInLobby();
        datosJugadores.TryGetValue("Nombres", out List<string> nombres);

        for (int i = 0; i < jugadores.Length; i++)
        {
            if (tiempos[i] == 0) break;
            int minutos = Mathf.FloorToInt(tiempos[i] / 60); 
            int segundos = Mathf.FloorToInt(tiempos[i] % 60); 
            posiciones[i].text = (i+1).ToString() + "º - " + nombres[jugadores[i]] + " -> " + $"{minutos:00}:{segundos:00}";

            if (jugadores[i] == idCliente)
            {
                puntuacionAsignada = puntuaciones[i];
            }
        }

        puntuacion.text = puntuacionAsignada.ToString();
        NivelJugador.instancia.SumarPuntos(puntuacionAsignada);
        ActualizacionRanking.instancia.EnviarPuntuacionRanking(temporizador);
    }


}
