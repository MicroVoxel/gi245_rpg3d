using UnityEngine;

/// <summary>
/// จัดการระบบเควสส่วนกลาง: ตรวจสอบสถานะ, การตอบโต้บทสนทนา, และการมอบรางวัล
/// </summary>
public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;

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
        // ลบการ Hardcode AddQuestToNPC ทิ้งไป
        // และให้ Manager สั่ง NPC แต่ละตัวจัดการตัวเอง (Encapsulation)
        foreach (Character character in npcPerson)
        {
            character.CharInit(UIManager.instance, InventoryManager.instance, PartyManager.instance);

            // เช็คว่าเป็น NPC หรือไม่ ถ้าใช่ให้โหลดเควสเริ่มต้นที่ตั้งค่าไว้ใน Inspector ของตัวมันเอง
            if (character is Npc npc)
            {
                npc.InitializeStartingQuests();
            }
        }
    }
    #endregion

    #region === QUEST LOGIC & CHECKING ===
    /// <summary>
    /// ดึงเควสจาก NPC ตามสถานะที่ต้องการ และเก็บเป็น State ปัจจุบันของ Manager
    /// </summary>
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
        }
        return success;
    }

    private bool CheckItemToDelivery()
    {
        if (curQuest == null) return false;
        return InventoryManager.instance.CheckPartyForItem(curQuest.QuestItemId);
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
        return InventoryManager.instance.RemoveItemFromParty(curQuest.QuestItemId);
    }

    /// <summary>
    /// มอบรางวัลไอเทมและ EXP หลังจบเควส
    /// </summary>
    public bool NpcGiveReward(out Item rewardItem, out int rewardEXP)
    {
        rewardItem = null;
        rewardEXP = 0;

        if (curQuest == null || PartyManager.instance.SelectChars.Count == 0)
            return false;

        Character hero = PartyManager.instance.SelectChars[0];
        Item item = new Item(InventoryManager.instance.ItemData[curQuest.RewardItemId]);

        // [แก้ไข] จำกัดการให้รางวัลเฉพาะในพื้นที่กระเป๋า 16 ช่องเท่านั้น
        for (int i = 0; i < InventoryManager.INVENTORY_CAPACITY; i++)
        {
            if (hero.InventoryItems[i] == null)
            {
                hero.InventoryItems[i] = item;
                curQuest.Status = QuestStatus.Finish;

                rewardItem = item;
                rewardEXP = curQuest.RewardExp;

                // แจก EXP เข้าปาร์ตี้อัตโนมัติ
                PartyManager.instance.DistributeTotalExp(rewardEXP);

                return true;
            }
        }

        Debug.LogWarning("กระเป๋าเต็ม! ไม่สามารถรับรางวัลไอเทมได้");
        return false;
    }
    #endregion
}