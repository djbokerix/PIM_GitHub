using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance;

    [Header("UI General")]
    public GameObject inventoryUI;
    public Transform itemsContainer;
    public GameObject itemSlotPrefab;
    [Header("Avisos UI")]
    public TextMeshProUGUI fullInventoryText;

    [Header("Mapa")]
    public Image mapSlotImage;
    public TextMeshProUGUI mapText;
    public bool poseemosMapa = false;

    [Header("Audio")]
    public AudioSource audioSourcePlayer;
    public AudioClip sonidoComerPastilla;
    public AudioClip sonidoPapelNota;
    public AudioClip sonidoCogerItemGenerico;
    public AudioClip encenderVela;

    [Header("Lógica de Juego")]
    public Transform dropPoint;

    // LISTA MAESTRA
    public List<ItemData> todosLosItemsDelJuego;

    public List<ItemData> itemsData = new List<ItemData>();
    public List<GameObject> slotObjects = new List<GameObject>();
    private int selectedIndex = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    void Start()
    {
        if (fullInventoryText != null) fullInventoryText.gameObject.SetActive(false);
        if (mapSlotImage != null)
        {
            mapSlotImage.color = new Color(1, 1, 1, 0);
            mapSlotImage.sprite = null;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) inventoryUI.SetActive(!inventoryUI.activeSelf);

        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectSlot(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SelectSlot(4);

        if (Input.GetKeyDown(KeyCode.G)) DropSelectedItem();
        if (Input.GetKeyDown(KeyCode.R)) HandleInteractionR();
    }

    void HandleInteractionR()
    {
        if (selectedIndex == -1 || selectedIndex >= itemsData.Count) return;
        ItemData currentItem = itemsData[selectedIndex];

        // Lógica de consumo con R
        if (currentItem.tipo == TipoObjeto.Pastillas)
        {
            if (SanitySystem.Instance != null)
            {
                SanitySystem.Instance.TomarPastilla();
                if (audioSourcePlayer && sonidoComerPastilla) audioSourcePlayer.PlayOneShot(sonidoComerPastilla, 1f);
                if (NoteSystem.Instance != null) NoteSystem.Instance.DesequiparNota();
                RemoveItem(currentItem);
            }
        }
        else if (currentItem.tipo == TipoObjeto.Linterna)
        {
            ItemData pila = itemsData.Find(x => x.tipo == TipoObjeto.Bateria);
            if (pila != null) { RemoveItem(pila); FlashlightSystem.Instance.RechargeBattery(); }
        }
        else if (currentItem.tipo == TipoObjeto.Vela)
        {
            ItemData mechero = itemsData.Find(x => x.tipo == TipoObjeto.Mechero);
            RemoveItem(mechero);
            audioSourcePlayer.PlayOneShot(encenderVela, 1f);
            if (mechero != null) FlashlightSystem.Instance.RechargeBattery();
        }
    }

    // CORRUTINA: SOLTAR TODO
    public IEnumerator SoltarTodoAlSuelo(List<string> nombresGuardados, bool teniaMapa)
    {
        itemsData.Clear();
        foreach (GameObject slot in slotObjects) Destroy(slot);
        slotObjects.Clear();
        selectedIndex = -1;

        if (FlashlightSystem.Instance != null) FlashlightSystem.Instance.Unequip();
        if (NoteSystem.Instance != null) NoteSystem.Instance.DesequiparNota();

        // Mapa
        poseemosMapa = teniaMapa;
        if (poseemosMapa && mapSlotImage != null)
        {
            ItemData mapaData = todosLosItemsDelJuego.Find(x => x.itemName == "Mapa");
            if (mapaData)
            {
                mapSlotImage.sprite = mapaData.icon;
                mapSlotImage.color = Color.white;
                if (mapText != null) mapText.gameObject.SetActive(false);
            }
        }
        else if (mapSlotImage != null) mapSlotImage.color = new Color(1, 1, 1, 0);

        // Lluvia de objetos
        Transform player = GameObject.FindGameObjectWithTag("Player").transform;
        Vector3 centroFrente = player.position + (player.forward * 1.5f) + (Vector3.up * 1.5f);
        Vector3 derecha = player.right * 0.5f;
        Vector3 inicioLinea = centroFrente - (derecha * (nombresGuardados.Count / 2.0f));

        int contador = 0;
        foreach (string nombre in nombresGuardados)
        {
            ItemData itemMaestro = todosLosItemsDelJuego.Find(x => x.itemName == nombre);
            if (itemMaestro != null)
            {
                GameObject modeloAUsar = itemMaestro.prefabModelo3D;
                if (modeloAUsar == null) modeloAUsar = itemMaestro.modeloFPS;

                if (modeloAUsar != null)
                {
                    Vector3 posFinal = inicioLinea + (derecha * contador);
                    GameObject drop = Instantiate(modeloAUsar, posFinal, Quaternion.identity);
                    drop.name = "RECOVERY_" + itemMaestro.itemName;

                    ItemPickup pickup = drop.GetComponent<ItemPickup>();
                    if (pickup == null) pickup = drop.AddComponent<ItemPickup>();
                    pickup.item = itemMaestro;
                    pickup.esItemSoltado = true; // INMUNIDAD

                    Rigidbody rb = drop.GetComponent<Rigidbody>();
                    if (rb == null) rb = drop.AddComponent<Rigidbody>();
                    rb.isKinematic = false;
                    rb.useGravity = true;

                    if (drop.GetComponent<Collider>() == null) drop.AddComponent<BoxCollider>();
                    contador++;
                }
                else
                {
                    Add(itemMaestro);
                }
            }
        }
        yield return null;
    }

    public List<string> ObtenerNombresInventario()
    {
        List<string> nombres = new List<string>();
        foreach (var item in itemsData) nombres.Add(item.itemName);
        return nombres;
    }

    public bool TieneMapa() { return poseemosMapa; }

    public bool Add(ItemData item)
    {
        if (item.tipo == TipoObjeto.Linterna || item.tipo == TipoObjeto.Vela)
        {
            // 1. Buscamos si ya tenemos una en el inventario para no spamear asi linternas y velas
            bool yaTengoUno = itemsData.Exists(x => x.tipo == item.tipo);

            if (yaTengoUno)
            {
                Debug.Log("¡Ya tienes este objeto equipado! Buscando pilas...");
                StartCoroutine(ShowFullInventoryText()); // Reusamos tu texto de aviso
                if (fullInventoryText != null)
                {
                    fullInventoryText.text = "¡Ya tienes una!";
                }

                return false; // IMPORTANTE: Devuelve false para que NO se añada al inventario
            }
        }

        if (item.tipo == TipoObjeto.Normal && item.itemName == "Mapa")
        {
            poseemosMapa = true;
            if (mapSlotImage != null)
            {
                mapSlotImage.sprite = item.icon;
                mapSlotImage.color = Color.white;
            }
            if (mapText != null) mapText.gameObject.SetActive(false);
            if (audioSourcePlayer && sonidoPapelNota) audioSourcePlayer.PlayOneShot(sonidoPapelNota);
            return true;
        }

        if (itemsData.Count >= 5)
        {
            StartCoroutine(ShowFullInventoryText());
            return false;
        }

        itemsData.Add(item);
        GameObject slot = Instantiate(itemSlotPrefab, itemsContainer);
        Image iconImage = slot.GetComponentInChildren<Image>();
        if (iconImage != null) iconImage.sprite = item.icon;
        slotObjects.Add(slot);

        //ESTO ES LO QUE HACE QUE APAREZCA EN LA MANO AL COGERLO 
        if (selectedIndex == -1)
        {
            SelectSlot(itemsData.Count - 1);
        }
        return true;
    }

    IEnumerator ShowFullInventoryText()
    {
        if (fullInventoryText != null)
        {
            fullInventoryText.gameObject.SetActive(true);
            fullInventoryText.text = "¡Inventario Lleno!";
            yield return new WaitForSeconds(2f);
            fullInventoryText.gameObject.SetActive(false);
        }
    }

    public void SelectSlot(int index)
    {
        if (index >= itemsData.Count || index < 0) return;
        selectedIndex = index;

        // Actualizamos colores de la UI para cuando seleccionemos el objeto
        for (int i = 0; i < slotObjects.Count; i++)
        {
            Image bg = slotObjects[i].GetComponent<Image>();
            if (i == selectedIndex) bg.color = Color.green;
            else bg.color = Color.white;
        }

        ItemData itemASeleccionar = itemsData[selectedIndex];

        //CORRECCIÓN: EVITAR REINICIO
        // Si ya tenemos equipado ESTE MISMO objeto, nos salimos y no tocamos nada.
        // Así la luz no se apaga.
        if (FlashlightSystem.Instance != null && FlashlightSystem.Instance.itemDataActual == itemASeleccionar)
        {
            return;
        }
        

        if (FlashlightSystem.Instance != null) FlashlightSystem.Instance.Unequip();
        if (NoteSystem.Instance != null) NoteSystem.Instance.DesequiparNota();

        // Lógica de equipar (Mano Izquierda o Derecha)
        if (itemASeleccionar.tipo == TipoObjeto.Nota || itemASeleccionar.tipo == TipoObjeto.Pastillas)
        {
            if (NoteSystem.Instance != null) NoteSystem.Instance.EquiparNota(itemASeleccionar);
            if (itemASeleccionar.tipo == TipoObjeto.Nota && audioSourcePlayer && sonidoPapelNota)
                audioSourcePlayer.PlayOneShot(sonidoPapelNota);
        }
        else
        {
            if (FlashlightSystem.Instance != null) FlashlightSystem.Instance.EquipItem(itemASeleccionar);
        }
    }

    void DropSelectedItem()
    {
        if (selectedIndex == -1 || itemsData.Count == 0) return;
        ItemData itemToDrop = itemsData[selectedIndex];
        if (itemToDrop.prefabModelo3D != null)
        {
            GameObject drop = Instantiate(itemToDrop.prefabModelo3D, dropPoint.position, dropPoint.rotation);
            Rigidbody rb = drop.GetComponent<Rigidbody>();
            if (rb == null) rb = drop.AddComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.useGravity = true;

            ItemPickup pickupScript = drop.GetComponent<ItemPickup>();
            if (pickupScript != null)
            {
                pickupScript.item = itemToDrop;
                pickupScript.esItemSoltado = false;
                pickupScript.fueTiradoPorJugador = true;
            }
        }

        Destroy(slotObjects[selectedIndex]);
        slotObjects.RemoveAt(selectedIndex);
        itemsData.RemoveAt(selectedIndex);

        if (FlashlightSystem.Instance != null) FlashlightSystem.Instance.Unequip();
        if (NoteSystem.Instance != null) NoteSystem.Instance.DesequiparNota();

        selectedIndex = -1;
        if (itemsData.Count > 0) SelectSlot(0);
    }

    void TryReload() { }

    public void RemoveItem(ItemData itemToRemove)
    {
        int index = itemsData.IndexOf(itemToRemove);
        if (index != -1)
        {
            Destroy(slotObjects[index]);
            slotObjects.RemoveAt(index);
            itemsData.RemoveAt(index);
            if (selectedIndex >= itemsData.Count) selectedIndex = itemsData.Count - 1;
            SelectSlot(selectedIndex);
        }
    }

    public bool HasItem(string itemName)
    {
        return itemsData.Exists(x => x.itemName == itemName);
    }

    public void ConsumeItem(string itemName)
    {
        ItemData itemToDelete = itemsData.Find(x => x.itemName == itemName);
        if (itemToDelete != null) RemoveItem(itemToDelete);
    }
}
