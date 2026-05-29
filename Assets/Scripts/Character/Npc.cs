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

    [SerializeField] private List<ItemData> shopItems = new List<ItemData>();

    public bool IsShopKeeper => isShopKeeper;
    public int NpcMoney { get => npcMoney; set => npcMoney = value; }

    public List<ItemData> ShopItems { get => shopItems; set => shopItems = value; }
    #endregion

    #region === QUEST SYSTEM ===
    [Header("Quest Settings")]
    [SerializeField] private List<QuestData> startingQuestData = new List<QuestData>();
    [SerializeField] private List<Quest> questToGive = new List<Quest>();

    public List<Quest> QuestToGive { get => questToGive; set => questToGive = value; }

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

    public Quest CheckQuestList(QuestStatus status)
    {
        foreach (Quest quest in questToGive)
        {
            if (quest.Status == status)
                return quest;
        }
        return null;
    }

    public Quest GetInteractableQuest()
    {
        Quest inProgressQuest = CheckQuestList(QuestStatus.InProgress);
        if (inProgressQuest != null) return inProgressQuest;

        Quest newQuest = CheckQuestList(QuestStatus.New);
        if (newQuest != null) return newQuest;

        return null;
    }
    #endregion
}