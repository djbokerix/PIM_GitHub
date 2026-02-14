using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawerInteractable : MonoBehaviour
{
    public enum TipoMueble { Cajon, Puerta }
    [Header("¿Qué es esto?")]
    public TipoMueble tipoDeObjeto;
    public float velocidad = 2f;
    public float distanciaDeslizar = 0.5f;
    public float anguloRotacion = 90f;
    public bool necesitaLlave = false;
    public string nombreDeLaLlave = "Llave";

    [Header("Textos y Sonidos")]
    public string textoCerrado = "Abrir [E]";
    public string textoAbierto = "Cerrar [E]";
    public string textoBloqueado = "Necesitas llave";
    public AudioClip sonidoAbrir;
    public AudioClip puertaBloqueada;
    public AudioSource source;

    public bool estaAbierto = false;
    public bool estaDesbloqueado = false;

    private Vector3 posCerrado;
    private Vector3 posAbierto;
    private Quaternion rotCerrada;
    private Quaternion rotAbierta;

    // VARIABLE NUEVA PARA EVITAR ERRORES
    private bool haSidoInicializado = false;

    void Start()
    {
        // Llamamos a Inicializar. Si ya se llamó desde el SaveSystem, esto no hará nada.
        Inicializar();
    }

    void Inicializar()
    {
        if (haSidoInicializado) return; // Si ya calculamos, no repetimos.

        if (tipoDeObjeto == TipoMueble.Cajon)
        {
            posCerrado = transform.localPosition;
            posAbierto = posCerrado + (transform.up * distanciaDeslizar);
        }
        else
        {
            rotCerrada = transform.localRotation;
            rotAbierta = rotCerrada * Quaternion.Euler(0, 0, anguloRotacion);
        }

        if (!necesitaLlave) estaDesbloqueado = true;

        haSidoInicializado = true; // Marcamos como listo
    }

    public void CargarEstadoExterno(bool abierto, bool desbloqueado)
    {
        Inicializar();

        estaAbierto = abierto;
        estaDesbloqueado = desbloqueado;

        if (tipoDeObjeto == TipoMueble.Cajon)
            transform.localPosition = estaAbierto ? posAbierto : posCerrado;
        else
            transform.localRotation = estaAbierto ? rotAbierta : rotCerrada;
    }

    void Update()
    {
        if (tipoDeObjeto == TipoMueble.Cajon)
        {
            Vector3 destino = estaAbierto ? posAbierto : posCerrado;
            transform.localPosition = Vector3.Lerp(transform.localPosition, destino, Time.deltaTime * velocidad);
        }
        else
        {
            Quaternion destino = estaAbierto ? rotAbierta : rotCerrada;
            transform.localRotation = Quaternion.Slerp(transform.localRotation, destino, Time.deltaTime * velocidad);
        }
    }

    public void Interact()
    {
        if (estaAbierto) estaAbierto = false;
        else
        {
            if (estaDesbloqueado)
            {
                estaAbierto = true;
                if (source && sonidoAbrir) source.PlayOneShot(sonidoAbrir);
            }
            else
            {
                if (InventorySystem.Instance.HasItem(nombreDeLaLlave))
                {
                    InventorySystem.Instance.ConsumeItem(nombreDeLaLlave);
                    estaDesbloqueado = true;
                    estaAbierto = true;
                    if (source && sonidoAbrir) source.PlayOneShot(sonidoAbrir);
                }
                else
                {
                    if (source && puertaBloqueada) source.PlayOneShot(puertaBloqueada);
                }
            }
        }
    }

    public string GetTextoInteraccion()
    {
        if (estaDesbloqueado || estaAbierto) return estaAbierto ? textoAbierto : textoCerrado;
        if (InventorySystem.Instance.HasItem(nombreDeLaLlave)) return textoCerrado;
        return textoBloqueado;
    }
}
