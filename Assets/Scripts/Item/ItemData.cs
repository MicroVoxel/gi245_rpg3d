using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("Item Core Data")]
    public int id;
    public string itemName;
    public ItemType type;
    public Sprite icon;
    public int power;
    public int normalPrice;

    [Header("Visual Asset")]
    [Tooltip("ลาก Prefab มาใส่ตรงนี้")]
    public GameObject itemPrefab; // เปลี่ยนจาก int prefabID เป็น GameObject โดยตรง
}