using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    [Header("Enemy Tracking")]
    [SerializeField] private List<Enemy> monsters = new List<Enemy>();

    /// <summary>
    /// ใช้ IReadOnlyList เพื่อให้คลาสอื่นอ่านค่าได้อย่างเดียว (Read-Only) ป้องกันการแก้ไข List โดยตรงจากภายนอก
    /// </summary>
    public IReadOnlyList<Enemy> Monsters => monsters;

    public static EnemyManager instance { get; private set; }

    private void Awake()
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
        // ทำการคัดลอกรายชื่อเริ่มต้นออกมาเพื่อป้องกันบั๊ก Collection Modified ตอนลงทะเบียน
        var initialMonsters = new List<Enemy>(monsters);
        monsters.Clear();

        foreach (Enemy m in initialMonsters)
        {
            if (m != null)
            {
                RegisterEnemy(m);
            }
        }
    }

    /// <summary>
    /// ลงทะเบียนศัตรูเข้าสู่ระบบ (รองรับทั้งมอนสเตอร์ที่วางไว้ใน Scene และมอนสเตอร์ที่เสกมาใหม่แบบ Dynamic)
    /// </summary>
    public void RegisterEnemy(Enemy enemy)
    {
        if (enemy == null || monsters.Contains(enemy)) return;

        monsters.Add(enemy);

        // สั่งให้มอนสเตอร์เริ่มทำงานทันทีหลังจากลงทะเบียนสำเร็จ
        enemy.CharInit(UIManager.instance, InventoryManager.instance, PartyManager.instance);

        Debug.Log($"[EnemyManager] Registered: {enemy.CharName} (ID: {enemy.EnemyID})");
    }

    /// <summary>
    /// ถอนการลงทะเบียนมอนสเตอร์เมื่อตายหรือถูกทำลาย เพื่อป้องกันการสะสม Null ขยะในระบบ (Memory Leak Prevention)
    /// </summary>
    public void UnregisterEnemy(Enemy enemy)
    {
        if (enemy == null) return;

        if (monsters.Contains(enemy))
        {
            monsters.Remove(enemy);
            Debug.Log($"[EnemyManager] Unregistered: {enemy.CharName}");
        }
    }
}