using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ExitCodeDoor : MonoBehaviour
{
    [Header("Configuración del Código")]
    public string codigoCorrecto = "12345";
    public GameObject panelNumericoUI;
    public TextMeshProUGUI pantallaTexto;

    [Header("Referencias del Jugador")]
    public GameObject player;
    public Image panelNegroFinal;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip sonidoInteractuar;
    public AudioClip sonidoBoton;
    public AudioClip sonidoAcierto;
    public AudioClip sonidoFallo;
    public AudioClip sonidoPuertaFinal;

    [Header("Victoria y Final")]
    public GameObject pantallaVictoria; 
    public GameObject botonCreditos;    
    public GameObject panelCreditos;    
    public GameObject botonMenuFinal;   

    private string inputActual = "";
    private bool panelAbierto = false;
    private bool juegoTerminado = false;

    void Start()
    {
        if (panelNumericoUI != null) panelNumericoUI.SetActive(false);

        // Aseguramos que todo lo del final empiece apagado
        if (pantallaVictoria != null) pantallaVictoria.SetActive(false);
        if (botonCreditos != null) botonCreditos.SetActive(false);
        if (panelCreditos != null) panelCreditos.SetActive(false);
        if (botonMenuFinal != null) botonMenuFinal.SetActive(false);

        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (panelNegroFinal != null)
        {
            panelNegroFinal.gameObject.SetActive(false);
            Color c = panelNegroFinal.color;
            c.a = 0f;
            panelNegroFinal.color = c;
        }
    }

    public void Interactuar()
    {
        if (juegoTerminado) return;

        panelAbierto = !panelAbierto;

        if (panelAbierto) AbrirPanel();
        else CerrarPanel();
    }

    void AbrirPanel()
    {
        panelNumericoUI.SetActive(true);
        inputActual = "";
        ActualizarPantalla();
        if (audioSource && sonidoInteractuar) audioSource.PlayOneShot(sonidoInteractuar);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CerrarPanel()
    {
        panelAbierto = false;
        panelNumericoUI.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void PulsarNumero(string numero)
    {
        if (juegoTerminado) return;

        if (inputActual.Length < codigoCorrecto.Length)
        {
            inputActual += numero;
            ActualizarPantalla();
            if (audioSource && sonidoBoton) audioSource.PlayOneShot(sonidoBoton);
        }

        if (inputActual.Length == codigoCorrecto.Length)
        {
            ComprobarCodigo();
        }
    }

    public void BorrarTodo()
    {
        inputActual = "";
        ActualizarPantalla();
        if (audioSource && sonidoBoton) audioSource.PlayOneShot(sonidoBoton);
    }

    void ActualizarPantalla()
    {
        if (pantallaTexto != null) pantallaTexto.text = inputActual;
    }

    void ComprobarCodigo()
    {
        if (inputActual == codigoCorrecto)
        {
            Debug.Log("¡CÓDIGO CORRECTO!");
            if (audioSource && sonidoAcierto) audioSource.PlayOneShot(sonidoAcierto);
            StartCoroutine(SecuenciaFinalPelicula());
        }
        else
        {
            Debug.Log("Código Incorrecto");
            if (audioSource && sonidoFallo) audioSource.PlayOneShot(sonidoFallo);
            inputActual = "";
            ActualizarPantalla();
        }
    }

    //LA SECUENCIA DE FINAL
    IEnumerator SecuenciaFinalPelicula()
    {
        juegoTerminado = true;

        // 1. CERRAR EL TECLADO Y BLOQUEAR AL JUGADOR
        panelNumericoUI.SetActive(false);

        if (player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
        }

        if (SanitySystem.Instance != null)
        {
            SanitySystem.Instance.enabled = false;
        }

        // 2. FUNDIDO A NEGRO
        if (panelNegroFinal != null)
        {
            panelNegroFinal.gameObject.SetActive(true);
            float alpha = 0f;

            while (alpha < 1f)
            {
                alpha += Time.deltaTime * 0.5f;
                Color c = panelNegroFinal.color;
                c.a = alpha;
                panelNegroFinal.color = c;
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(2f);
        }

        // 3. SILENCIO Y SONIDO PUERTA
        AudioSource[] todosLosAudios = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audio in todosLosAudios)
        {
            audio.Stop();
        }

        if (audioSource != null && sonidoPuertaFinal != null)
        {
            audioSource.clip = sonidoPuertaFinal;
            audioSource.volume = 1f;
            audioSource.Play();
        }

        yield return new WaitForSeconds(1f); // Pequeña pausa dramática

        // 4. MOSTRAR TEXTO DE VICTORIA
        if (pantallaVictoria != null) pantallaVictoria.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 5. ESPERAR 1 SEGUNDO Y MOSTRAR EL BOTÓN DE CRÉDITOS
        yield return new WaitForSeconds(1f);

        if (botonCreditos != null) botonCreditos.SetActive(true);
    }

    //FUNCIONES PARA LOS BOTONES

    public void ClickVerCreditos()
    {
        // Ocultamos la victoria y el botón de créditos
        if (pantallaVictoria != null) pantallaVictoria.SetActive(false);
        if (botonCreditos != null) botonCreditos.SetActive(false);

        // Mostramos el texto de los créditos
        if (panelCreditos != null) panelCreditos.SetActive(true);

        // Mostramos el botón de ir al menú
        if (botonMenuFinal != null) botonMenuFinal.SetActive(true);
    }

    public void ClickIrMenuPrincipal()
    {
        Time.timeScale = 1f; // Restaurar el tiempo por si acaso
        SceneManager.LoadScene("MenuPrincipal");
    }
}
