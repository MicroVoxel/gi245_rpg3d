using UnityEngine;

/// <summary>
/// แนบเข้ากับไอเทมที่ดรอปอยู่บนพื้น เพื่อให้ผู้เล่นคลิกเก็บเข้ากระเป๋าได้
/// พร้อมระบบหมุนและลอยขึ้น-ลงแบบปลอดภัย ไม่จมดินและไม่ดีดขึ้นฟ้า
/// </summary>
public class ItemPick : MonoBehaviour
{
    [SerializeField] private Item item;
    public Item Item { get { return item; } }

    private InventoryManager inventoryManager;
    private PartyManager partyManager;

    [Header("Visual Effects (หมุนและลอย)")]
    [Tooltip("ความเร็วในการหมุนรอบตัวเอง (องศาต่อวินาที)")]
    [SerializeField] private float rotationSpeed = 50f;

    [Tooltip("ความสูงในการลอยขึ้นด้านบน (จะลอยขึ้นจากพื้นอย่างเดียว ไม่ลอยลงใต้ดิน)")]
    [SerializeField] private float hoverAmplitude = 0.15f;

    [Tooltip("ความถี่หรือความเร็วในการลอยขึ้น-ลง")]
    [SerializeField] private float hoverFrequency = 1.5f;

    private Vector3 startPos;
    private float hoverTimer;
    private bool isInitialized;

    /// <summary>
    /// ฟังก์ชันสถาปนาข้อมูล (Dependency Injection) เมื่อไอเทมถูกเสกขึ้นมาบนพื้นโลก
    /// </summary>
    public void Init(Item item, InventoryManager invManager, PartyManager ptyManager)
    {
        this.item = item;
        this.inventoryManager = invManager;
        this.partyManager = ptyManager;

        // [CRITICAL FIX 1]: ป้องกันปัญหาฟิสิกส์ดีดตัวขึ้นฟ้าตอนดรอปบนพื้น
        // หากใน Prefab มี Rigidbody ให้บังคับเป็น Kinematic เพื่อไม่ให้ฟิสิกส์ตีกับการเคลื่อนที่แบบลอยตัวของเรา
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // บันทึกตำแหน่งเริ่มต้นหลังจากถูกปรับความสูงจากพื้นและขนาดของ Collider แล้ว
        startPos = transform.position;
        hoverTimer = 0f; // เริ่มต้นนับเวลาจาก 0 เสมอ
        isInitialized = true;
    }

    private void Update()
    {
        HandleVisualEffects();
    }

    /// <summary>
    /// จัดการเอฟเฟกต์หมุนรอบตัวเองและลอยขึ้น-ลงของไอเทมบนพื้นอย่างปลอดภัย
    /// </summary>
    private void HandleVisualEffects()
    {
        // [CRITICAL FIX 2]: ป้องกันอาวุธสวมใส่หมุนหลุดมือ
        // สคริปต์จะทำงานเฉพาะเมื่อไอเทมนี้ถูก Initialize ให้เป็น "ของดรอปบนพื้น" แล้วเท่านั้น
        // หากสวมใส่อยู่ในมือฮีโร่ (isInitialized เป็น false) จะไม่มีการหมุนและลอยเด็ดขาด!
        if (!isInitialized) return;

        // 1. หมุนไอเทมรอบแกน Y (แกนตั้ง) อ้างอิงตามพิกัดโลก (Space.World)
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        // 2. เอฟเฟกต์ลอยขึ้น-ลงแบบนุ่มนวล
        hoverTimer += Time.deltaTime;

        Vector3 tempPos = transform.position;

        // ใช้สูตร Cosine Shifting (1f - Cos) แทน Sine เพื่อไม่ให้ลอยจมดินไปเบียดฟิสิกส์ฉาก
        float hoverOffset = (1f - Mathf.Cos(hoverTimer * hoverFrequency)) * 0.5f * hoverAmplitude;

        tempPos.y = startPos.y + hoverOffset;
        transform.position = tempPos;
    }

    public void PickUpItem()
    {
        if (partyManager == null || partyManager.SelectChars == null || partyManager.SelectChars.Count == 0) return;
        if (item == null || inventoryManager == null) return;

        // ดึงฮีโร่ตัวแรกที่ผู้เล่นกำลังเลือกอยู่มาเก็บไอเทมชิ้นนี้เข้ากระเป๋า
        if (inventoryManager.AddItem(partyManager.SelectChars[0], item.ID))
        {
            Destroy(gameObject);
        }
    }
}