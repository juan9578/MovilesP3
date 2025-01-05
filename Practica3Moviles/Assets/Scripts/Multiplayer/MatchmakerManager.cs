using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public class MatchmakerManager : NetworkBehaviour
{
    public static MatchmakerManager Instance;

    private string _currentTicket;
    private string _serverIP;
    private ushort _serverPort;

    private bool _isDeallocating = false;
    private bool _deallocatingCancellationToken = false;

    public string GetServerIP() { return _serverIP; }
    public ushort GetServerPort() { return _serverPort; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Update()
    {
        // Este c�digo solo se ejecuta en el servidor, ya que es el encargado de desasignarlo
        if (NetworkManager.Singleton.IsServer && Application.platform == RuntimePlatform.LinuxServer)
        {
            // Si no hay ning�n cliente conectado se detiene el servidor
            if (NetworkManager.Singleton.ConnectedClients.Count == 0 && !_isDeallocating)
            {
                _isDeallocating = true;
                _deallocatingCancellationToken = false;
                DeallocateServer();
            }
            if (NetworkManager.Singleton.ConnectedClients.Count != 0)
            {
                _isDeallocating = false;
                _deallocatingCancellationToken = true;
            }

        }
    }

    private void OnApplicationQuit()
    {
        // Verifica si el NetworkManager es el servidor y est� ejecut�ndose en Linux
        if (NetworkManager.Singleton.IsServer && Application.platform == RuntimePlatform.LinuxServer)
        {
            // Verifica si hay clientes conectados
            if (NetworkManager.Singleton.ConnectedClients.Count > 0)
            {
                // Desconecta a todos los clientes
                foreach (var client in NetworkManager.Singleton.ConnectedClients)
                {
                    NetworkManager.Singleton.DisconnectClient(client.Key);
                }
            }
            // Finaliza el NetworkManager
            NetworkManager.Singleton.Shutdown();
        }
    }

    public async Task<bool> FirstServerJoinAsync()
    {
        // Este proceso se ejecuta hasta que se encuentra un servidor. En caso de timeout, se crea un ticket nuevo, hasta que se encuentra el servidor
        while (true)
        {
            // Se configura la petici�n del ticket
            CreateTicketOptions createTicketOptions = new CreateTicketOptions("JoinServerQueue");
            List<Player> players = new List<Player> { new Player(AuthenticationService.Instance.PlayerId) };

            try
            {
                var createTicketTask = MatchmakerService.Instance.CreateTicketAsync(players, createTicketOptions);
                var createTicketResult = await createTicketTask;

                _currentTicket = createTicketResult.Id;
                Debug.Log("Ticket created: " + _currentTicket);

                // Despu�s, se comprueba el estado del ticket
                bool serverFound = false;

                while (!serverFound)
                {
                    var getTicketTask = MatchmakerService.Instance.GetTicketAsync(_currentTicket);
                    var ticketStatusResponse = await getTicketTask;

                    if (ticketStatusResponse.Type == typeof(MultiplayAssignment))
                    {
                        MultiplayAssignment multiplayAssignment = (MultiplayAssignment)ticketStatusResponse.Value;

                        if (multiplayAssignment.Status == MultiplayAssignment.StatusOptions.Found)
                        {
                            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                            _serverIP = multiplayAssignment.Ip;
                            _serverPort = ushort.Parse(multiplayAssignment.Port.ToString());
                            transport.SetConnectionData(_serverIP, _serverPort);
                            // transport.SetClientSecrets(SecureParameters.ServerCommonName, SecureParameters.MyGameClientCA); // Credenciales de seguridad de la red
                            NetworkManager.Singleton.StartClient();
                            Debug.Log("Server found");
                            return true; // Servidor encontrado, salir de la funci�n
                        }
                        else if (multiplayAssignment.Status == MultiplayAssignment.StatusOptions.Timeout)
                        {
                            Debug.Log("Match timeout, retrying...");
                            break; // Salir del bucle interno y recrear el ticket
                        }
                        else if (multiplayAssignment.Status == MultiplayAssignment.StatusOptions.Failed)
                        {
                            Debug.Log("Match failed: " + multiplayAssignment.Status + "  " + multiplayAssignment.Message);
                            return false; // En caso de fallo, se sale de la funci�n
                        }
                        else if (multiplayAssignment.Status == MultiplayAssignment.StatusOptions.InProgress)
                        {
                            Debug.Log("Match in progress");
                        }
                    }

                    // Espera antes de reintentar el estado del ticket
                    await Task.Delay(1000); // Espera 1 segundo antes de la siguiente comprobaci�n
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Error during ticket creation or checking: " + ex.Message);
                return false; // Si ocurre un error, se sale de la funci�n
            }

            // Espera antes de reintentar la creaci�n del ticket
            await Task.Delay(1000); // Espera 1 segundo antes de volver a intentar crear el ticket
        }
    }

    // Funci�n para desasignar un servidor
    private async void DeallocateServer()
    {
        await Task.Delay(60 * 1000);

        if (!_deallocatingCancellationToken)
        {
            Application.Quit();
        }
    }

}
