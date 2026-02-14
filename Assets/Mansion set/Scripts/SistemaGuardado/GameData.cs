using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public Vector3 playerPos;
    public Quaternion playerRot;
    public float currentSanity;
    public string sceneName;

    public List<string> itemsInventario = new List<string>();
    public bool tieneMapa;

    public float energiaLinterna = 100f;
    public float energiaVela = 100f;
    public string nombreItemEnMano = "";

    public List<string> objetosRecogidos = new List<string>();

    [System.Serializable]
    public struct ObjectData
    {
        public string id;
        public bool boolState;
        public bool boolState2;
        public float floatState;
    }
    public List<ObjectData> objetosMundo = new List<ObjectData>();

    [System.Serializable]
    public struct DroppedItemData
    {
        public string itemName;
        public Vector3 position;
        public Quaternion rotation;
    }
    public List<DroppedItemData> itemsSueltos = new List<DroppedItemData>();


}
