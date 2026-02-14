using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO; 
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    [Header("Configuración Escenas")]
    public string nombreEscenaJuego;

    [Header("Referencias UI")]
    public Slider sliderVolumen;
    public GameObject panelInstrucciones;
    public GameObject grupoClickables;

    private void Start()
    {
        // Panel instruciones desactivado al principio
        if (panelInstrucciones != null)
        {
            panelInstrucciones.SetActive(false);
        }
        float volumenGuardado = PlayerPrefs.GetFloat("VolumenJuego", 1f);
        AudioListener.volume = volumenGuardado;

        if (sliderVolumen != null)
        {
            sliderVolumen.value = volumenGuardado;
        }
    }

    public void AbrirInstrucciones()
    {
        if (panelInstrucciones != null)
        {
            if (grupoClickables != null) grupoClickables.SetActive(false);
            panelInstrucciones.SetActive(true);
        }
    }

    public void CerrarInstrucciones()
    {
        if (panelInstrucciones != null)
        {
            
            panelInstrucciones.SetActive(false);
            if (grupoClickables != null) grupoClickables.SetActive(true);
        }
    }
    public void AjustarVolumen(float volumen)
    {
        // Esto cambia el volumen GLOBAL de todo el juego (0 a 1)
        AudioListener.volume = volumen;

        // Guardamos el dato para que se mantenga al cambiar de escena
        PlayerPrefs.SetFloat("VolumenJuego", volumen);
    }

    public void BotonNuevaPartida()
    {
        
        Time.timeScale = 1f;
        string rutaArchivo = Application.persistentDataPath + "/savegame.json";

        // 1. Borramos el archivo de guardado antiguo (si existe) para empezar limpio
        if (File.Exists(rutaArchivo))
        {
            File.Delete(rutaArchivo);
            Debug.Log("Archivo de guardado anterior eliminado.");
        }

        // 2. Reseteamos la "memoria" del PlayerPrefs para que NO intente cargar nada
        PlayerPrefs.SetInt("CargarAlEmpezar", 0);
        PlayerPrefs.Save();

        // 3. Cargamos la escena del juego
        SceneManager.LoadScene("LoadingScene");
        
    }

    public void BotonCargarPartida()
    {
        string rutaArchivo = Application.persistentDataPath + "/savegame.json";

        // Solo cargamos si realmente existe un archivo
        if (File.Exists(rutaArchivo))
        {
            // 1. Le decimos al SaveSystem que cargue nada más empezar
            PlayerPrefs.SetInt("CargarAlEmpezar", 1);
            PlayerPrefs.Save();

            // 2. Cargamos la escena
            SceneManager.LoadScene("Mansion");
        }
        else
        {
            Debug.Log("¡No hay partida guardada!");
            
        }
    }

    public void BotonSalir()
    {
        Application.Quit();
        Debug.Log("Saliendo del juego...");
    }
}
