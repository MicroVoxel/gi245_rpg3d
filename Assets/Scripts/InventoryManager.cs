using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// จัดการระบบช่องเก็บของส่วนกลาง (Inventory)
/// พร้อมระบบป้องกันการเก็บไอเทมไหลเข้าช่องสวมใส่โดยไม่ได้ตั้งใจ (Strict Boundary Guard)
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    #region === DATA & CONFIG ===
    [Header("Item Database")]
    [Tooltip("เก็บอาเรย์ Prefab สำรอง เช่น โมเดลกระเป๋าเป้ (Bag) สำหรับเป็น Fallback")]
    [SerializeField] private GameObject[] itemPrefabs;
    public GameObject[] ItemPrefabs { get { return itemPrefabs; } set { itemPrefabs = value; } }

    [SerializeField] private ItemData[] itemData;
    public ItemData[] ItemData { get { return itemData; } set { itemData = value; } }

    public const int MAXSLOT = 18;
    public const int INVENTORY_CAPACITY = 16;
    public const int SHIELD_SLOT = 16;
    public const int WEAPON_SLOT = 17;
    #endregion

    #region === UNITY CALLBACKS ===
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    #endregion

    #region === INVENTORY LOGIC ===
    /// <summary>
    /// เพิ่มไอเทมเข้ากระเป๋าของตัวละครอย่างปลอดภัย โดยจำกัดขอบเขตไม่ให้ไหลเข้าช่องสวมใส่เด็ดขาด
    /// </summary>
    public bool AddItem(Character character, int id)
    {
        if (id < 0 || id >= itemData.Length || itemData[id] == null) return false;

        Item item = new Item(itemData[id]);

        // [CRITICAL FIX 1]: ใช้ Strict Boundary Guard บังคับลูปตรวจเช็คเฉพาะช่องเก็บของทั่วไป (0 - 15) เท่านั้น
        // และเพิ่มเงื่อนไขป้องกันดัชนีเกินขนาดจริงของอาเรย์เพื่อความปลอดภัยสูงสุด
        for (int i = 0; i < INVENTORY_CAPACITY; i++)
        {
            if (i >= character.InventoryItems.Length) break;

            if (character.InventoryItems[i] == null)
            {
                character.InventoryItems[i] = item;
                return true;
            }
        }

        Debug.Log("Inventory Full");
        return false;
    }

    public void SaveItemInBag(int index, Item item)
    {
        if (PartyManager.instance.SelectChars.Count == 0) return;

        Character hero = PartyManager.instance.SelectChars[0];

        switch (index)
        {
            case SHIELD_SLOT:
                hero.EquipShield(item);
                break;
            case WEAPON_SLOT:
                hero.EquipWeapon(item);
                break;
            default:
                if (index >= 0 && index < INVENTORY_CAPACITY)
                {
                    if (hero.InventoryItems[index] != null)
                    {
                        RemoveItemInBag(index);
                    }
                    hero.InventoryItems[index] = item;
                }
                break;
        }
    }

    public void RemoveItemInBag(int index)
    {
        if (PartyManager.instance.SelectChars.Count == 0) return;

        Character hero = PartyManager.instance.SelectChars[0];

        switch (index)
        {
            case SHIELD_SLOT:
                hero.UnEquipShield();
                break;
            case WEAPON_SLOT:
                hero.UnEquipWeapon();
                break;
            default:
                if (index >= 0 && index < INVENTORY_CAPACITY)
                {
                    hero.InventoryItems[index] = null;
                }
                break;
        }
    }

    public void RemoveItemFromHeroBag(Character hero, int itemID)
    {
        // [CRITICAL FIX 2]: จำกัดวงลูปการลบไอเทมธรรมดาให้อยู่ภายในขอบเขตกระเป๋าเป้หลักเท่านั้น
        for (int i = 0; i < INVENTORY_CAPACITY; i++)
        {
            if (i >= hero.InventoryItems.Length) break;

            if (hero.InventoryItems[i] != null && hero.InventoryItems[i].ID == itemID)
            {
                hero.InventoryItems[i] = null;
                return;
            }
        }

        if (hero.Shield != null && hero.Shield.ID == itemID)
            hero.UnEquipShield();
        else if (hero.MainWeapon != null && hero.MainWeapon.ID == itemID)
            hero.UnEquipWeapon();
    }

    public void DrinkConsumableItem(Item item, int slotID)
    {
        if (PartyManager.instance.SelectChars.Count > 0)
        {
            PartyManager.instance.SelectChars[0].Recover(item.Power);
            RemoveItemInBag(slotID);
        }
    }
    #endregion

    #region === QUEST & PARTY INVENTORY LOGIC ===
    public bool CheckPartyForItem(int id)
    {
        List<Character> party = PartyManager.instance.Members;

        foreach (Character hero in party)
        {
            for (int i = 0; i < INVENTORY_CAPACITY; i++)
            {
                if (i >= hero.InventoryItems.Length) break;

                if (hero.InventoryItems[i] != null && hero.InventoryItems[i].ID == id)
                    return true;
            }
        }
        return false;
    }

    public bool RemoveItemFromParty(int id)
    {
        List<Character> party = PartyManager.instance.Members;

        foreach (Character hero in party)
        {
            for (int i = 0; i < INVENTORY_CAPACITY; i++)
            {
                if (i >= hero.InventoryItems.Length) break;

                if (hero.InventoryItems[i] != null && hero.InventoryItems[i].ID == id)
                {
                    hero.InventoryItems[i] = null;
                    return true;
                }
            }
        }
        return false;
    }
    #endregion

    #region === DROP SYSTEM ===
    public void SpawnDropInventory(Item[] items, Vector3 pos)
    {
        float minRadius = 0.3f;
        float maxRadius = 0.8f;

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null && !string.IsNullOrEmpty(items[i].ItemName))
            {
                float angle = Random.Range(0f, Mathf.PI * 2);
                float radius = Random.Range(minRadius, maxRadius);

                Vector3 offset = new Vector3(
                    Mathf.Cos(angle) * radius,
                    0,
                    Mathf.Sin(angle) * radius
                );

                SpawnDropItem(items[i], pos + offset);
            }
        }
    }

    /// <summary>
    /// สปอว์นของดรอปโดยปรับสเกลให้ถูกต้องเป็นอันดับแรก ก่อนนำไปคำนวณตำแหน่งออฟเซ็ตป้องกันการดีดตัว
    /// </summary>
    public void SpawnDropItem(Item item, Vector3 pos)
    {
        if (item == null) return;

        GameObject prefabToSpawn = item.ItemPrefab;

        if (prefabToSpawn == null)
        {
            if (itemPrefabs != null && itemPrefabs.Length > 0)
            {
                prefabToSpawn = itemPrefabs[0];
            }
        }

        if (prefabToSpawn == null)
        {
            Debug.LogError($"[InventoryManager] ไม่สามารถดรอป {item.ItemName} ได้ เนื่องจากไม่พบวัตถุต้นแบบ (Prefab)!");
            return;
        }

        LayerMask groundLayer = LayerMask.GetMask("Ground");
        Vector3 rayStart = pos + Vector3.up * 5f;
        Vector3 targetGroundPos = pos;

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 20f, groundLayer))
        {
            targetGroundPos = hit.point;
        }

        GameObject itemObj = Instantiate(prefabToSpawn, targetGroundPos, Quaternion.identity);
        itemObj.name = $"{item.ItemName}_OnGround";
        itemObj.tag = "Item";

        if (itemObj.transform.localScale.x > 5f || itemObj.transform.localScale.y > 5f || itemObj.transform.localScale.z > 5f)
        {
            itemObj.transform.localScale = Vector3.one;
        }

        Physics.SyncTransforms();

        Collider col = itemObj.GetComponentInChildren<Collider>();
        if (col != null)
        {
            Vector3 adjustedPos = itemObj.transform.position;
            adjustedPos.y += col.bounds.extents.y;
            itemObj.transform.position = adjustedPos;
        }

        ItemPick itemPick = itemObj.GetComponent<ItemPick>();
        if (itemPick == null)
        {
            itemPick = itemObj.AddComponent<ItemPick>();
        }

        itemPick.Init(item, instance, PartyManager.instance);
    }
    #endregion

    #region === SHOP INITIALIZATION ===
    private void AddItemShopToNpc(int npcId, int itemId)
    {
        if (npcId < QuestManager.instance.NPCPerson.Length && itemId < itemData.Length)
        {
            QuestManager.instance.NPCPerson[npcId].ShopItems.Add(itemData[itemId]);
        }
    }
    #endregion
}