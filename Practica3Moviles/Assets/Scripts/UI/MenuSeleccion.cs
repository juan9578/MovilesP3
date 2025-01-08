using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuSeleccion : MonoBehaviour
{
    public Image escenarioActual;
    public TMP_Text descripcionEscenarioActual;
    public TMP_Text dificultadEscenarioActual;
    public Sprite[] imagenesEscenarios;
    public string[] descripcionEscenarios;
    public string[] dificultadesEscenarios;
    public int indiceEscenarioActual = 0;


    // Start is called before the first frame update
    void Start()
    {
        ActualizarImagenEscenario();
    }

    // Método para avanzar al siguiente escenario
    public void SiguienteEscenario()
    {
        indiceEscenarioActual = (indiceEscenarioActual + 1) % imagenesEscenarios.Length;
        ActualizarImagenEscenario();
    }

    // Método para retroceder al escenario anterior
    public void AnteriorEscenario()
    {
        indiceEscenarioActual--;
        if (indiceEscenarioActual < 0)
        {
            indiceEscenarioActual = imagenesEscenarios.Length - 1;
        }
        ActualizarImagenEscenario();
    }

    public void ActualizarImagenEscenario()
    {
        escenarioActual.sprite = imagenesEscenarios[indiceEscenarioActual];
        descripcionEscenarioActual.text = descripcionEscenarios[indiceEscenarioActual];
        dificultadEscenarioActual.text = dificultadesEscenarios[indiceEscenarioActual];
        ServerSelectionManager.instance.escenarioElegido = indiceEscenarioActual;
    }
}
