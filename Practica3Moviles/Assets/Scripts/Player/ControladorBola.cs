using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControladorBola : MonoBehaviour
{
    public float velocidadBola = 10f; // Velocidad de movimiento
    private Rigidbody RBbola;

    void Start()
    {
        RBbola = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Leer el acelerómetro
        Vector3 tilt = Input.acceleration;
        Vector3 fuerza = new Vector3(tilt.x, 0, tilt.y);

        // Aplicar fuerza para mover la bola
        RBbola.AddForce(fuerza * velocidadBola);
    }
}
