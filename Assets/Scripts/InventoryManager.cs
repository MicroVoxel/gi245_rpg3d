using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private GameObject[] itemPrefabs;
    public GameObject[] ItemPrefabs { get { return itemPrefabs; } set { itemPrefabs = value; } }

    [SerializeField] private ItemData[] itemData;
    public ItemData[] ItemData { get { return itemData; } set { itemData = value; } }

    public const int MAXSLOT = 18;

    public static InventoryManager instance;

    void Awake()
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

    public bool AddItem(Character character, int id)
    {
        Item item = new Item(itemData[id]);

        for (int i = 0; i < character.InventoryItems.Length; i++)
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

        PartyManager.instance.SelectChars[0].InventoryItems[index] = item;

        switch (index)
        {
            case 16:
                PartyManager.instance.SelectChars[0].EquipShield(item); 
                break;
            case 17:
                PartyManager.instance.SelectChars[0].EquipWeapon(item); 
                break;    
        }

    }

    public void RemoveItemInBag(int index)
    {
        if (PartyManager.instance.SelectChars.Count == 0) return;

        PartyManager.instance.SelectChars[0].InventoryItems[index] = null;

        switch (index)
        {
            case 16:
                PartyManager.instance.SelectChars[0].UnEquipShield();
                break;
            case 17:
                PartyManager.instance.SelectChars[0].UnEquipWeapon();
                break;
        }
    }

    private void SpawnDropItem(Item item, Vector3 pos)
    {
        int id;

        switch (item.Type)
        {
            case ItemType.Consumable:
                id = 1;
                break;
            default:
                id = 0;
                break;
        }

        LayerMask groundLayer = LayerMask.GetMask("Ground");
        RaycastHit hit;
        Vector3 rayStart = pos + Vector3.up * 5f;
        if (Physics.Raycast(rayStart, Vector3.down, out hit, 20f, groundLayer))
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

    public void DrinkConsumableItem(Item item, int slotID)
    {
        string s = string.Format("Drink: {0}", item.ItemName);
        Debug.Log(s);

        if(PartyManager.instance.SelectChars.Count > 0)
        {
            PartyManager.instance.SelectChars[0].Recover(item.Power);
            RemoveItemInBag(slotID);
        }

    }

    public bool CheckPartyForItem(int id)
    {
        Item item = new Item(itemData[id]);

        List<Character> party = PartyManager.instance.Members;

        foreach (Character hero in party)
        {
            for (int i = 0; i < hero.InventoryItems.Length; i++)
            {
                if (hero.InventoryItems[i].ID == item.ID)
                    return true;
            }
        }

        return false;

    }

    public bool RemoveItemFromParty(int id)
    {
        Item item = new Item(itemData[id]);
        Debug.Log($"Finding {item.ItemName}");

        List<Character> selectedHero = PartyManager.instance.SelectChars;

        foreach (Character hero in selectedHero)
        {
            for (int i = 0; i < hero.InventoryItems.Length; i++)
            {
                if (hero.InventoryItems[i].ID == item.ID)
                {
                    Debug.Log($"Removing {hero.InventoryItems[i].ItemName}");
                    hero.InventoryItems[i] = null;
                    Debug.Log($"Removed {hero.InventoryItems[i]}");
                    return true;
                }
            }
        }
        return false;
    }

    private void AddItemShopToNpc(int npcId, int itemId)
    {
        Item item = new Item(itemData[itemId]);
        QuestManager.instance.NPCPerson[npcId].ShopItems.Add(item);
    }

}
