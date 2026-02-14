using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FlashlightSystem : MonoBehaviour
{
    public static FlashlightSystem Instance;

    [Header("Posición Mano")]
    public Transform manoPosicion;
    private GameObject objetoEnManoActual;
    public ItemData itemDataActual;

    private Light luzActual;
    private ParticleSystem fuegoActual;
    private float intensidadOriginal;

    [Header("Batería / Cera")]
    public float maxBattery = 100f;
    public float batteryDrain = 2f;

    [HideInInspector] public float memoriaLinterna;
    [HideInInspector] public float memoriaVela;

    [Header("Sonidos")]
    public AudioSource audioSource;
    public AudioClip clickLinterna;
    [Range(0f, 1f)] public float volumenClick = 0.8f;

    [Header("Interfaz (UI)")]
    public Slider batteryBar;
    public Image batteryFillImage;

    private float currentBattery;
    private bool isEquipped = false;
    private TipoObjeto currentType;

    void Awake()
    {
        Instance = this;
        memoriaLinterna = maxBattery;
        memoriaVela = maxBattery;
        currentBattery = maxBattery;
        if (batteryBar != null) batteryBar.gameObject.SetActive(false);
    }

    void Start()
    {
        // Intentamos recuperar referencias de UI si se han perdido
        if (batteryBar == null)
        {
            GameObject obj = GameObject.Find("SliderBateria");
            if (obj != null) batteryBar = obj.GetComponent<Slider>();
        }
        if (batteryFillImage == null)
        {
            GameObject obj = GameObject.Find("RellenoBateria");
            if (obj != null) batteryFillImage = obj.GetComponent<Image>();
        }
    }

    //  BUSCADOR de objetos
    private Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName) return child;
            Transform result = FindDeepChild(child, childName);
            if (result != null) return result;
        }
        return null;
    }

    public void ValidarMano()
    {
        // 1. Reseteamos referencia
        manoPosicion = null;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // 2.Buscar DENTRO DE LA CÁMARA (Para asegurar que es la mano FPS)
        Camera cam = player.GetComponentInChildren<Camera>();
        if (cam != null)
        {
            Transform t = FindDeepChild(cam.transform, "PosicionMano");
            if (t != null)
            {
                manoPosicion = t;
                return;
            }
        }

        // 3. Si falla, buscar en todo el Player
        Transform encontrada = FindDeepChild(player.transform, "PosicionMano");
        if (encontrada != null) manoPosicion = encontrada;
    }

    void Update()
    {
        // Recuperación de emergencia
        if (isEquipped && manoPosicion == null) ValidarMano();

        if (isEquipped && objetoEnManoActual != null)
        {
            // Input Linterna
            if (currentType == TipoObjeto.Linterna && Input.GetKeyDown(KeyCode.F))
            {
                if (currentBattery > 0 && luzActual != null)
                {
                    luzActual.enabled = !luzActual.enabled;
                    if (audioSource != null && clickLinterna != null)
                        audioSource.PlayOneShot(clickLinterna, volumenClick);
                }
            }

            // Consumo
            bool consumiendo = false;
            if (currentType == TipoObjeto.Linterna && luzActual != null && luzActual.enabled) consumiendo = true;
            else if (currentType == TipoObjeto.Vela && fuegoActual != null && fuegoActual.isPlaying) consumiendo = true;

            if (consumiendo)
            {
                currentBattery -= batteryDrain * Time.deltaTime;

                if (currentType == TipoObjeto.Vela && luzActual != null)
                {
                    float porcentaje = currentBattery / maxBattery;
                    luzActual.intensity = intensidadOriginal * porcentaje;
                    if (currentBattery <= 0 && fuegoActual != null) fuegoActual.Stop();
                }

                if (currentBattery <= 0)
                {
                    currentBattery = 0;
                    if (luzActual != null) luzActual.enabled = false;
                    if (fuegoActual != null) fuegoActual.Stop();
                }

                if (batteryBar != null && batteryBar.gameObject.activeSelf)
                {
                    batteryBar.value = currentBattery;
                    UpdateColor();
                }

                // Actualización en tiempo real de la memoria
                if (currentType == TipoObjeto.Linterna) memoriaLinterna = currentBattery;
                else if (currentType == TipoObjeto.Vela) memoriaVela = currentBattery;
            }
        }
    }

    // FUNCIÓN PARA EL SAVE SYSTEM
    public void PrepararGuardado()
    {
        if (isEquipped)
        {
            if (currentType == TipoObjeto.Linterna) memoriaLinterna = currentBattery;
            else if (currentType == TipoObjeto.Vela) memoriaVela = currentBattery;
        }
    }

    void UpdateColor()
    {
        if (batteryFillImage != null)
        {
            float porcentaje = currentBattery / maxBattery;
            batteryFillImage.color = Color.Lerp(Color.red, Color.green, porcentaje);
        }
    }

    public void EquipItem(ItemData item)
    {
        ValidarMano();
        BorrarModeloMano();

        if (manoPosicion == null)
        {
            Debug.LogError(" FLASHLIGHT ERROR: No encuentro 'PosicionMano'.");
            return;
        }

        // SEGURIDAD 1: Asegurar que la mano está activa
        if (!manoPosicion.gameObject.activeInHierarchy)
        {
            manoPosicion.gameObject.SetActive(true);
        }

        itemDataActual = item;

        if (item.tipo == TipoObjeto.Linterna || item.tipo == TipoObjeto.Vela || item.modeloFPS != null)
        {
            isEquipped = true;
            currentType = item.tipo;

            // Recuperar batería guardada
            if (currentType == TipoObjeto.Linterna) currentBattery = memoriaLinterna;
            else if (currentType == TipoObjeto.Vela) currentBattery = memoriaVela;

            if (item.modeloFPS != null)
            {
                objetoEnManoActual = Instantiate(item.modeloFPS, manoPosicion);

                // SEGURIDAD 2: CORRECCIÓN DE POSICIÓN ZERO (Anti-Invisible)
                // Si la posición en el ItemData es (0,0,0), la forzamos a una visible a la derecha
                if (item.posicionFPS == Vector3.zero)
                {
                    // Posición estándar: Un poco a la derecha, abajo y adelante
                    objetoEnManoActual.transform.localPosition = new Vector3(0.3f, -0.3f, 0.5f);
                    Debug.LogWarning($" FIX: El objeto '{item.itemName}' tenía posición Zero. Corrigiendo...");
                }
                else
                {
                    objetoEnManoActual.transform.localPosition = item.posicionFPS;
                }

                objetoEnManoActual.transform.localRotation = Quaternion.Euler(item.rotacionFPS);

                // SEGURIDAD 3: CAPA VISIBLE
                // Ponemos el objeto en la capa "Default" (0) para asegurar que la cámara lo ve
                SetLayerRecursively(objetoEnManoActual, 0);

                luzActual = objetoEnManoActual.GetComponentInChildren<Light>();
                fuegoActual = objetoEnManoActual.GetComponentInChildren<ParticleSystem>();
                if (luzActual != null) intensidadOriginal = luzActual.intensity;
            }

            // Gestión de UI según tipo
            if (currentType == TipoObjeto.Linterna)
            {
                if (luzActual != null) luzActual.enabled = false;
                if (batteryBar != null)
                {
                    batteryBar.gameObject.SetActive(true);
                    batteryBar.maxValue = maxBattery;
                    batteryBar.value = currentBattery;
                    UpdateColor();
                }
            }
            else if (currentType == TipoObjeto.Vela)
            {
                if (luzActual != null) luzActual.enabled = false;
                if (fuegoActual != null) fuegoActual.Stop();
                if (batteryBar != null) batteryBar.gameObject.SetActive(false);
            }
            else
            {
                // Otros objetos (Pastillas, llaves...)
                if (batteryBar != null) batteryBar.gameObject.SetActive(false);
            }
        }
        else
        {
            Unequip();
        }
    }

    public void Unequip()
    {
        isEquipped = false;
        itemDataActual = null;
        BorrarModeloMano();
        if (batteryBar != null) batteryBar.gameObject.SetActive(false);
        luzActual = null;
        fuegoActual = null;
    }

    public void RechargeBattery()
    {
        currentBattery = maxBattery;
        if (currentType == TipoObjeto.Linterna) memoriaLinterna = currentBattery;

        if (luzActual != null)
        {
            luzActual.intensity = intensidadOriginal;
            luzActual.enabled = true;
        }
        if (currentType == TipoObjeto.Vela && fuegoActual != null) fuegoActual.Play();
        if (batteryBar != null)
        {
            batteryBar.value = currentBattery;
            UpdateColor();
        }
    }

    void BorrarModeloMano()
    {
        if (objetoEnManoActual != null) Destroy(objetoEnManoActual);
    }

    public bool IsLightActive()
    {
        return isEquipped && ((luzActual != null && luzActual.enabled) || (fuegoActual != null && fuegoActual.isPlaying));
    }

    public void CargarBateriasExternas(float linterna, float vela)
    {
        memoriaLinterna = linterna;
        memoriaVela = vela;
        if (isEquipped && currentType == TipoObjeto.Linterna) currentBattery = memoriaLinterna;
        else if (isEquipped && currentType == TipoObjeto.Vela) currentBattery = memoriaVela;
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform) SetLayerRecursively(child.gameObject, newLayer);
    }
}
