using UnityEngine;

#region Enums
/// <summary>
/// ประเภทของเวทมนตร์ภายในเกม
/// </summary>
public enum MagicType
{
    Projectile,     // ขีปนาวุธวิ่งไปหาเป้าหมาย (ต้องใช้เวลาเดินทาง)
    SpawnOnEnemy,   // เกิดผลลัพธ์หรือระเบิดบนตัวศัตรูทันทีโดยไม่ต้องเดินทาง
    Buff,           // เพิ่มสเตตัสและฟื้นพลังให้กับตัวเองหรือฝ่ายเดียวกัน
    Debuff          // ลดพลังโจมตีหรือพลังป้องกันของศัตรูชั่วคราว
}
#endregion

[System.Serializable]
public class Magic
{
    #region === PROPERTIES ===
    [SerializeField] private int id;
    public int ID => id;

    [SerializeField] private string name;
    public string Name => name;

    [SerializeField] private Sprite icon;
    public Sprite Icon => icon;

    [SerializeField] private float range;
    public float Range => range;

    [SerializeField] private int power;
    public int Power => power;

    [SerializeField] private float loadTime;
    public float LoadTime => loadTime;

    [SerializeField] private float shootTime;
    public float ShootTime => shootTime;

    [SerializeField] private int loadID;
    public int LoadID => loadID;

    [SerializeField] private int shootId;
    public int ShootId => shootId;

    // [NEW PROPERTIES] เพื่อรองรับระบบเวทมนตร์แบบใหม่
    [SerializeField] private MagicType type;
    public MagicType Type => type;

    [Tooltip("ระยะเวลาส่งผลของสถานะ Buff หรือ Debuff (หน่วยเป็นวินาที)")]
    [SerializeField] private float duration;
    public float Duration => duration;
    #endregion

    /// <summary>
    /// Constructor สำหรับแปลงข้อมูลจาก ScriptableObject มาเป็น Instance พร้อมใช้งานในระบบเกม
    /// </summary>
    public Magic(MagicData data)
    {
        if (data == null)
        {
            Debug.LogError("Magic: ไม่สามารถสร้างข้อมูลได้เนื่องจาก MagicData ต้นแบบเป็น Null!");
            return;
        }

        this.id = data.id;
        this.name = data.magicName;
        this.icon = data.icon;
        this.range = data.range;
        this.power = data.power;
        this.loadTime = data.loadTime;
        this.shootTime = data.shootTime;
        this.loadID = data.loadId;
        this.shootId = data.shootId;

        this.type = data.type;
        this.duration = data.duration;
    }
}