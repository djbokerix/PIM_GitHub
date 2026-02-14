using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeZone : MonoBehaviour
{
    public float energiaActual = 20f;
    public float umbralParpadeo = 5f;
    public AudioSource audioSource;
    public AudioClip sonidoParpadeo;
    public AudioClip sonidoRotura;
    public Light luzComponente;

    private bool jugadorDentro = false;
    private float energiaInicial;

    void Start()
    {
        if (luzComponente == null) luzComponente = GetComponent<Light>();
        energiaInicial = 20f;
    }

    //FUNCIÓN QUE LLAMA EL SAVE SYSTEM
    public void CargarEstadoExterno(float energia)
    {
        energiaActual = energia;
        if (energiaActual <= 0) ApagarLamparaTotalmente();
    }
    // ----------------------------------------

    void Update()
    {
        if (jugadorDentro && energiaActual > 0)
        {
            energiaActual -= Time.deltaTime;
            if (energiaActual <= umbralParpadeo)
            {
                if (luzComponente != null)
                {
                    bool encendida = Random.value > 0.3f;
                    luzComponente.enabled = encendida;
                    if (!encendida && audioSource && sonidoParpadeo && !audioSource.isPlaying)
                        audioSource.PlayOneShot(sonidoParpadeo, 0.3f);
                }
            }
            if (energiaActual <= 0)
            {
                ApagarLamparaTotalmente();
                if (audioSource && sonidoRotura) audioSource.PlayOneShot(sonidoRotura);
            }
        }
        else if (energiaActual > 0 && energiaActual <= umbralParpadeo)
        {
            if (luzComponente != null) luzComponente.enabled = true;
        }
    }

    void ApagarLamparaTotalmente()
    {
        energiaActual = 0;
        if (luzComponente != null) luzComponente.enabled = false;
        if (SanitySystem.Instance != null && jugadorDentro) SanitySystem.Instance.SetSafeZone(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && energiaActual > 0)
        {
            jugadorDentro = true;
            if (SanitySystem.Instance != null) SanitySystem.Instance.SetSafeZone(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorDentro = false;
            if (SanitySystem.Instance != null) SanitySystem.Instance.SetSafeZone(false);
            if (energiaActual > 0 && luzComponente != null) luzComponente.enabled = true;
        }
    }
}
