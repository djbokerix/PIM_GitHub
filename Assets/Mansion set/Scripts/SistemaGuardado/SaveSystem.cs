using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance;

    [Header("Referencias")]
    public GameObject player;
    public SanitySystem sanity;
    public InventorySystem inventory;

    public GameData datosActuales;
    public bool juegoCargado = false;
    private string rutaArchivo;

    public List<string> listaRecogidosTemporal = new List<string>();

    public void RegistrarObjetoRecogido(string idObjeto)
    {
        if (!listaRecogidosTemporal.Contains(idObjeto)) listaRecogidosTemporal.Add(idObjeto);
    }

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
        rutaArchivo = Application.persistentDataPath + "/savegame.json";
    }

    // Buscamos referencias frescas
    void RefrescarReferencias()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        inventory = FindObjectOfType<InventorySystem>();
        if (player != null) sanity = player.GetComponent<SanitySystem>();

        if (player != null)
            Debug.Log($" SAVESYSTEM: Player detectado en {player.transform.position}");
        else
            Debug.LogError(" SAVESYSTEM: ¡NO ENCUENTRO AL PLAYER CON EL TAG 'Player'!");
    }

    void Start()
    {
        RefrescarReferencias();
        if (PlayerPrefs.GetInt("CargarAlEmpezar") == 1)
        {
            PlayerPrefs.SetInt("CargarAlEmpezar", 0);
            PlayerPrefs.Save();
            CargarJuego();
        }
    }

    public void GuardarJuego()
    {
        RefrescarReferencias();
        if (player == null) return;

        GameData data = new GameData();
        data.playerPos = player.transform.position;
        data.playerRot = player.transform.rotation;
        data.sceneName = SceneManager.GetActiveScene().name;

        if (inventory)
        {
            data.itemsInventario = inventory.ObtenerNombresInventario();
            data.tieneMapa = inventory.TieneMapa();
        }

        //Guardado de Muebles y Luces
        DrawerInteractable[] muebles = FindObjectsOfType<DrawerInteractable>();
        foreach (var m in muebles)
        {
            GameData.ObjectData obj = new GameData.ObjectData();
            obj.id = m.gameObject.name;
            obj.boolState = m.estaAbierto;
            obj.boolState2 = m.estaDesbloqueado;
            data.objetosMundo.Add(obj);
        }
        SafeZone[] luces = FindObjectsOfType<SafeZone>();
        foreach (var l in luces)
        {
            GameData.ObjectData obj = new GameData.ObjectData();
            obj.id = l.gameObject.name;
            obj.floatState = l.energiaActual;
            data.objetosMundo.Add(obj);
        }
        // ...

        if (FlashlightSystem.Instance != null)
        {
            if (FlashlightSystem.Instance.itemDataActual != null)
                data.nombreItemEnMano = FlashlightSystem.Instance.itemDataActual.itemName;

            FlashlightSystem.Instance.PrepararGuardado();
            data.energiaLinterna = FlashlightSystem.Instance.memoriaLinterna;
            data.energiaVela = FlashlightSystem.Instance.memoriaVela;
        }

        // GUARDAR ITEMS TIRADOS EN EL SUELO
        ItemPickup[] todosLosPickups = FindObjectsOfType<ItemPickup>();
        foreach (var pickup in todosLosPickups)
        {
            // Solo guardamos los que tienen la marca de haber sido tirados por el jugador
            if (pickup.fueTiradoPorJugador)
            {
                GameData.DroppedItemData dropData = new GameData.DroppedItemData();
                dropData.itemName = pickup.item.itemName;
                dropData.position = pickup.transform.position;
                dropData.rotation = pickup.transform.rotation;
                data.itemsSueltos.Add(dropData);
            }
        }
        // ------------------------------------------------

        data.objetosRecogidos = new List<string>(listaRecogidosTemporal);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(rutaArchivo, json);
        Debug.Log($" PARTIDA GUARDADA.");
    }

    public void CargarJuego()
    {
        if (!File.Exists(rutaArchivo)) return;

        RefrescarReferencias();

        string json = File.ReadAllText(rutaArchivo);
        GameData data = JsonUtility.FromJson<GameData>(json);
        datosActuales = data;
        listaRecogidosTemporal = new List<string>(data.objetosRecogidos);
        juegoCargado = true;

        StartCoroutine(MoverJugadorLento(data.playerPos, data.playerRot, data.itemsInventario, data.tieneMapa));

        if (FlashlightSystem.Instance != null)
        {
            FlashlightSystem.Instance.CargarBateriasExternas(data.energiaLinterna, data.energiaVela);
        }

        AplicarDatosMundo();

        CargarItemsSueltos(data.itemsSueltos);
    }

    // FUNCIÓN PARA RESTAURAR EL SUELO
    void CargarItemsSueltos(List<GameData.DroppedItemData> itemsSueltos)
    {
        if (itemsSueltos == null || inventory == null) return;

        foreach (var dropData in itemsSueltos)
        {
            ItemData itemMaestro = inventory.todosLosItemsDelJuego.Find(x => x.itemName == dropData.itemName);

            if (itemMaestro != null && itemMaestro.prefabModelo3D != null)
            {
                // SPAWN "DEL CIELO" para que no se buguee los objetos y atraviesen el suelo
                // Sumamos 2.0 en el eje Y para que aparezcan cayendo
                Vector3 posicionCielo = dropData.position + (Vector3.up * 2.0f);

                GameObject obj = Instantiate(itemMaestro.prefabModelo3D, posicionCielo, dropData.rotation);

                // Configuración para que no se borren
                ItemPickup pickup = obj.GetComponent<ItemPickup>();
                if (pickup == null) pickup = obj.AddComponent<ItemPickup>();

                pickup.item = itemMaestro;
                pickup.fueTiradoPorJugador = true; // Recordamos que fue tirado por ti
                pickup.esItemSoltado = true;       // Inmunidad al nacer

                // Asegurar físicas
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb == null) rb = obj.AddComponent<Rigidbody>();
                rb.isKinematic = false;
                rb.useGravity = true;

                Debug.Log($" RECUPERADO DEL SUELO: {itemMaestro.itemName} en {posicionCielo}");
            }
        }
    }

    // Aquí para poner al PLAYER
    IEnumerator MoverJugadorLento(Vector3 pos, Quaternion rot, List<string> itemsGuardados, bool tieneMapa)
    {
        yield return new WaitForSeconds(0.5f);

        if (player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.transform.position = pos;
            player.transform.rotation = rot;
            Physics.SyncTransforms();

            yield return new WaitForFixedUpdate();

            if (cc != null) cc.enabled = true;

            if (inventory != null)
            {
                // SOLTAMOS LOS OBJETOS AL SUELO
                StartCoroutine(inventory.SoltarTodoAlSuelo(itemsGuardados, tieneMapa));
            }
        }
    }

    void AplicarDatosMundo()
    {
        if (datosActuales == null) return;
        DrawerInteractable[] muebles = FindObjectsOfType<DrawerInteractable>();
        foreach (var m in muebles)
        {
            var d = datosActuales.objetosMundo.FirstOrDefault(x => x.id == m.gameObject.name);
            if (!string.IsNullOrEmpty(d.id)) m.CargarEstadoExterno(d.boolState, d.boolState2);
        }
        SafeZone[] luces = FindObjectsOfType<SafeZone>();
        foreach (var l in luces)
        {
            var d = datosActuales.objetosMundo.FirstOrDefault(x => x.id == l.gameObject.name);
            if (!string.IsNullOrEmpty(d.id)) l.CargarEstadoExterno(d.floatState);
        }
    }

    public void ClickCargarDesdeMuerte()
    {
        if (File.Exists(rutaArchivo))
        {
            Time.timeScale = 1f;
            PlayerPrefs.SetInt("CargarAlEmpezar", 1);
            PlayerPrefs.Save();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
