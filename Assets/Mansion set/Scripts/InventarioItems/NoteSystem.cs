using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NoteSystem : MonoBehaviour
{
    public static NoteSystem Instance;

    [Header("Configuración")]
    public Transform manoIzquierdaPos;
    public GameObject panelNotaUI;
    public TextMeshProUGUI textoNota;

    private GameObject notaEnMano;
    private string contenidoActual;
    private bool tieneNotaEquipada = false;
    private bool estaLeyendo = false;

    void Awake()
    {
        Instance = this;
        if (panelNotaUI != null) panelNotaUI.SetActive(false);
    }

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
        if (manoIzquierdaPos != null && manoIzquierdaPos.gameObject.activeInHierarchy) return;

        manoIzquierdaPos = null;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Transform encontrada = FindDeepChild(player.transform, "PosicionManoIzq");
            if (encontrada != null) manoIzquierdaPos = encontrada;
            else
            {
                GameObject global = GameObject.Find("PosicionManoIzq");
                if (global != null) manoIzquierdaPos = global.transform;
            }
        }
    }

    void Update()
    {
        if (tieneNotaEquipada && !string.IsNullOrEmpty(contenidoActual) && Input.GetKeyDown(KeyCode.F))
        {
            ToggleLectura();
        }
    }

    public void EquiparNota(ItemData item)
    {
        ValidarMano();
        DesequiparNota();

        if (manoIzquierdaPos == null)
        {
            Debug.LogError(" NOTE SYSTEM: ¡No encuentro la mano izquierda!");
            return;
        }

        tieneNotaEquipada = true;
        contenidoActual = item.contenidoNota;

        if (item.modeloFPS != null)
        {
            notaEnMano = Instantiate(item.modeloFPS, manoIzquierdaPos);
            notaEnMano.transform.localPosition = item.posicionFPS;
            notaEnMano.transform.localRotation = Quaternion.Euler(item.rotacionFPS);

            SetLayerRecursively(notaEnMano, manoIzquierdaPos.gameObject.layer);
        }

        if (textoNota != null) textoNota.text = contenidoActual;
    }

    public void DesequiparNota()
    {
        tieneNotaEquipada = false;
        estaLeyendo = false;
        if (notaEnMano != null) Destroy(notaEnMano);
        if (panelNotaUI != null) panelNotaUI.SetActive(false);
    }

    void ToggleLectura()
    {
        estaLeyendo = !estaLeyendo;
        if (panelNotaUI != null)
        {
            panelNotaUI.SetActive(estaLeyendo);
            if (notaEnMano != null) notaEnMano.SetActive(!estaLeyendo);
        }
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform) SetLayerRecursively(child.gameObject, newLayer);
    }
}
