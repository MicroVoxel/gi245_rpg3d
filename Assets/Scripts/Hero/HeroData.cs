using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// HeroData: ใช้เก็บค่าเริ่มต้นและสถานะของ Hero เพื่อการ Save/Load
/// อัปเดต: แยก Equipment ออกจาก Inventory Array ตามหลัก SRP
/// </summary>
[CreateAssetMenu(fileName = "HeroData", menuName = "Scriptable Objects/HeroData")]
public class HeroData : ScriptableObject
{
    public int prefabId;
    public int curHp;
    public int maxHP;
    public List<int> magicIds = new List<int>();

    [Header("Inventory (Max 16)")]
    public int[] inventoryItemIds;

    [Header("Equipment")]
    public int mainWeaponId = -1;
    public int shieldId = -1;
    // อนาคตสามารถเพิ่ม public int helmetId = -1; ฯลฯ ได้ง่ายๆ ที่นี่

    [Header("Stats")]
    public int attackDamage;
    public int baseDefense;
    public int exp;
    public int level;
    public int nextExp;
    public int strength;
    public int dexterity;
    public int constitution;
    public int intelligence;
    public int wisdom;
    public int charisma;

    private void OnEnable()
    {
        // ค่าคงที่ความจุใหม่ของกระเป๋า (เอาช่องอาวุธ/โล่ออกไปแล้ว)
        int NEW_CAPACITY = 16;

        // 1. เพิ่งสร้างไฟล์ใหม่เอี่ยม
        if (inventoryItemIds == null || inventoryItemIds.Length == 0)
        {
            inventoryItemIds = new int[NEW_CAPACITY];
            for (int i = 0; i < inventoryItemIds.Length; i++)
            {
                inventoryItemIds[i] = -1;
            }
        }
        // 2. Migration: กรณีเป็นเซฟเก่าที่ Array เป็นขนาด 18 (หรือขนาดอื่นที่ผิดเพี้ยน)
        else if (inventoryItemIds.Length != NEW_CAPACITY)
        {
            int[] newInventory = new int[NEW_CAPACITY];

            for (int i = 0; i < inventoryItemIds.Length; i++)
            {
                // โอนของในกระเป๋าปกติ (0-15)
                if (i < NEW_CAPACITY)
                {
                    newInventory[i] = inventoryItemIds[i];
                }
                // [DATA MIGRATION] ดึงข้อมูลสวมใส่จาก Array เก่ามาใส่ตัวแปรใหม่ (ถ้ามีของอยู่)
                else if (i == 16 && inventoryItemIds[16] != -1)
                {
                    shieldId = inventoryItemIds[16];
                }
                else if (i == 17 && inventoryItemIds[17] != -1)
                {
                    mainWeaponId = inventoryItemIds[17];
                }
            }

            // สลับไปใช้กระเป๋าใบใหม่ที่สะอาดและขนาดถูกต้อง (16 ช่อง)
            inventoryItemIds = newInventory;
        }
    }
}