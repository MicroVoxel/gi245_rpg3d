using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// จัดการระบบช่องเก็บของส่วนกลาง (Inventory)
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    #region === DATA & CONFIG ===
    [Header("Item Database")]
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

    private void Start()
    {
        AddItemShopToNpc(1, 0);
        AddItemShopToNpc(1, 2);
        AddItemShopToNpc(1, 5);
    }
    #endregion

    #region === INVENTORY LOGIC ===
    public bool AddItem(Character character, int id)
    {
        Item item = new Item(itemData[id]);

        for (int i = 0; i < INVENTORY_CAPACITY; i++)
        {
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

        // [FIX LOGIC] ถอดของเก่าออกก่อน (ถ้ามี) ป้องกันสเตตัสบวกทบกันและโมเดลซ้อนกัน
        if (hero.InventoryItems[index] != null)
        {
            RemoveItemInBag(index);
        }

        hero.InventoryItems[index] = item;

        switch (index)
        {
            case SHIELD_SLOT:
                hero.EquipShield(item);
                break;
            case WEAPON_SLOT:
                hero.EquipWeapon(item);
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
        }

        hero.InventoryItems[index] = null;
    }

    // [NEW] ฟังก์ชันสำหรับ UIManager ที่สั่งขายของออกจากช่องไหนก็ได้
    public void RemoveItemFromHeroBag(Character hero, int itemID)
    {
        for (int i = 0; i < MAXSLOT; i++)
        {
            if (hero.InventoryItems[i] != null && hero.InventoryItems[i].ID == itemID)
            {
                // ตรวจสอบและ UnEquip ด้วยหากเป็นช่องสวมใส่
                if (i == SHIELD_SLOT) hero.UnEquipShield();
                else if (i == WEAPON_SLOT) hero.UnEquipWeapon();

                hero.InventoryItems[i] = null;
                return;
            }
        }
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
            if (items[i] != null)
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

    private void SpawnDropItem(Item item, Vector3 pos)
    {
        int id = item.Type == ItemType.Consumable ? 1 : 0;

        LayerMask groundLayer = LayerMask.GetMask("Ground");
        Vector3 rayStart = pos + Vector3.up * 5f;

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 20f, groundLayer))
        {
            pos = hit.point;
        }

        GameObject itemObj = Instantiate(ItemPrefabs[id], pos, Quaternion.identity);

        Collider col = itemObj.GetComponentInChildren<Collider>();
        if (col != null)
        {
            Vector3 adjustedPos = itemObj.transform.position;
            adjustedPos.y += col.bounds.extents.y;
            itemObj.transform.position = adjustedPos;
        }

        ItemPick itemPick = itemObj.GetComponent<ItemPick>();
        if (itemPick == null)
            itemPick = itemObj.AddComponent<ItemPick>();

        itemPick.Init(item, instance, PartyManager.instance);
    }
    #endregion

    #region === SHOP INITIALIZATION ===
    private void AddItemShopToNpc(int npcId, int itemId)
    {
        Item item = new Item(itemData[itemId]);
        QuestManager.instance.NPCPerson[npcId].ShopItems.Add(item);
    }
    #endregion
}