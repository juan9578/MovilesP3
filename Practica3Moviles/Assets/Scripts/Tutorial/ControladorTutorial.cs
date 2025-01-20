using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControladorTutorial : MonoBehaviour
{
    public float velocidadBola = 0.5f;
    private Rigidbody _rb;
    public Vector3 direccionMovimiento;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!MenuInicial.instancia.tutorialIniciado) return;
        // Gestión de los controles
        // En móvil
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
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

        _rb.AddForce(direccionMovimiento * velocidadBola);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Meta"))
        {
            MenuInicial.instancia.botonTutorial.SetActive(true);
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Laberinto"))
        {
            ControladorEfectosSonido.instancia.SonidoColision();
        }
    }
}
