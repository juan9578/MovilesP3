using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    // Variable que indica si se está en el lobby
    public bool inLobby = false;

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
        // Este código solo se ejecuta en el cliente
        if (Application.platform == RuntimePlatform.LinuxServer) return;
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

    // Update is called once per frame
    void Update()
    {

    }

    // Función que devuelve si hay lobbies disponibles para que el jugador pueda unirse
    public async Task<bool> CheckLobbies()
    {
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
            // Player = SelectionController.instance.GetPlayer() // Descomentado si es necesario
        };

        // Se crea la sala con las características dadas
        _hostLobby = await LobbyService.Instance.CreateLobbyAsync(_lobbyName, MAX_PLAYERS, createLobbyOptions);
        _joinedLobby = _hostLobby;
        Debug.Log("Created Lobby! " + _hostLobby.LobbyCode);
        lobbyCode = _joinedLobby.LobbyCode;

        // Llama a FirstServerJoin y espera a que termine de forma asincrónica
        bool serverFound = await MatchmakerManager.Instance.FirstServerJoinAsync();

        if (!serverFound)
        {
            Debug.Log("Server not found.");
            return;
        }

        // Se recibe la IP y el puerto del servidor para establecer la conexión
        string serverIP = MatchmakerManager.Instance.GetServerIP();
        ushort serverPort = MatchmakerManager.Instance.GetServerPort();

        // Se guarda dicha información en la sala
        Dictionary<string, DataObject> lobbyData = new Dictionary<string, DataObject>
        {
            { "serverIP", new DataObject(DataObject.VisibilityOptions.Member, serverIP) },
            { "serverPort", new DataObject(DataObject.VisibilityOptions.Member, serverPort.ToString()) }
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
            // Llamada para unirse automáticamente a la primera sala pública disponible
            var quickJoinTask = LobbyService.Instance.QuickJoinLobbyAsync();
            var quickJoinResult = await quickJoinTask;

            if (quickJoinTask.Exception != null)
            {
                Debug.LogError("Error during quick join: " + quickJoinTask.Exception.Message);
                return false; // Si hay un error durante el intento de unirse
            }

            // Se ha unido correctamente a la sala
            _joinedLobby = quickJoinResult;
            Debug.Log("Successfully joined a public lobby: " + _joinedLobby.LobbyCode);

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
            // Player = SelectionController.instance.GetPlayer() // Descomentado si es necesario
        };

        // Se crea la sala con las características dadas
        _hostLobby = await LobbyService.Instance.CreateLobbyAsync(_lobbyName, MAX_PLAYERS, createLobbyOptions);
        _joinedLobby = _hostLobby;
        Debug.Log("Created Lobby! " + _hostLobby.LobbyCode);
        lobbyCode = _joinedLobby.LobbyCode;

        // Llama a FirstServerJoin y espera a que termine de forma asincrónica
        bool serverFound = await MatchmakerManager.Instance.FirstServerJoinAsync();

        if (!serverFound)
        {
            Debug.Log("Server not found.");
            return;
        }

        // Se recibe la IP y el puerto del servidor para establecer la conexión
        string serverIP = MatchmakerManager.Instance.GetServerIP();
        ushort serverPort = MatchmakerManager.Instance.GetServerPort();

        // Se guarda dicha información en la sala
        Dictionary<string, DataObject> lobbyData = new Dictionary<string, DataObject>
        {
            { "serverIP", new DataObject(DataObject.VisibilityOptions.Member, serverIP) },
            { "serverPort", new DataObject(DataObject.VisibilityOptions.Member, serverPort.ToString()) }
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
                // Player = SelectionController.instance.GetPlayer()
            };

            // Intentar unirse al lobby usando el código
            var joinLobbyTask = Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, options);
            var joinedLobby = await joinLobbyTask; // Espera a que se complete la operación de unión

            // Si el lobby fue encontrado
            _joinedLobby = joinedLobby;
            lobbyCode = _joinedLobby.LobbyCode;
            Debug.Log("Joined Lobby with code: " + lobbyCode);

            // Una vez se une al lobby, se obtienen la IP y el puerto del servidor
            string serverIP = _joinedLobby.Data["serverIP"].Value;
            string serverPort = _joinedLobby.Data["serverPort"].Value;

            // Se une al servidor
            MultiplayManager.Instance.JoinToServer(serverIP, serverPort);

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

}
