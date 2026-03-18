using System.Security.Cryptography;
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

}
