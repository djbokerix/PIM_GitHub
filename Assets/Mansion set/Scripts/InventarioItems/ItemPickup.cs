using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ItemPickup : MonoBehaviour
{
    public ItemData item;
    public bool esItemSoltado = false;
    public bool fueTiradoPorJugador = false;

    IEnumerator Start()
    {
        // ESPERA DE SEGURIDAD
        // Esperamos al final del frame. Esto da tiempo al SaveSystem a cargar los datos
        // si coincidieran en el mismo milisegundo al iniciar la escena.
        yield return new WaitForEndOfFrame();

        // Si el objeto acaba de ser soltado por el sistema, IGNORAMOS las comprobaciones de borrado
        if (esItemSoltado) yield break; 

        if (SaveSystem.Instance != null && SaveSystem.Instance.juegoCargado)
        {
            // 1. Comprobación por ID único
            string idUnico = gameObject.name + "_" + item.itemName;

            if (SaveSystem.Instance.listaRecogidosTemporal.Contains(idUnico))
            {
                Destroy(gameObject);
                yield break;
            }
        }
    }

    public void Interact()
    {
        bool seHaGuardado = InventorySystem.Instance.Add(item);
        if (seHaGuardado)
        {
            // Solo registramos el objeto como "desaparecido para siempre" 
            // SI NO fue tirado por el jugador (es decir, si es un objeto original del mapa).
            if (SaveSystem.Instance != null && !fueTiradoPorJugador)
            {
                string idUnico = gameObject.name + "_" + item.itemName;
                SaveSystem.Instance.RegistrarObjetoRecogido(idUnico);
            }

            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Inventario lleno.");
        }
    }
}
