using UnityEngine;

[CreateAssetMenu(fileName = "MagicData", menuName = "Scriptable Objects/MagicData")]
public class MagicData : ScriptableObject
{
    [Header("Basic Magic Info")]
    public int id;
    public string magicName;
    public Sprite icon;

    [Header("Magic Classification")]
    [Tooltip("ระบุประเภทของเวทมนตร์เพื่อกำหนดพฤติกรรมการทำงาน")]
    public MagicType type;

    [Header("Casting Specifications")]
    public float range;
    [Tooltip("ความรุนแรงของพลังโจมตี, พลังฟื้นฟู หรือขนาดของสเตตัสที่บวก/ลบ")]
    public int power;
    public float loadTime;
    public float shootTime;
    public int loadId;
    public int shootId;

    [Header("Duration Settings (For Buff/Debuff)")]
    [Tooltip("ระยะเวลาการแสดงผลชั่วคราวของสถานะ Buff หรือ Debuff (ใส่ 0 หากเป็นเวทโจมตีปกติ)")]
    public float duration;
}