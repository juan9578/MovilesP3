using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using UnityEngine;
// El servicio de Multiplay no es compatible con WebGL y s�lo es necesario para el c�digo del servidor
#if UNITY_SERVER
using Unity.Services.Multiplay;
#endif

public class MultiplayManager : MonoBehaviour
{
    public static MultiplayManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Si se est� en el servidor, se hace que este objeto pase entre escenas, para que no se reinicie al volver al lobby
#if UNITY_SERVER
        DontDestroyOnLoad(this);
#endif
    }

#if UNITY_SERVER
    private IServerQueryHandler _serverQueryHandler; // Variable para el manejador del servidor

    private async void Start()
    {
        // Se verifica que se est� en el servidor
        if (Application.platform == RuntimePlatform.LinuxServer)
        {
            // Se ajusta el frame rate para no crashear el servidor
            Application.targetFrameRate = 60;        
            await UnityServices.InitializeAsync(); // Se inicializan los servicios de Unity
            // Se obtiene la informaci�n del servidor
            ServerConfig serverConfig = MultiplayService.Instance.ServerConfig;
            // Se configuran los datos del servidor
            _serverQueryHandler = await MultiplayService.Instance.StartServerQueryHandlerAsync(4, "TimeSpinServer", "HistoricGame", "0", "TestMap");
            // Se comprueba si el servidor est� activo
            if (serverConfig.AllocationId != string.Empty)
            {
                // Se establece la conexi�n
                UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetConnectionData("0.0.0.0", serverConfig.Port, "0.0.0.0");
                NetworkManager.Singleton.StartServer();
                // Se indica que el servidor est� listo para que los jugadores se puedan unir
                await MultiplayService.Instance.ReadyServerForPlayersAsync();
            }
        }
    }

    private async void Update()
    {
        if (NetworkManager.Singleton.IsServer && Application.platform == RuntimePlatform.LinuxServer)
        {
            if(_serverQueryHandler != null)
            {
                // Se actualiza el n�mero de jugadores del servidor
                _serverQueryHandler.CurrentPlayers = (ushort)NetworkManager.Singleton.ConnectedClients.Count;
                _serverQueryHandler.UpdateServerCheck();
                // Se retrasa la ejecuci�n para no saturar las comprobaciones
                await Task.Delay(100);
            }
        }
    }
#endif

    // Esta funci�n se utiliza para conectar a los clientes con el servidor
    public void JoinToServer(string IP, string port)
    {
        // Se establece la conexi�n
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(IP, ushort.Parse(port));
        // Se inicia el cliente
        NetworkManager.Singleton.StartClient();
    }
}
