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

    // Características del lobby
    private string _lobbyName = "Fall Balls";
    private const int MAX_PLAYERS = 4;

    // Referencias al lobby
    private Lobby _hostLobby;
    private Lobby _joinedLobby;

    // Código de la sala
    public string lobbyCode;
    // Código de relay
    public string relayCode;

    // Variable que indica si se está en el lobby
    public bool inLobby = false;

    // Variables encargadas de hacer una pulsación cada cierto tiempo, para que la sala no se destruya por inactividad
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
        // Se hace un registro anónimo
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
        // Si se está en un lobby, se debe actualizar cada cierto tiempo
        if (inLobby)
        {
            HandleLobbyUpdate();
        }
    }

    // Esta función se utiliza para enviar un mensaje al lobby cada 15 segundos, para evitar que la sala desaparezca por inactividad
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

    // Esta función se utiliza para actualizar la referencia del lobby
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

    // Función que devuelve si hay lobbies disponibles para que el jugador pueda unirse
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

    // Función que lista las salas que aún admiten jugadores
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

    // Función para crear una nueva sala pública, en caso de que no haya ninguna disponible
    public async Task CreatePublicLobby()
    {
        // Se definen las propiedades de la sala
        CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
        {
            IsPrivate = false,
            Player = ControladorPersonalizacion.instancia.GetPlayer()
        };

        // Se crea la sala con las características dadas
        _hostLobby = await LobbyService.Instance.CreateLobbyAsync(_lobbyName, MAX_PLAYERS, createLobbyOptions);
        _joinedLobby = _hostLobby;
        Debug.Log("Created Lobby! " + _hostLobby.LobbyCode);
        lobbyCode = _joinedLobby.LobbyCode;

        // Se crea el punto de Relay para que se conecten los demás clientes
        // Se espera a que se cree para continuar
        await RelayManager.Instance.CreateRelay(MAX_PLAYERS);
        // Una vez creado se obtiene la clave
        relayCode = RelayManager.Instance.joinCode;

        // Se guarda dicha información en la sala
        Dictionary<string, DataObject> lobbyData = new Dictionary<string, DataObject>
        {
            { "relayCode", new DataObject(DataObject.VisibilityOptions.Member, relayCode) },
        };

        try
        {
            // Actualizar el lobby de manera asincrónica
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

    // Función para unirse a una sala pública, ya que, previamente se habrá comprobado que haya alguna disponible
    public async Task<bool> QuickJoinLobbyAsync()
    {
        try
        {
            QuickJoinLobbyOptions quickJoinLobbyOptions = new QuickJoinLobbyOptions
            {
                Player = ControladorPersonalizacion.instancia.GetPlayer()
            };

            // Llamada para unirse automáticamente a la primera sala pública disponible
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
            inLobby = true;
            // Una vez se une al lobby, se obtiene el código de relay para establecer la conexión
            relayCode = _joinedLobby.Data["relayCode"].Value;

            // Se une al servidor
            RelayManager.Instance.JoinRelay(relayCode);
            return true; // Indica que el unirse fue exitoso
        }
        catch (Exception ex)
        {
            Debug.LogError("Error in QuickJoin: " + ex.Message);
            return false; // Manejo de errores si ocurre alguna excepción
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

        // Se crea la sala con las características dadas
        _hostLobby = await LobbyService.Instance.CreateLobbyAsync(_lobbyName, MAX_PLAYERS, createLobbyOptions);
        _joinedLobby = _hostLobby;
        Debug.Log("Created Lobby! " + _hostLobby.LobbyCode);
        lobbyCode = _joinedLobby.LobbyCode;

        // Se crea el punto de Relay para que se conecten los demás clientes
        // Se espera a que se cree para continuar
        await RelayManager.Instance.CreateRelay(MAX_PLAYERS);
        // Una vez creado se obtiene la clave
        relayCode = RelayManager.Instance.joinCode;

        // Se guarda dicha información en la sala
        Dictionary<string, DataObject> lobbyData = new Dictionary<string, DataObject>
        {
            { "relayCode", new DataObject(DataObject.VisibilityOptions.Member, relayCode) },
        };

        try
        {
            // Actualizar el lobby de manera asincrónica
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
            // Se crea un jugador con las características correspondientes
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
            {
                Player = ControladorPersonalizacion.instancia.GetPlayer()
            };

            // Intentar unirse al lobby usando el código
            var joinLobbyTask = Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, options);
            var joinedLobby = await joinLobbyTask; // Espera a que se complete la operación de unión

            // Si el lobby fue encontrado
            _joinedLobby = joinedLobby;
            lobbyCode = _joinedLobby.LobbyCode;
            Debug.Log("Joined Lobby with code: " + lobbyCode);

            // Una vez se une al lobby, se obtiene el código de relay para establecer la conexión
            relayCode = _joinedLobby.Data["relayCode"].Value;

            // Se une al servidor
            RelayManager.Instance.JoinRelay(relayCode);

            inLobby = true;
            onComplete(true); // Indicar éxito

            return true; // Devolver verdadero si la operación fue exitosa
        }
        catch (Exception ex)
        {
            // Si ocurrió un error durante el proceso
            if (ex is LobbyServiceException lobbyException)
            {
                if (lobbyException.Reason == LobbyExceptionReason.LobbyNotFound)
                {
                    Debug.Log("No se ha encontrado ninguna sala con dicho código");
                }
                else if (lobbyException.Reason == LobbyExceptionReason.LobbyFull)
                {
                    Debug.Log("La sala a la que se intenta acceder está llena");
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

    // Función para obtener los datos de los jugadores en el lobby y poder mostrarlos
    public Dictionary<string, List<string>> GetPlayersInLobby()
    {
        // Se crea un diccionario de listas de strings, para almacenar los nombres y los personajes escogidos por los jugadores
        Dictionary<string, List<string>> datosPlayers = new Dictionary<string, List<string>>();
        // Se crea la lista de personajes y se van añadiendo los personajes de cada uno de los jugadores
        List<string> personajes = new List<string>();
        foreach (Player p in _joinedLobby.Players)
        {
            personajes.Add(p.Data["Character"].Value);
        }
        // Se añade la lista al diccionario
        datosPlayers.Add("Characters", personajes);
        // Se crea la lista de nombres y se van añadiendo los nombres de cada uno de los jugadores
        List<string> nombres = new List<string>();
        foreach (Player p in _joinedLobby.Players)
        {
            nombres.Add(p.Data["Name"].Value);
        }
        // Se añade la lista al diccionario
        datosPlayers.Add("Nombres", nombres);

        return datosPlayers;
    }

    // Función para abandonar la sala
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
