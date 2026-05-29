using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// โครงสร้างข้อมูลสำหรับเก็บ ItemData และโอกาสดรอป
/// </summary>
[System.Serializable]
public struct LootDrop
{
    [Tooltip("ลาก Scriptable Object 'ItemData' มาใส่ตรงนี้")]
    public ItemData itemData;

    [Tooltip("โอกาสดรอปไอเทมชิ้นนี้ (0 - 100%)")]
    [Range(0f, 100f)]
    public float dropChance;
}

public class Enemy : Character
{
    [Header("Enemy Settings")]
    [Tooltip("ไอดีเฉพาะของศัตรูตัวนี้ สำหรับนำไปตรวจสอบกับเงื่อนไขของเควส KillCount (ต้องตรงกับ TargetEnemyId ใน QuestData)")]
    [SerializeField] private int enemyID;
    public int EnemyID => enemyID;

    [Header("Enemy Rewards")]
    [SerializeField] private int expDrop;
    public int ExpDrop => expDrop;

    [Header("Loot Table (ตั้งค่าของดรอป)")]
    [Tooltip("เพิ่ม ItemData และเปอร์เซ็นต์การดรอปได้ที่นี่")]
    [SerializeField] private List<LootDrop> lootTable = new List<LootDrop>();

    private void Update()
    {
        switch (state)
        {
            case CharState.Walk:
                WalkUpdate();
                break;
            case CharState.WalkToEnemy:
                WalkToEnemyUpdate();
                break;
            case CharState.Attack:
                AttackUpdate();
                break;
        }
    }

    protected override void Die()
    {
        // ทำการสุ่มของดรอปก่อนที่ตัวละครจะถูกทำลาย
        DropLoot();

        base.Die();

        // ส่งสัญญาณการตายพร้อมส่งค่า ID ไปให้ QuestManager ทราบเพื่อบันทึกแต้มสะสม
        QuestManager.OnEnemyKilled?.Invoke(this.EnemyID);

        // [NEW] แจ้งเตือน EnemyManager ให้ถอนชื่อออกจากรายชื่อผู้ดูแลทันทีเมื่อตาย ป้องกันขยะ Null ในหน่วยความจำ
        if (EnemyManager.instance != null)
        {
            EnemyManager.instance.UnregisterEnemy(this);
        }

        if (partyManager != null)
        {
            partyManager.DistributeTotalExp(expDrop);
        }
    }

    /// <summary>
    /// ฟังก์ชันคำนวณและสุ่มไอเทมดรอปจาก ScriptableObject ตามเปอร์เซ็นต์ที่ตั้งไว้
    /// </summary>
    private void DropLoot()
    {
        if (invManager == null || lootTable.Count == 0) return;

        foreach (LootDrop loot in lootTable)
        {
            if (loot.itemData != null)
            {
                // สุ่มตัวเลข 0.00 - 100.00
                float randomValue = Random.Range(0f, 100f);

                if (randomValue <= loot.dropChance)
                {
                    // ดึง Data มาสร้างเป็น Instance ใหม่ ป้องกันการแก้ไข ScriptableObject ต้นฉบับ
                    Item dropItem = new Item(loot.itemData);
                    invManager.SpawnDropItem(dropItem, transform.position);
                }
            }
        }
    }
}