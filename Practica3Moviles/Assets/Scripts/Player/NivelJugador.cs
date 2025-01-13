using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NivelJugador : MonoBehaviour
{
    public static NivelJugador instancia;

    public const int PUNTOS_NIVEL = 100;
    public int puntosActuales = 0;
    public int nivelActual = 1;

    private void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
        }
        else
        {
            Destroy(this);
        }
        DontDestroyOnLoad(gameObject);
    }

    public void SumarPuntos(int puntos)
    {
        puntosActuales += puntos;
        if (puntosActuales >= PUNTOS_NIVEL)
        {
            nivelActual++;
            puntosActuales -= PUNTOS_NIVEL;
        }

        PlayerPrefs.SetInt("Nivel", nivelActual);
        PlayerPrefs.SetInt("Puntos", puntosActuales);
        PlayerPrefs.Save();

    }

    public void CargarPuntos()
    {
        puntosActuales = PlayerPrefs.GetInt("Puntos");
        nivelActual = PlayerPrefs.GetInt("Nivel");
        if (nivelActual == 0) nivelActual = 1;
    }


}
