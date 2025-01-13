using UnityEngine;
using UnityEngine.UI;

public class ControladorBrillo : MonoBehaviour
{
    public Slider sliderBrillo; // El slider de brillo
    private Material globalMaterial; // Material global (puedes usar un shader para efectos de brillo)
    private const string clave = "Brightness"; // Clave para guardar el brillo en PlayerPrefs

    private void Start()
    {
        // Busca automáticamente el slider si no está asignado
        if (sliderBrillo == null)
        {
            sliderBrillo = GameObject.Find("BrightnessSlider").GetComponent<Slider>();
        }

        // Cargar el valor guardado o usar un valor predeterminado
        float savedBrightness = PlayerPrefs.GetFloat(clave, 1.0f); // Por defecto, el brillo es 1.0
        sliderBrillo.value = savedBrightness;

        // Configura el evento para escuchar cambios en el slider
        sliderBrillo.onValueChanged.AddListener(AdjustBrightness);

        // Configura un material global (opcional, puedes usar un Light en lugar de esto)
        globalMaterial = RenderSettings.skybox; // Ejemplo: usa el Skybox como material global

        // Ajustar el brillo inicial
        AdjustBrightness(savedBrightness);
    }

    // Ajusta el brillo según el valor del slider y lo guarda
    public void AdjustBrightness(float value)
    {
        // Cambiar el brillo global del material
        if (globalMaterial != null)
        {
            globalMaterial.SetFloat("_Exposure", value); // Cambia el brillo global del material
        }

        // Alternativa: Cambiar la intensidad de una luz ambiental
        RenderSettings.ambientIntensity = value;

        // Guardar el valor en PlayerPrefs
        PlayerPrefs.SetFloat(clave, value);
        PlayerPrefs.Save(); // Asegurarse de guardar los cambios
    }
}
