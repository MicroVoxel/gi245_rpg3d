using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private int id;
    public int ID { get { return id; } set { id = value; } }

    [SerializeField] private ItemType itemType;
    public ItemType ItemType { get { return itemType; } set { itemType = value; } }

    [SerializeField] private InventoryManager inventoryManager;

    private void Start()
    {
        inventoryManager = InventoryManager.instance;
    }

    public void OnDrop(PointerEventData eventData)
    {
        // [FIX] Guard กรณี SelectChars ว่าง ป้องกัน NullRef ใน RemoveItemInBag/SaveItemInBag
        if (PartyManager.instance.SelectChars.Count == 0) return;

        // รับข้อมูล Item A (ที่กำลังถูกลาก)
        GameObject objA = eventData.pointerDrag;
        ItemDrag itemDragA = objA.GetComponent<ItemDrag>();
        if (itemDragA == null) return;

        InventorySlot slotA = itemDragA.IconParent.GetComponent<InventorySlot>();
        if (slotA == null) return;

        // ถ้า Destination (Slot B) เป็นช่องสวมใส่ ไอเทมที่ลากมาต้องตรงประเภท
        if (itemType == ItemType.Shield || itemType == ItemType.Weapon)
        {
            if (itemDragA.Item.Type != itemType) return;
        }

        // เช็คว่ามี Item B อยู่ใน Slot B หรือไม่
        ItemDrag itemInSlot = GetComponentInChildren<ItemDrag>();

        if (itemInSlot != null)
        {
            // --- SWAP: มีไอเทมอยู่ใน Slot B แล้ว ---
            ItemDrag itemDragB = itemInSlot;

            // ถ้า Source (Slot A) เป็นช่องสวมใส่ ไอเทม B ที่จะย้ายมาแทนต้องตรงประเภทด้วย
            if (slotA.ItemType == ItemType.Shield || slotA.ItemType == ItemType.Weapon)
            {
                if (itemDragB.Item.Type != slotA.ItemType) return;
            }

            // ขั้นที่ 1: ถอด A ออกจาก Slot A (UnEquip ถ้าเป็นช่องสวมใส่)
            inventoryManager.RemoveItemInBag(slotA.ID);

            // ขั้นที่ 2: ย้าย B ไป Slot A (Equip ถ้า Slot A เป็นช่องสวมใส่)
            itemDragB.transform.SetParent(itemDragA.IconParent);
            itemDragB.IconParent = itemDragA.IconParent;
            inventoryManager.SaveItemInBag(slotA.ID, itemDragB.Item);

            // ขั้นที่ 3: ถอด B ออกจาก Slot B (UnEquip ถ้าเป็นช่องสวมใส่)
            inventoryManager.RemoveItemInBag(id);
        }
        else
        {
            // --- MOVE: Slot B ว่างอยู่ ---
            // ถอด A ออกจาก Slot A (UnEquip ถ้าเป็นช่องสวมใส่)
            inventoryManager.RemoveItemInBag(slotA.ID);
        }

        // ขั้นสุดท้าย: วาง A ลง Slot B (Equip ถ้า Slot B เป็นช่องสวมใส่)
        itemDragA.IconParent = transform;
        inventoryManager.SaveItemInBag(id, itemDragA.Item);
    }
}