using UnityEngine;
using System.Collections.Generic;

public class Npc : Character
{
    [SerializeField] private bool isShopKeeper;
    public bool IsShopKeeper {  get { return isShopKeeper; } }

    [SerializeField] private List<Item> shopItems = new List<Item>();
    public List<Item> ShopItems { get { return shopItems; } set { shopItems = value; } }

    [SerializeField] private int npcMoney = 3000;
    public int NpcMoney {  get { return npcMoney; } set { npcMoney = value; } }

    [SerializeField] private List<Quest> questToGive = new List<Quest>();
    public List<Quest> QuestToGive { get { return questToGive; } set { questToGive = value; } }

    public Quest CheckQuestList(QuestStatus status)
    {
        foreach (Quest quest in QuestToGive)
        {
            if (quest.Status == status)
            {
                return quest;
            }
        }
        return null;
    }

    public Quest GetInteractableQuest()
    {
        // ลำดับที่ 1: เช็คว่ามีเควสที่กำลังทำอยู่และต้องส่งหรือไม่?
        Quest inProgressQuest = CheckQuestList(QuestStatus.InProgress);
        if (inProgressQuest != null) return inProgressQuest;

        // ลำดับที่ 2: เช็คว่ามีเควสใหม่ให้รับหรือไม่?
        Quest newQuest = CheckQuestList(QuestStatus.New);
        if (newQuest != null) return newQuest;

        // ถ้าไม่มีทั้งสองสถานะ แสดงว่า Finish หรือ Reject ไปแล้ว คืนค่า null เพื่อไม่ให้เปิด UI
        return null;
    }


}
