using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GestorPartidas : MonoBehaviour
{
    public static GestorPartidas instance;

    public bool juegoComenzado = false;

    public GameObject panelResultados;
    public TMP_Text puntuacion;
    public List<TMP_Text> posiciones;

    public int puntuacionAsignada;

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

    public void MostrarResultados()
    {
        panelResultados.SetActive(true);
        puntuacionAsignada = Random.Range(0, 100);
        puntuacion.text = puntuacionAsignada.ToString();
        NivelJugador.instancia.SumarPuntos(puntuacionAsignada);
        ActualizacionRanking.instancia.EnviarPuntuacionRanking(puntuacionAsignada);
    }

}
