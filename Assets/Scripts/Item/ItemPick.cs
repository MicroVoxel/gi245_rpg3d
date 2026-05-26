using UnityEngine;

public class ItemPick : MonoBehaviour
{
    [SerializeField] private Item item;
    public Item Item { get { return item; } }

    private InventoryManager inventoryManager;
    private PartyManager partyManager;

    public void Init(Item item, InventoryManager invManager, PartyManager ptyManager)
    {
        this.item = item;
        this.inventoryManager = invManager;
        this.partyManager = ptyManager;
    }

    public void PickUpItem()
    {
        // [FIX] Guard กรณี SelectChars ว่าง ป้องกัน IndexOutOfRangeException
        if (partyManager.SelectChars.Count == 0) return;
        if (item == null) return;

        if (inventoryManager.AddItem(partyManager.SelectChars[0], item.ID))
        {
            Destroy(gameObject);
        }
    }
}