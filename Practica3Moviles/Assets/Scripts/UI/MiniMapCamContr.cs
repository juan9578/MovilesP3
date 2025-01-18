using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapCamContr : MonoBehaviour
{
    public Transform playerTransform;
    private const float CAM_HEIGHT = 10;
    void Start()
    {
        var controladores = FindObjectsOfType<ControladorBola>();
        foreach (var player in controladores){
            if (player.IsOwner){
                playerTransform = player.transform;
                Debug.Log("ASIGNO!");
            }
        }
    }
    private void LateUpdate() {
         if (playerTransform == null) return;

        Vector3 newPosition = playerTransform.position;
        newPosition.y = CAM_HEIGHT; // Altura fija
        transform.position = newPosition;
    }
}
