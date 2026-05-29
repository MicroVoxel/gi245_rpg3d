using UnityEngine;

public enum ItemType
{
    Consumable,
    Equipment,
    Shield,
    Armor,
    Weapon,
    Ammo,
    Quest,
    Other
}

[System.Serializable]
public class Item
{
    [SerializeField] private int id;
    public int ID { get { return id; } }

    [SerializeField] private string itemName;
    public string ItemName { get { return itemName; } }

    [SerializeField] private ItemType type;
    public ItemType Type { get { return type; } }

    [SerializeField] private Sprite icon;
    public Sprite Icon { get { return icon; } }

    [SerializeField] private int power;
    public int Power { get { return power; } }

    [SerializeField] private GameObject itemPrefab;
    public GameObject ItemPrefab { get { return itemPrefab; } }

    [SerializeField] private int normalPrice;
    public int NormalPrice { get { return normalPrice; } }

    /// <summary>
    /// Default Constructor จำเป็นมากสำหรับระบบ Unity Serialization (ป้องกันไม่ให้ Array กระเป๋าเป้เป็น Null หรือพัง)
    /// </summary>
    public Item()
    {
    }

    /// <summary>
    /// Constructor สำหรับแปลงข้อมูลจาก ItemData (ScriptableObject) มาสร้างเป็น Item (Instance) สำหรับตัวละคร
    /// </summary>
    public Item(ItemData data)
    {
        if (data == null)
        {
            Debug.LogWarning("Item: พยายามสร้างไอเทมจาก ItemData ที่ว่างเปล่า (Null)");
            return;
        }

        id = data.id;
        itemName = data.itemName;
        type = data.type;
        icon = data.icon;
        power = data.power;
        itemPrefab = data.itemPrefab; // ทำการคัดลอกตำแหน่ง GameObject Prefab มาใช้งาน
        normalPrice = data.normalPrice;
    }
}