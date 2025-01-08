using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ServerSelectionManager : NetworkBehaviour
{
    public static ServerSelectionManager instance;
    public int escenarioElegido;
    public bool opcionEnviada; // Se indica si ya se ha enviado la elección
    public Button[] botones;
    public GameObject textoEspera;
    public GameObject textoInformacion;

    // Variables para escoger el escenario en el servidor
    public List<int> escenariosElegidos;
    public int numSelecciones = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void EnviarSeleccion()
    {
        if (opcionEnviada) return;
        opcionEnviada = true;
        EnviarSeleccionServerRpc(escenarioElegido);
        // Se bloquean los botones
        for (int i = 0; i < botones.Length; i++)
        {
            botones[i].interactable = false;
            // Cambia el color para indicar que el botón está bloqueado
            ColorBlock colores = botones[i].colors;
            colores.normalColor = Color.gray; // Cambia a un color gris
            botones[i].colors = colores;
        }
        // Se muestra el texto de espera
        textoInformacion.SetActive(false);
        textoEspera.SetActive(true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void EnviarSeleccionServerRpc(int opcionElegida)
    {
        escenariosElegidos[opcionElegida]++; // Se indica que se ha elegido el escenario recibido
        numSelecciones++;
        // Si todos los jugadores han elegido ya, se busca el escenario más votado y se pasa a la escena
        if (numSelecciones == ServerPlayerManager.instance.numJugadores)
        {
            int mejorOpcion = 0;
            int mejorEscenario = 0;
            for(int i = 0; i < escenariosElegidos.Count; i++)
            {
                if (escenariosElegidos[i] > mejorOpcion)
                {
                    mejorEscenario = i;
                    mejorOpcion = escenariosElegidos[i];
                }
            }
            // Se pasa a la escena escogida
            ServerSceneLoader.Instance.CargarEscenario(mejorEscenario);
        }

    }



}
