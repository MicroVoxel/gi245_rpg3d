using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private GameObject[] itemPrefabs;
    public GameObject[] ItemPrefabs { get { return itemPrefabs; } set { itemPrefabs = value; } }

    [SerializeField] private ItemData[] itemData;
    public ItemData[] ItemData { get { return itemData; } set { itemData = value; } }

    public const int MAXSLOT = 16;

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
    }

    public void RemoveItemInBag(int index)
    {
        if (PartyManager.instance.SelectChars.Count == 0) return;

        PartyManager.instance.SelectChars[0].InventoryItems[index] = null;
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
}
