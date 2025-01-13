using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ControladorMateriales : NetworkBehaviour
{
    public List<Material> skinsPersonaje;
    public Renderer playerRenderer;
    public int idCliente;

    void Start()
    {
        playerRenderer = GetComponent<Renderer>();
        idCliente = (int) OwnerClientId;

        Dictionary<string, List<string>> datosJugadores = LobbyController.instancia.GetPlayersInLobby();
        datosJugadores.TryGetValue("Characters", out List<string> skins);
        string idSkin = skins[idCliente];

        switch (idSkin)
        {
            case "0":
                playerRenderer.material = skinsPersonaje[0];
                break;
            case "1":
                playerRenderer.material = skinsPersonaje[1];
                break;
            case "2":
                playerRenderer.material = skinsPersonaje[2];
                break;
            case "3":
                playerRenderer.material = skinsPersonaje[3];
                break;
            case "4":
                playerRenderer.material = skinsPersonaje[4];
                break;
            case "5":
                playerRenderer.material = skinsPersonaje[5];
                break;
            case "6":
                playerRenderer.material = skinsPersonaje[6];
                break;
            case "7":
                playerRenderer.material = skinsPersonaje[7];
                break;

        }
    }

}
