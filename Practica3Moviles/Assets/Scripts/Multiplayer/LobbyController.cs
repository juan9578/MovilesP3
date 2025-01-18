using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyController : MonoBehaviour
{
    public static LobbyController instancia;

    // Caracter�sticas del lobby
    private string _lobbyName = "Fall Balls";
    private const int MAX_PLAYERS = 4;

    // Referencias al lobby
    private Lobby _hostLobby;
    private Lobby _joinedLobby;

    // C�digo de la sala
    public string lobbyCode;
    // C�digo de relay
    public string relayCode;

    // Variable que indica si se est� en el lobby
    public bool inLobby = false;

    // Variables encargadas de hacer una pulsaci�n cada cierto tiempo, para que la sala no se destruya por inactividad
    float heartBeatLobbyTimer = 0;
    const int MAX_HEARTBEAT_TIMER = 15;

    // Variables encargadas de actualizar la referencia del lobby cada cierto tiempo
    float updateLobbyTimer = 0;
    const int MAX_UPDATE_TIMER = 2;


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

        DontDestroyOnLoad(gameObject);
    }

    async void Start()
    {
        // Se inicializan los servicios de Unity
        await UnityServices.InitializeAsync();
        // Se hace un registro an�nimo
        // Se crea un evento para comprobar que se realiza dicho registro
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void Update()
    {
        // Si estamos unidos a un lobby, cada cierto tiempo se la mandan pulsaciones
        if (_hostLobby != null)
        {
            HandleLobbyHeartbeat();
        }
        // Si se est� en un lobby, se debe actualizar cada cierto tiempo
        if (inLobby)
        {
            HandleLobbyUpdate();
        }
    }

    // Esta funci�n se utiliza para enviar un mensaje al lobby cada 15 segundos, para evitar que la sala desaparezca por inactividad
    private async void HandleLobbyHeartbeat()
    {
        if (_hostLobby != null)
        {
            heartBeatLobbyTimer += Time.deltaTime;
            // Si se supera el tiempo umbral sin enviar un mensaje, este se manda
            if (heartBeatLobbyTimer > MAX_HEARTBEAT_TIMER)
            {
                heartBeatLobbyTimer = 0;
                await LobbyService.Instance.SendHeartbeatPingAsync(_hostLobby.Id);
            }
        }
    }

    // Esta funci�n se utiliza para actualizar la referencia del lobby
    private async void HandleLobbyUpdate()
    {
        updateLobbyTimer += Time.deltaTime;
        if (updateLobbyTimer > MAX_UPDATE_TIMER)
        {
            updateLobbyTimer = 0;
            // Mediante el ID de la sala, se obtiene una referencia actualizada
            Lobby lobbyActualizado = await Lobbies.Instance.GetLobbyAsync(_joinedLobby.Id);
            _joinedLobby = lobbyActualizado;
        }
    }

    // Funci�n que devuelve si hay lobbies disponibles para que el jugador pueda unirse
    public async Task<bool> CheckLobbies()
    {
        await Task.Delay(1000);
        bool available = await IsLobbyAvailableAsync();

        if (available)
        {
            Debug.Log("Hay lobbies disponibles para unirse.");
            return true;
        }
        else
        {
            Debug.Log("No hay lobbies disponibles en este momento.");
            return false;
        }
    }

    // Funci�n que lista las salas que a�n admiten jugadores
    private async Task<bool> IsLobbyAvailableAsync()
    {
        try
        {
            // Buscar lobbies con filtro
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(
                        field: QueryFilter.FieldOptions.AvailableSlots, // Filtrar por espacios disponibles
                        op: QueryFilter.OpOptions.GT,                   // Mayor que
                        value: "0"                                      // Espacios libres
                    )
                }
            };

            QueryResponse response = await Lobbies.Instance.QueryLobbiesAsync(options);

            // Comprobar si hay lobbies disponibles
            if (response.Results.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError($"Error al buscar lobbies: {ex.Message}");
            return false;
        }
    }

    // Funci�n para crear una nueva sala p�blica, en caso de que no haya ninguna disponible
    public async Task CreatePublicLobby()
    {
        // Se definen las propiedades de la sala
        CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
        {
            IsPrivate = false,
            Player = ControladorPersonalizacion.instancia.GetPlayer()
        };

        // Se crea la sala con las caracter�sticas dadas
        _hostLobby = await LobbyService.Instance.CreateLobbyAsync(_lobbyName, MAX_PLAYERS, createLobbyOptions);
        _joinedLobby = _hostLobby;
        Debug.Log("Created Lobby! " + _hostLobby.LobbyCode);
        lobbyCode = _joinedLobby.LobbyCode;

        // Se crea el punto de Relay para que se conecten los dem�s clientes
        // Se espera a que se cree para continuar
        await RelayManager.Instance.CreateRelay(MAX_PLAYERS);
        // Una vez creado se obtiene la clave
        relayCode = RelayManager.Instance.joinCode;

        // Se guarda dicha informaci�n en la sala
        Dictionary<string, DataObject> lobbyData = new Dictionary<string, DataObject>
        {
            { "relayCode", new DataObject(DataObject.VisibilityOptions.Member, relayCode) },
        };

        try
        {
            // Actualizar el lobby de manera asincr�nica
            var updateLobbyTask = LobbyService.Instance.UpdateLobbyAsync(_hostLobby.Id, new UpdateLobbyOptions { Data = lobbyData });
            _joinedLobby = await updateLobbyTask;

            Debug.Log("Server information saved to lobby!");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error updating lobby: " + ex.Message);
            return;
        }

        inLobby = true;
    }

    // Funci�n para unirse a una sala p�blica, ya que, previamente se habr� comprobado que haya alguna disponible
    public async Task<bool> QuickJoinLobbyAsync()
    {
        try
        {
            QuickJoinLobbyOptions quickJoinLobbyOptions = new QuickJoinLobbyOptions
            {
                Player = ControladorPersonalizacion.instancia.GetPlayer()
            };

            // Llamada para unirse autom�ticamente a la primera sala p�blica disponible
            var quickJoinTask = LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
            var quickJoinResult = await quickJoinTask;

            if (quickJoinTask.Exception != null)
            {
                Debug.LogError("Error during quick join: " + quickJoinTask.Exception.Message);
                return false; // Si hay un error durante el intento de unirse
            }

            // Se ha unido correctamente a la sala
            _joinedLobby = quickJoinResult;
            Debug.Log("Successfully joined a public lobby: " + _joinedLobby.LobbyCode);
            lobbyCode = _joinedLobby.LobbyCode;
            inLobby = true;
            // Una vez se une al lobby, se obtiene el c�digo de relay para establecer la conexi�n
            relayCode = _joinedLobby.Data["relayCode"].Value;
Debug.Log("AQUI" + _joinedLobby.LobbyCode);
            // Se une al servidor
            RelayManager.Instance.JoinRelay(relayCode);
            Debug.Log("AQUI22" + _joinedLobby.LobbyCode);
            return true; // Indica que el unirse fue exitoso
        }
        catch (Exception ex)
        {
            Debug.LogError("Error in QuickJoin: " + ex.Message);
            return false; // Manejo de errores si ocurre alguna excepci�n
        }
    }

    public async Task CreatePrivateLobby()
    {
        // Se definen las propiedades de la sala
        CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
        {
            IsPrivate = true,
            Player = ControladorPersonalizacion.instancia.GetPlayer()
        };

        // Se crea la sala con las caracter�sticas dadas
        _hostLobby = await LobbyService.Instance.CreateLobbyAsync(_lobbyName, MAX_PLAYERS, createLobbyOptions);
        _joinedLobby = _hostLobby;
        Debug.Log("Created Lobby! " + _hostLobby.LobbyCode);
        lobbyCode = _joinedLobby.LobbyCode;

        // Se crea el punto de Relay para que se conecten los dem�s clientes
        // Se espera a que se cree para continuar
        await RelayManager.Instance.CreateRelay(MAX_PLAYERS);
        // Una vez creado se obtiene la clave
        relayCode = RelayManager.Instance.joinCode;

        // Se guarda dicha informaci�n en la sala
        Dictionary<string, DataObject> lobbyData = new Dictionary<string, DataObject>
        {
            { "relayCode", new DataObject(DataObject.VisibilityOptions.Member, relayCode) },
        };

        try
        {
            // Actualizar el lobby de manera asincr�nica
            var updateLobbyTask = LobbyService.Instance.UpdateLobbyAsync(_hostLobby.Id, new UpdateLobbyOptions { Data = lobbyData });
            _joinedLobby = await updateLobbyTask;

            Debug.Log("Server information saved to lobby!");
        }
        catch (Exception ex)
        {
            Debug.LogError("Error updating lobby: " + ex.Message);
            return;
        }

        inLobby = true;
    }

    public async Task<bool> JoinPrivateLobby(string lobbyCode, Action<bool> onComplete)
    {
        try
        {
            // Se crea un jugador con las caracter�sticas correspondientes
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
            {
                Player = ControladorPersonalizacion.instancia.GetPlayer()
            };

            // Intentar unirse al lobby usando el c�digo
            var joinLobbyTask = Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, options);
            var joinedLobby = await joinLobbyTask; // Espera a que se complete la operaci�n de uni�n

            // Si el lobby fue encontrado
            _joinedLobby = joinedLobby;
            lobbyCode = _joinedLobby.LobbyCode;
            Debug.Log("Joined Lobby with code: " + lobbyCode);

            // Una vez se une al lobby, se obtiene el c�digo de relay para establecer la conexi�n
            relayCode = _joinedLobby.Data["relayCode"].Value;

            // Se une al servidor
            RelayManager.Instance.JoinRelay(relayCode);

            inLobby = true;
            onComplete(true); // Indicar �xito

            return true; // Devolver verdadero si la operaci�n fue exitosa
        }
        catch (Exception ex)
        {
            // Si ocurri� un error durante el proceso
            if (ex is LobbyServiceException lobbyException)
            {
                if (lobbyException.Reason == LobbyExceptionReason.LobbyNotFound)
                {
                    Debug.Log("No se ha encontrado ninguna sala con dicho c�digo");
                }
                else if (lobbyException.Reason == LobbyExceptionReason.LobbyFull)
                {
                    Debug.Log("La sala a la que se intenta acceder est� llena");
                }
            }
            else
            {
                Debug.LogError("Error al unirse al lobby: " + ex.Message);
            }

            onComplete(false); // Indicar fallo si ocurre un error
            return false; // Devolver falso si hubo un error
        }
    }

    // Funci�n para obtener los datos de los jugadores en el lobby y poder mostrarlos
    public Dictionary<string, List<string>> GetPlayersInLobby()
    {
        // Se crea un diccionario de listas de strings, para almacenar los nombres y los personajes escogidos por los jugadores
        Dictionary<string, List<string>> datosPlayers = new Dictionary<string, List<string>>();
        // Se crea la lista de personajes y se van a�adiendo los personajes de cada uno de los jugadores
        List<string> personajes = new List<string>();
        foreach (Player p in _joinedLobby.Players)
        {
            personajes.Add(p.Data["Character"].Value);
        }
        // Se a�ade la lista al diccionario
        datosPlayers.Add("Characters", personajes);
        // Se crea la lista de nombres y se van a�adiendo los nombres de cada uno de los jugadores
        List<string> nombres = new List<string>();
        foreach (Player p in _joinedLobby.Players)
        {
            nombres.Add(p.Data["Name"].Value);
        }
        // Se a�ade la lista al diccionario
        datosPlayers.Add("Nombres", nombres);

        return datosPlayers;
    }

    // Funci�n para abandonar la sala
    public async void LeaveLobby()
    {
        await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        Debug.Log("Jugador ha abandonado la sala");
        _joinedLobby = null;
        _hostLobby = null;
        inLobby = false;
        NetworkManager.Singleton.Shutdown();
        ServerPlayerManager.instance.abandonandoSala = false;
    }

}
