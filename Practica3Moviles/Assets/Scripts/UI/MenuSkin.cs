using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuSkin : MonoBehaviour
{
    public Image skinActual;
    public TMP_Text condicionBloqueado;
    public Sprite[] imagenesSkins;
    public string[] condiciones;
    public int indiceSkinActual = 0;
    public bool skinBloqueada = false;

    public GameObject panelSkin;
    public GameObject panelLobby;
    public GameObject textoEspera;

    public Slider puntosNivel;
    public TMP_Text nivel;
    public TMP_Text nombreJugador;

    public Color colorBloqueo;

    void Start()
    {
        ActualizarImagenSkin();
        ActualizarNivel();
    }

    private void Update()
    {
        nombreJugador.text = ControladorPersonalizacion.instancia.nombreJugador;
    }

    public void ActualizarNivel()
    {
        NivelJugador.instancia.CargarPuntos();
        puntosNivel.value = NivelJugador.instancia.puntosActuales;
        nivel.text = NivelJugador.instancia.nivelActual.ToString();
    }

    // M�todo para avanzar al siguiente escenario
    public void SiguienteSkin()
    {
        ControladorEfectosSonido.instancia.SonidoClick();
        indiceSkinActual = (indiceSkinActual + 1) % imagenesSkins.Length;
        ActualizarImagenSkin();
    }

    // M�todo para retroceder al escenario anterior
    public void AnteriorSkin()
    {
        ControladorEfectosSonido.instancia.SonidoClick();
        indiceSkinActual--;
        if (indiceSkinActual < 0)
        {
            indiceSkinActual = imagenesSkins.Length - 1;
        }
        ActualizarImagenSkin();
    }

    public void ActualizarImagenSkin()
    {
        skinActual.sprite = imagenesSkins[indiceSkinActual];
        switch(indiceSkinActual)
        {
            case 4:
                if (NivelJugador.instancia.nivelActual < 5)
                {
                    skinActual.color = colorBloqueo;
                    skinBloqueada = true;
                    condicionBloqueado.text = condiciones[0];
                }
                else
                {
                    skinActual.color = Color.white;
                    skinBloqueada = false;
                    condicionBloqueado.text = "";
                }
                break;
            case 5:
                if (NivelJugador.instancia.nivelActual < 10)
                {
                    skinActual.color = colorBloqueo;
                    skinBloqueada = true;
                    condicionBloqueado.text = condiciones[1];
                }
                else
                {
                    skinActual.color = Color.white;
                    skinBloqueada = false;
                    condicionBloqueado.text = "";
                }
                break;
            case 6:
                if (NivelJugador.instancia.nivelActual < 15)
                {
                    skinActual.color = colorBloqueo;
                    skinBloqueada = true;
                    condicionBloqueado.text = condiciones[2];
                }
                else
                {
                    skinActual.color = Color.white;
                    skinBloqueada = false;
                    condicionBloqueado.text = "";
                }
                break;
            case 7:
                if (NivelJugador.instancia.nivelActual < 20)
                {
                    skinActual.color = colorBloqueo;
                    skinBloqueada = true;
                    condicionBloqueado.text = condiciones[3];
                }
                else
                {
                    skinActual.color = Color.white;
                    skinBloqueada = false;
                    condicionBloqueado.text = "";
                }
                break;
            default:
                skinActual.color = Color.white;
                skinBloqueada = false;
                condicionBloqueado.text = "";
                break;
        }
    }

    public async void ConfirmarSeleccion()
    {
        ControladorEfectosSonido.instancia.SonidoClick();

        if (skinBloqueada) return;
        textoEspera.SetActive(true);
        ControladorPersonalizacion.instancia.personajeSeleccionado = indiceSkinActual;
        switch(MenuInicial.instancia.indicePantallaAnterior)
        {
            case 0:
                await MenuInicial.instancia.BuscarPartida();
                textoEspera.SetActive(false);
                break;
            case 1:
                await MenuInicial.instancia.CrearSalaPrivada();
                textoEspera.SetActive(false);
                break;
            case 2:
                await MenuInicial.instancia.EntrarSalaPrivada();
                textoEspera.SetActive(false);
                break;
        }
    }
}
