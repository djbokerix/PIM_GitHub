using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public GameObject panelPausa;   
    public SaveSystem saveSystem;   
    public Slider barraVolumen;
    public MonoBehaviour scriptPlayer;  //Tiene que ser el mouselook
    public MonoBehaviour scriptCamara;  //Tiene que ser el mouselook

    private bool isPaused = false;

    void Start()
    {
        if (panelPausa != null) panelPausa.SetActive(false);
    }

    void Update()
    {
        // Solo funciona si NO estamos muertos ni en el final
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Reanudar();
            else Pausar();
        }
    }

    public void Pausar()
    {
        isPaused = true;
        scriptPlayer.enabled = false;
        scriptCamara.enabled = false;
        panelPausa.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Reanudar()
    {
        isPaused = false;
        scriptPlayer.enabled = true;
        scriptCamara.enabled = true;
        panelPausa.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void BotonGuardar()
    {
        SaveSystem.Instance.GuardarJuego();
    }

    public void BotonMenuPrincipal()
    {
        Time.timeScale = 1f; // IMPORTANTE: Descongelar el tiempo antes de salir
        SceneManager.LoadScene("MenuPrincipal"); 
    }

    // Esta función la pondremos en el slider de volumen
    public void CambiarVolumen(float volumen)
    {
        AudioListener.volume = volumen;
    }
}
