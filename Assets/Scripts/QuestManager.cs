using System;
using UnityEngine;

/// <summary>
/// จัดการระบบเควสส่วนกลาง: ตรวจสอบสถานะ, การตอบโต้บทสนทนา, และการมอบรางวัล
/// </summary>
public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;

    // [NEW] Event สำหรับแจ้งเตือนเมื่อมีศัตรูตาย (Observer Pattern)
    // ส่งค่า ID ของศัตรูตัวนั้นๆ แนบมาด้วย
    public static Action<int> OnEnemyKilled;

    #region === DATA & STATE ===
    [Header("Database")]
    [SerializeField] private Npc[] npcPerson;
    public Npc[] NPCPerson { get => npcPerson; set => npcPerson = value; }

    [SerializeField] private QuestData[] questData;
    public QuestData[] QuestData { get => questData; set => questData = value; }

    [Header("Current Interaction")]
    [SerializeField] private Npc curNpc;
    public Npc CurNPC { get => curNpc; set => curNpc = value; }

    [SerializeField] private Quest curQuest;
    public Quest CurQuest { get => curQuest; set => curQuest = value; }
    #endregion

    #region === UNITY CALLBACKS ===
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
        foreach (Character character in npcPerson)
        {
            character.CharInit(UIManager.instance, InventoryManager.instance, PartyManager.instance);

            if (character is Npc npc)
            {
                npc.InitializeStartingQuests();
            }
        }
    }

    // [NEW] Subscribe Event เมื่อเปิดปิด Object เพื่อป้องกัน Memory Leak
    private void OnEnable()
    {
        OnEnemyKilled += UpdateKillCountQuests;
    }

    private void OnDisable()
    {
        OnEnemyKilled -= UpdateKillCountQuests;
    }
    #endregion

    #region === QUEST LOGIC & CHECKING ===
    /// <summary>
    /// อัปเดตจำนวนการฆ่า (ทำงานก็ต่อเมื่อ Event OnEnemyKilled ถูกเรียกเท่านั้น)
    /// </summary>
    private void UpdateKillCountQuests(int killedEnemyId)
    {
        if (PartyManager.instance == null) return;

        // วนลูปหาเฉพาะเควสในปาร์ตี้ที่กำลังทำอยู่ (InProgress) และเป็นเควสประเภท KillCount
        foreach (Quest q in PartyManager.instance.QuestList)
        {
            if (q.Status == QuestStatus.InProgress && q.Type == QuestType.KillCount)
            {
                // ตรวจสอบว่าศัตรูที่ตาย ตรงกับเป้าหมายของเควสหรือไม่
                if (q.TargetEnemyId == killedEnemyId)
                {
                    q.CurrentKillCount++;

                    // ป้องกันไม่ให้ Count เกินค่าเป้าหมาย (กันบั๊ก UI แสดงผล 11/10)
                    if (q.CurrentKillCount > q.RequiredKillCount)
                        q.CurrentKillCount = q.RequiredKillCount;

                    Debug.Log($"Quest '{q.QuestName}' Progress: {q.CurrentKillCount} / {q.RequiredKillCount}");
                }
            }
        }
    }

    public Quest CheckForQuest(Npc npc, QuestStatus status)
    {
        curNpc = npc;
        curQuest = npc.CheckQuestList(status);
        return curQuest;
    }

    public bool CheckIfFinishQuest()
    {
        if (curQuest == null) return false;

        bool success = false;
        switch (curQuest.Type)
        {
            case QuestType.Delivery:
                success = CheckItemToDelivery();
                break;
            case QuestType.KillCount: // [NEW] เพิ่มการเช็คเควส KillCount
                success = CheckKillCountTarget();
                break;
        }
        return success;
    }

    private bool CheckItemToDelivery()
    {
        if (curQuest == null) return false;
        return InventoryManager.instance.CheckPartyForItem(curQuest.QuestItemId);
    }

    private bool CheckKillCountTarget()
    {
        if (curQuest == null) return false;
        // เช็คว่าจำนวนที่ฆ่าได้ เท่ากับหรือมากกว่าที่ต้องการหรือยัง
        return curQuest.CurrentKillCount >= curQuest.RequiredKillCount;
    }
    #endregion

    #region === DIALOGUE HANDLING ===
    public bool CheckLastDialogue(int i)
    {
        if (curQuest == null || curQuest.QuestDialogue == null) return false;

        return i == curQuest.QuestDialogue.Length - 1;
    }

    public string NextDialogue(int i)
    {
        if (curQuest != null && curQuest.QuestDialogue != null && i < curQuest.QuestDialogue.Length)
        {
            return curQuest.QuestDialogue[i];
        }
        return "";
    }
    #endregion

    #region === QUEST ACTIONS ===
    public void RejectQuest()
    {
        if (curQuest != null)
        {
            curQuest.Status = QuestStatus.Reject;
        }
    }

    public void AcceptQuest()
    {
        if (curQuest != null)
        {
            curQuest.Status = QuestStatus.InProgress;
            PartyManager.instance.QuestList.Add(curQuest);
        }
    }

    public bool DeliverItem()
    {
        if (curQuest == null) return false;

        // เพิ่มการตรวจสอบประเภทเควส เพื่อให้ส่งของแค่เฉพาะเวลาเป็นเควส Delivery
        if (curQuest.Type == QuestType.Delivery)
        {
            return InventoryManager.instance.RemoveItemFromParty(curQuest.QuestItemId);
        }

        return true; // ถ้าไม่ใช่เควสส่งของ ถือว่าผ่านไปได้เลย (เพราะเช็ค Count มาก่อนหน้าแล้ว)
    }

    public bool NpcGiveReward(out Item rewardItem, out int rewardEXP)
    {
        rewardItem = null;
        rewardEXP = 0;

        if (curQuest == null || PartyManager.instance.SelectChars.Count == 0)
            return false;

        Character hero = PartyManager.instance.SelectChars[0];
        Item item = new Item(InventoryManager.instance.ItemData[curQuest.RewardItemId]);

        for (int i = 0; i < InventoryManager.INVENTORY_CAPACITY; i++)
        {
            if (hero.InventoryItems[i] == null)
            {
                hero.InventoryItems[i] = item;
                curQuest.Status = QuestStatus.Finish;

                rewardItem = item;
                rewardEXP = curQuest.RewardExp;

                PartyManager.instance.DistributeTotalExp(rewardEXP);

                return true;
            }
        }

        Debug.LogWarning("กระเป๋าเต็ม! ไม่สามารถรับรางวัลไอเทมได้");
        return false;
    }
    #endregion
}