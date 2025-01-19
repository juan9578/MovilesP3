using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapCamContr : MonoBehaviour
{
    public Transform playerTransform;
    private const float CAM_HEIGHT = 10;
    public bool jugadorAsignado = false;
    
    // Funci�n para asignar el objetivo a la c�mara
    public void AsignarObjetivo(GameObject jugador)
    {
        playerTransform = jugador.transform;
        jugadorAsignado = true;
    }
    private void LateUpdate() {
         if (playerTransform == null) return;

        Vector3 newPosition = playerTransform.position;
        newPosition.y = CAM_HEIGHT; // Altura fija
        transform.position = newPosition;
    }
}
