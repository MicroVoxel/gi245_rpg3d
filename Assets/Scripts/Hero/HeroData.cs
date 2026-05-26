using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// HeroData: ใช้เก็บค่าเริ่มต้นและสถานะของ Hero เพื่อการ Save/Load
/// </summary>
[CreateAssetMenu(fileName = "HeroData", menuName = "Scriptable Objects/HeroData")]
public class HeroData : ScriptableObject
{
    public int prefabId;
    public int curHp;
    public int maxHP;
    public List<int> magicIds = new List<int>();

    public int[] inventoryItemIds;

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
        // 1. กรณีที่ 1: ยังไม่เคยสร้าง Array เลย (ไฟล์เพิ่งถูกสร้างใหม่เอี่ยม)
        if (inventoryItemIds == null || inventoryItemIds.Length == 0)
        {
            inventoryItemIds = new int[InventoryManager.MAXSLOT];
            for (int i = 0; i < inventoryItemIds.Length; i++)
            {
                inventoryItemIds[i] = -1;
            }
        }
        // 2. กรณีที่ 2: ขนาด Array เดิม ไม่เท่ากับ MAXSLOT ปัจจุบัน (มีการอัปเดตแพตช์เกมเพิ่ม/ลดช่องกระเป๋า)
        else if (inventoryItemIds.Length != InventoryManager.MAXSLOT)
        {
            // สร้างกระเป๋าใบใหม่มารองรับขนาดใหม่
            int[] newInventory = new int[InventoryManager.MAXSLOT];

            for (int i = 0; i < newInventory.Length; i++)
            {
                // ถ้าเป็นช่องที่มีของอยู่เดิม ให้โอนย้ายของเดิมมาใส่
                if (i < inventoryItemIds.Length)
                {
                    newInventory[i] = inventoryItemIds[i];
                }
                // ถ้าเป็นช่องที่งอกขึ้นมาใหม่ ให้ตั้งค่าเป็นช่องว่าง (-1)
                else
                {
                    newInventory[i] = -1;
                }
            }

            // สลับกระเป๋าไปใช้ใบใหม่ที่โอนข้อมูลเรียบร้อยแล้ว
            inventoryItemIds = newInventory;
        }
    }
}