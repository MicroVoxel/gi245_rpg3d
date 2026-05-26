using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ตัวละคร NPC ในเกม รองรับระบบร้านค้า (Shop) และระบบมอบหมายเควส (Quest)
/// </summary>
public class Npc : Character
{
    #region === SHOP SYSTEM ===
    [Header("Shop Settings")]
    [SerializeField] private bool isShopKeeper;
    [SerializeField] private int npcMoney = 3000;
    [SerializeField] private List<Item> shopItems = new List<Item>();

    public bool IsShopKeeper => isShopKeeper;
    public int NpcMoney
    {
        get => npcMoney;
        set => npcMoney = value;
    }
    public List<Item> ShopItems
    {
        get => shopItems;
        set => shopItems = value;
    }
    #endregion

    #region === QUEST SYSTEM ===
    [Header("Quest Settings")]
    [Tooltip("ใส่ Scriptable Object (QuestData) ที่ต้องการให้ NPC ตัวนี้มอบให้ผู้เล่นตั้งแต่เริ่มเกม")]
    [SerializeField] private List<QuestData> startingQuestData = new List<QuestData>();

    [SerializeField] private List<Quest> questToGive = new List<Quest>();

    public List<Quest> QuestToGive
    {
        get => questToGive;
        set => questToGive = value;
    }

    /// <summary>
    /// แปลงข้อมูล QuestData เริ่มต้น ให้กลายเป็น Quest Instance เพื่อใช้งานในเกม
    /// </summary>
    public void InitializeStartingQuests()
    {
        foreach (QuestData data in startingQuestData)
        {
            if (data != null)
            {
                questToGive.Add(new Quest(data));
            }
        }
    }

    /// <summary>
    /// ค้นหาเควสในตัว NPC ที่ตรงกับสถานะ (Status) ที่ต้องการ
    /// </summary>
    public Quest CheckQuestList(QuestStatus status)
    {
        foreach (Quest quest in questToGive)
        {
            if (quest.Status == status)
            {
                return quest;
            }
        }
        return null;
    }

    /// <summary>
    /// ดึงเควสที่ผู้เล่นสามารถโต้ตอบได้ในขณะนี้ เรียงตามลำดับความสำคัญ (ส่งเควสก่อน -> รับเควสใหม่)
    /// </summary>
    public Quest GetInteractableQuest()
    {
        // ลำดับที่ 1: เช็คว่ามีเควสที่กำลังทำอยู่และต้องส่งหรือไม่?
        Quest inProgressQuest = CheckQuestList(QuestStatus.InProgress);
        if (inProgressQuest != null)
            return inProgressQuest;

        // ลำดับที่ 2: เช็คว่ามีเควสใหม่ให้รับหรือไม่?
        Quest newQuest = CheckQuestList(QuestStatus.New);
        if (newQuest != null)
            return newQuest;

        // ถ้าไม่มีทั้งสองสถานะ แสดงว่า Finish หรือ Reject ไปแล้ว คืนค่า null เพื่อไม่ให้เปิด UI
        return null;
    }
    #endregion
}