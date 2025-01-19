using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.VisualScripting;

public class Helper : NetworkBehaviour
{
    private Transform playerTransform;
    public Transform goal;
    public bool objetivoAsignado = false;
    public Image proximityImage;
    public Color coldColor = Color.blue; // Color para "frío"
    public Color hotColor = Color.red; // Color para "caliente"
    public float maxDistance = 20f; // Distancia máxima para medir la proximidad

    public void AsignarObjetivo (GameObject objetivo)
    {
        objetivoAsignado = true;
        playerTransform = objetivo.transform;
    }
    void Update()
    {
        if (!objetivoAsignado || playerTransform == null) return;
        // Calcula la distancia entre la bola y la meta
        float distance = Vector3.Distance(playerTransform.position, goal.position);
        // Normaliza la distancia en un rango de 0 a 1
        float normalizedDistance = Mathf.Clamp01(distance / maxDistance);
        // Interpola entre los colores frío y caliente
        Color currentColor = Color.Lerp(hotColor, coldColor, normalizedDistance);
        // Aplica el color al componente de la UI
        proximityImage.color = currentColor;
    }
}
