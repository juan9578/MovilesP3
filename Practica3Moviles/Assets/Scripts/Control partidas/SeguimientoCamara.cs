using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeguimientoCamara : MonoBehaviour
{
    public bool jugadorAsignado = false;
    public Vector3 offset;
    public GameObject objetivo;

    void LateUpdate()
    {
        if (!jugadorAsignado || objetivo == null) return;
        transform.position = objetivo.transform.position + offset;
    }

    // Funci�n para asignar el objetivo a la c�mara
    public void AsignarObjetivo(GameObject jugador)
    {
        objetivo = jugador;
        jugadorAsignado = true;
    }
}
