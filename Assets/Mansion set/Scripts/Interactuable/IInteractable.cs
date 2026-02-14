using UnityEngine;

// Define qué deben tener todos los objetos interactuables
public interface IInteractable
{
    void Interact();
    string GetDescription(); // Para mostrar texto tipo "Abrir Cajón"
}
