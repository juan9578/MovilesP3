using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;

public class SceneLoader : MonoBehaviour
{
   public TMP_Dropdown mapDropdown;

    public void LoadSelectedScene()
    {
        // Obtener el índice seleccionado del Dropdown
        int selectedIndex = mapDropdown.value;
        switch (selectedIndex)
        {
            case 0: // Primera opción
                SceneManager.LoadScene("Mapa1");
                break;
            case 1: // Segunda opción
                SceneManager.LoadScene("Mapa2");
                break;
            case 2: // Tercera opción
                SceneManager.LoadScene("Mapa3");
                break;
            default:
                Debug.LogError("Opcion no valida seleccionada.");
                break;
        }
    }
}
