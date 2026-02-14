using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Configuración")]
    public float interactionDistance = 3f; // Distancia máxima para alcanzar objetos
    public TextMeshProUGUI interactionText; 
    public Camera mainCamera; 

    void Update()
    {
        // Lanzamos un rayo desde el centro de la pantalla
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // Si el rayo choca con algo dentro de la distancia...
        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            // CASO A: Es un objeto para recoger (Llave, Mapa, Pilas...)
            if (hit.collider.TryGetComponent(out ItemPickup item))
            {
                interactionText.text = "Recoger " + item.item.itemName + " <color=yellow>[E]</color>";

                if (Input.GetKeyDown(KeyCode.E))
                {
                    item.Interact();
                }
            }
            // CASO B: Es un Mueble (Cajón o Puerta)
            else if (hit.collider.TryGetComponent(out DrawerInteractable mueble))
            {
                // Pedimos al mueble qué texto mostrar (ej: "Abrir", "Cerrar" o "Necesitas Llave")
                interactionText.text = mueble.GetTextoInteraccion();

                if (Input.GetKeyDown(KeyCode.E))
                {
                    mueble.Interact();
                }
            }
            // CASO C: Es la Puerta de Código (Salida)
            else if (hit.collider.TryGetComponent(out ExitCodeDoor puertaCodigo))
            {
                interactionText.text = "Introducir Código <color=yellow>[E]</color>";

                if (Input.GetKeyDown(KeyCode.E))
                {
                    puertaCodigo.Interactuar();
                }
            }

            // CASO D: Miramos algo que no hace nada (Paredes, suelo...)
            else
            {
                interactionText.text = "";
            }
        }
        else
        {
            // Si miramos al aire
            interactionText.text = "";
        }
    }
}
