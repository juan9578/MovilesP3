using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CuentaAtras : MonoBehaviour
{
    public TMP_Text temporizador;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(RelojInicial());
    }

    private IEnumerator RelojInicial()
    {
        yield return new WaitForSeconds(1f);
        temporizador.text = "2";
        yield return new WaitForSeconds(1f);
        temporizador.text = "1";
        yield return new WaitForSeconds(1f);
        temporizador.text = "YA";
        GestorPartidas.instance.juegoComenzado = true;
        yield return new WaitForSeconds(1f);
        temporizador.text = "";
        GestorPartidas.instance.MostrarResultados();
        gameObject.SetActive(false);
    }
}
