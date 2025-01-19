using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ControladorBrillo : MonoBehaviour
{
    public Slider sliderBrillo; // El slider de brillo
    public Image brillo;
    private const string clave = "Brightness"; // Clave para guardar el brillo en PlayerPrefs


    private void Start()
    {
        // Busca automáticamente el slider si no está asignado
        if (SceneManager.GetActiveScene().name == "Inicio")
        {
            // Cargar el valor guardado o usar un valor predeterminado
            float savedBrightness = PlayerPrefs.GetFloat(clave, 1.0f); // Por defecto, el brillo es 1.0
            sliderBrillo.value = savedBrightness;
            sliderBrillo.onValueChanged.AddListener(AdjustBrightness);
            // Ajustar el brillo inicial
            AdjustBrightness(savedBrightness);

        }
        else
        {
            float savedBrightness = PlayerPrefs.GetFloat(clave, 1.0f);
            AdjustBrightness(savedBrightness);
        }
    }

    // Ajusta el brillo según el valor del slider y lo guarda
    public void AdjustBrightness(float value)
    {
        brillo.color = new Color(brillo.color.r, brillo.color.g, brillo.color.b, 1 - value);

        // Guardar el valor en PlayerPrefs
        PlayerPrefs.SetFloat(clave, value);
        PlayerPrefs.Save(); // Asegurarse de guardar los cambios
    }
}
