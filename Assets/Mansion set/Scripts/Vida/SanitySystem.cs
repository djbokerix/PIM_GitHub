using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SanitySystem : MonoBehaviour
{
    public static SanitySystem Instance;

    [Header("Configuración Vital")]
    public float maxSanity = 100f;
    public float damageRate = 5f;
    public float healRate = 10f;

    [Header("Sonidos de Tensión")]
    public AudioSource heartbeatSource; 
    public AudioClip sonidoMuerte;      //El pitido
    public AudioClip musicaMenuMuerte;

    [Header("Interfaz")]
    public Image damagePanel;      
    public GameObject deathScreen;
    public GameObject grupoBotones;

    public float currentSanity;
    public bool isDead = false;
    private bool isInSafeZone = false;

    void Awake()
    {
        Instance = this;
        currentSanity = maxSanity;
        Time.timeScale = 1f;

        // Al empezar, nos aseguramos de que la pantalla negra está apagada
        if (deathScreen != null) deathScreen.SetActive(false);
        if (grupoBotones != null) grupoBotones.SetActive(false);
    }

    void Update()
    {
        if (isDead) return;

        // --- 1. LÓGICA DE LUZ ---
        bool isSafe = false;
        if (FlashlightSystem.Instance != null && FlashlightSystem.Instance.IsLightActive()) isSafe = true;
        else if (isInSafeZone) isSafe = true;

        // --- 2. VIDA ---
        if (isSafe) currentSanity += healRate * Time.deltaTime;
        else currentSanity -= damageRate * Time.deltaTime;

        currentSanity = Mathf.Clamp(currentSanity, 0, maxSanity);

        // --- 3. ACTUALIZAR PANEL ROJO y sonido---
        UpdateRedVignette();
        ManejarLatido();

        // --- 4. MUERTE ---
        if (currentSanity <= 0)
        {
            Die();
        }
    }

    void ManejarLatido()
    {
        if (heartbeatSource == null) return;

        // Si tenemos menos del 50% de vida
        if (currentSanity < maxSanity/2)
        {
            if (!heartbeatSource.isPlaying) heartbeatSource.Play();

            // Calcular intensidad (0 a 1) basada en cuánto nos falta para morir
            // Vida 50 -> Intensidad 0. Vida 0 -> Intensidad 1.
            float intensidad = 1f - (currentSanity / 50f);

            // VOLUMEN: Sube cuanto más cerca de morir (Max 0.8 para no ensordecer)
            heartbeatSource.volume = intensidad * 0.8f;

            // PITCH (Velocidad): Acelera el latido (De 1.0 a 1.5 de velocidad)
            heartbeatSource.pitch = 1f + (intensidad * 0.5f);
        }
        else
        {
            // Si estamos sanos, silencio
            if (heartbeatSource.isPlaying) heartbeatSource.Stop();
        }
    }

    void UpdateRedVignette()
    {
        if (damagePanel != null)
        {
            // 1. Calculamos cuánto nos falta de vida (de 0 a 1)
            float porcentajeDaño = 1f - (currentSanity / maxSanity);

            float alphaFinal = porcentajeDaño * porcentajeDaño;

            // 2. Aplicamos el color
            Color c = damagePanel.color;
            c.a = alphaFinal;
            damagePanel.color = c;
        }
    }

    void Die()
    {
        if (isDead) return; // Si ya estoy muerto, no hago nada
        isDead = true;

        // 1. BUSCAR TODOS LOS ALTAVOCES DEL JUEGO Y APAGARLOS
        AudioSource[] todosLosAudios = FindObjectsOfType<AudioSource>();

        foreach (AudioSource audio in todosLosAudios)
        {
            // Apagamos todos.
            audio.Stop();
        }

        // 2.SONIDO DE MUERTE (El pitido)
        if (heartbeatSource != null && sonidoMuerte != null)
        {
            heartbeatSource.clip = sonidoMuerte; 
            heartbeatSource.loop = false;       
            heartbeatSource.volume = 1f;         
            heartbeatSource.pitch = 1f;         
            heartbeatSource.Play();              
        }

        // 3. PANTALLA NEGRA Y UI
        if (deathScreen != null) deathScreen.SetActive(true);

        
        
        // 4. INICIAR LA SECUENCIA DE 2 SEGUNDOS
        StartCoroutine(SecuenciaMuerte());
    }

    IEnumerator SecuenciaMuerte()
    {
        // Encendemos el fondo negro, PERO mantenemos los botones apagados
        if (deathScreen != null) deathScreen.SetActive(true);
        if (grupoBotones != null) grupoBotones.SetActive(false);

        // Esperamos 2 segundos
        yield return new WaitForSeconds(4f);

        // --- AHORA SÍ: ACTIVAMOS LA INTERFAZ ---

        // 1. Mostrar botones
        if (grupoBotones != null) grupoBotones.SetActive(true);

        // 2. Poner música de ambiente de muerte 
        if (heartbeatSource != null && musicaMenuMuerte != null)
        {
            heartbeatSource.Stop(); // Paramos el pitido anterior
            heartbeatSource.clip = musicaMenuMuerte;
            heartbeatSource.loop = true; // Que se repita
            heartbeatSource.volume = 1f;
            heartbeatSource.Play();
        }

        // 3. Parar el tiempo y soltar el ratón
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


    public void TomarPastilla()
    {
        currentSanity = maxSanity;
        UpdateRedVignette();
    }

    public void SetSafeZone(bool status)
    {
        isInSafeZone = status;
    }
}
