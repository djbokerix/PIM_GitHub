using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    [Header("Configuración")]
    public Image fadeImage;
    public CanvasGroup textGroup;
    public float fadeDuration = 2f;
    public float waitBeforeLoad = 1f;
    public string sceneToLoad;
    private Canvas myCanvas;

    private static SceneTransition instance;

    void Awake()
    {
        Debug.Log(" [TRANSICIÓN] Iniciando Awake...");

        // Si ya existe una instancia vieja de una partida anterior que se quedó "viva"
        // y estamos intentando crear una nueva para una partida nueva...
        if (instance != null && instance != this)
        {
            // En lugar de destruir ESTE (el nuevo), destruimos el VIEJO 
            // para asegurarnos de que la nueva carga sea limpia.
            Destroy(instance.gameObject);
            Debug.Log(" [TRANSICIÓN] Destruida instancia vieja para evitar conflictos.");
        }

        // Ahora nos asignamos como la instancia oficial
        instance = this;
        DontDestroyOnLoad(gameObject);

        myCanvas = GetComponent<Canvas>();
        if (myCanvas == null) myCanvas = GetComponentInChildren<Canvas>();
    }

    void Start()
    {
        // Este es el debug que dices que no sale:
        Debug.Log(" [TRANSICIÓN] Start: Iniciando Corrutina de Fade");

        if (fadeImage == null)
        {
            Debug.LogError(" [ERROR] No has arrastrado la 'Image' al script SceneTransition en el Inspector");
            return;
        }

        StartCoroutine(FadeSequence());
    }

    // SE ACTIVA SOLO: Esto detiene los errores de "MissingReference"
    private void OnDisable()
    {
        StopAllCoroutines();
        Debug.Log(" [TRANSICIÓN] OnDisable: Corrutinas detenidas.");
    }

    IEnumerator FadeSequence()
    {
        float t = 0f;

        // --- FADE IN (Hacia Negro) ---
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;

            // SEGURIDAD: Si la imagen muere, paramos
            if (fadeImage == null) yield break;

            float alpha = Mathf.Clamp01(t / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            if (textGroup != null) textGroup.alpha = alpha;
            yield return null;
        }

        if (fadeImage != null) fadeImage.color = Color.black;
        yield return new WaitForSecondsRealtime(waitBeforeLoad);

        // --- CARGA DE ESCENA ---
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("[ERROR] El nombre de la escena a cargar está vacío.");
            yield break;
        }

        Debug.Log($" [TRANSICIÓN] Cargando escena: {sceneToLoad}");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
            yield return null;

        asyncLoad.allowSceneActivation = true;

        // Esperar a que la escena nueva esté lista
        while (!asyncLoad.isDone) yield return null;

        Debug.Log("[TRANSICIÓN] Escena cargada. Iniciando Fade Out.");

        // --- FADE OUT (Hacia Transparente) ---
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;

            if (fadeImage == null) yield break;

            float alpha = 1 - Mathf.Clamp01(t / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        Debug.Log(" [TRANSICIÓN] Proceso finalizado. Destruyendo objeto.");
        Destroy(gameObject);
    }
}

