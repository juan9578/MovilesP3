using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ControladorBola : NetworkBehaviour
{
    public float velocidadBola = 50f; // Velocidad de movimiento
    private Rigidbody _rb;
    public Vector3 direccionMovimiento;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        if (!IsOwner) return;
        
        // Se hace que la c�mara siga y mire hacia la bola
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<SeguimientoCamara>().AsignarObjetivo(gameObject);
        GameObject.FindGameObjectWithTag("MMCam").GetComponent<MiniMapCamContr>().AsignarObjetivo(gameObject);

        // Se hace que el controlador de la partida obtenga el id del cliente
        GestorPartidas.instance.idCliente = (int)OwnerClientId;
    }

    void Update()
    {
        if (!IsOwner) return;
        if (!GestorPartidas.instance.juegoComenzado || GestorPartidas.instance.juegoTerminado) return;
        // Se resetea la direccion de movimiento en cada frame
        direccionMovimiento = Vector3.zero;
        // Gesti�n de los controles
        // En m�vil
        if (Application.platform == RuntimePlatform.Android)
        {
            Vector3 aceleracion = Input.acceleration;
            direccionMovimiento = new Vector3(aceleracion.x, 0, aceleracion.y);
        }
        /// PROVISIONAL ///
        else
        {
            // En ordenador
            if (Input.GetKey(KeyCode.W)) direccionMovimiento.z = 1f;
            if (Input.GetKey(KeyCode.S)) direccionMovimiento.z = -1f;
            if (Input.GetKey(KeyCode.A)) direccionMovimiento.x = -1f;
            if (Input.GetKey(KeyCode.D)) direccionMovimiento.x = 1f;
        }

        // Si se trata del movimiento del Host, se aplica directamente
        if (IsHost)
        {
            _rb.AddForce(direccionMovimiento * velocidadBola);
        }
        // En el caso de los clientes, se notifica al servidor
        else
        {
            if (direccionMovimiento == Vector3.zero) return; // En caso de no haber hecho ning�n input, se evita enviar el mensaje
            CambiarDireccionJugadorServerRpc(direccionMovimiento, (int)OwnerClientId);
        }

    }

    [ServerRpc(RequireOwnership = false)]
    private void CambiarDireccionJugadorServerRpc(Vector3 direccion, int playerId)
    {
        // Se comprueba primero si el jugador que se est� comprobando es del que se ha recibido el input
        if (playerId != (int)OwnerClientId) return;
        // Se modifica la direcci�n de movimiento del jugador en el servidor
        _rb.AddForce(direccion * velocidadBola);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Meta"))
        {
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.isKinematic = true;
            // Se indica que un jugador m�s ha llegado a la meta
            GestorPartidas.instance.JugadorMeta((int)OwnerClientId, GestorPartidas.instance.temporizador);
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Laberinto"))
        {
            ControladorEfectosSonido.instancia.SonidoColision();
        }
    }

}
