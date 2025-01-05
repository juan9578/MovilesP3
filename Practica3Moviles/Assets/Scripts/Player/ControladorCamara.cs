using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControladorCamara : MonoBehaviour
{
    public Transform bola; // Referencia al transform de la bola
    public Vector3 offset; // Distancia fija entre la cámara y la bola

    void LateUpdate()
    {
        // Actualiza la posición de la cámara para que siga a la bola
        transform.position = bola.position + offset;
    }
}
