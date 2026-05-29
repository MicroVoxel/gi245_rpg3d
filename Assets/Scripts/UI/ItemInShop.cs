using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemInShop : MonoBehaviour
{
    [SerializeField] private int id;
    public int ID { get { return id; } set { id = value; } }

    [SerializeField] private Item item;
    public Item Item { get { return item; } set { item = value; } }

    [SerializeField] private Toggle iconToggle;
    public Toggle IconToggle { get { return iconToggle; } }

    [SerializeField] private TMP_Text itemText;
    [SerializeField] private TMP_Text priceText;

    [SerializeField] private UIManager uiMgr;

    /// <summary>
    /// รับ ItemData เข้ามาแล้ว Mapping ลงตัวแปรเดิม เพื่อให้ UIManager ใช้งานต่อได้ทันที
    /// </summary>
    public void SetupItemInShop(ItemData data, UIManager uIManager, float discount)
    {
        uiMgr = uIManager;

        // 1. Mapping ข้อมูลจาก ItemData ลงตัวแปรเดิมที่คุณมีอยู่
        this.id = data.id;

        // ตรงนี้ถ้า Item คลาสของคุณยังไม่ได้ทำ Constructor ไว้สำหรับ ItemData
        // แนะนำให้สร้างขึ้นมา หรือสร้าง Item ใหม่จาก data ตามที่เคยทำไว้ครับ
        this.item = new Item(data);

        // 2. ใช้ตัวแปรเดิมแสดงผลตามปกติ
        if (iconToggle != null && iconToggle.targetGraphic != null)
        {
            iconToggle.targetGraphic.GetComponent<Image>().sprite = item.Icon;
        }

        if (itemText != null) itemText.text = item.ItemName;
        if (priceText != null) priceText.text = ((int)(item.NormalPrice * discount)).ToString();
    }
}