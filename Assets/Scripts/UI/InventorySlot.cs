using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private int id;
    public int ID {  get { return id; } set { id = value; } }

    [SerializeField] private InventoryManager inventoryManager;

    private void Start()
    {
        inventoryManager = InventoryManager.instance;
    }

    public void OnDrop(PointerEventData eventData)
    {
        //Get Item A
        GameObject objA = eventData.pointerDrag;
        ItemDrag itemDragA = objA.GetComponent<ItemDrag>();
        InventorySlot slotA = itemDragA.IconParent.GetComponent<InventorySlot>();
        //Remove Item A from Slot A
        inventoryManager.RemoveItemInBag(slotA.ID);

        //There is an Item B in Slot B
        ItemDrag itemInSlot = GetComponentInChildren<ItemDrag>();
        if (itemInSlot != null)
        {
            GameObject objB = itemInSlot.gameObject;
            ItemDrag itemDragB = objB.GetComponent<ItemDrag>();

            //Set Item B on Slot A
            itemDragB.transform.SetParent(itemDragA.IconParent);
            itemDragB.IconParent = itemDragA.IconParent;
            inventoryManager.SaveItemInBag(slotA.ID,itemDragB.Item);
        }

        //Set Item A on Slot B
        itemDragA.IconParent = transform;
        inventoryManager.SaveItemInBag(id, itemDragA.Item);

    }
}
