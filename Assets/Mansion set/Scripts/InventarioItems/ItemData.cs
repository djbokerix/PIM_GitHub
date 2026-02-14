using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TipoObjeto
{
    Normal,     // Llaves, papeles, mapa...
    Linterna,   // La propia linterna
    Bateria,    // Pilas para recargar
    Vela,       // Vela
    Mechero,    // Para encender vela
    Nota,        //Notas
    Pastillas
}



[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public TipoObjeto tipo; 

    [Header("Para soltarlo (Opcional)")]
    public GameObject prefabModelo3D; // El modelo 3D que cae al suelo al soltarlo

    [Header("Vista Primera Persona")]
    public GameObject modeloFPS;    //El modelo que se vera en priemra persona
    public Vector3 posicionFPS; 
    public Vector3 rotacionFPS; 

    [Header("Solo para Notas")]
    [TextArea(5, 10)] // 
    public string contenidoNota;

}
