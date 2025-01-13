using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestorEscenas : MonoBehaviour
{
    public static GestorEscenas instance;

    public bool juegoComenzado = false;
    public int modoMultijugador = 0;

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

        DontDestroyOnLoad(gameObject);
    }


}
