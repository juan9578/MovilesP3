using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class ControladorPersonalizacion : MonoBehaviour
{
    public static ControladorPersonalizacion instancia;

    public string nombreJugador;
    public int personajeSeleccionado;

    private void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ModificarNombre(string nombre)
    {
        nombreJugador = nombre;
    }

    // Función que devuelve un jugador con sus datos para incluirlo en el lobby
    public Player GetPlayer()
    {
        // Este jugador se caracteriza por un nombre y un personaje seleccionado
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                // Solo almacenamos el nombre del jugador, junto con el color del coche que lleve
                { "Name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, nombreJugador) },
                { "Character", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, personajeSeleccionado.ToString()) }
            }
        };
    }


}
