using UnityEngine;

[CreateAssetMenu(fileName = "QuestData", menuName = "Scriptable Objects/QuestData")]
public class QuestData : ScriptableObject
{
    [Header("Basic Info")]
    public int questId;
    public QuestType type;
    public QuestStatus status;
    public string questName;
    [TextArea(3, 5)] public string questDetail;

    [Header("Delivery Quest Settings")]
    public int questItemId;

    [Header("Kill Count Quest Settings")]
    [Tooltip("ใส่ PrefabID หรือ ID ของศัตรูเป้าหมาย")]
    public int targetEnemyId;
    [Tooltip("จำนวนที่ต้องฆ่าให้ครบ")]
    public int requiredKillCount;

    [Header("Dialogues")]
    public string[] questDialogue;
    public string[] answerNext;
    public string anwerAccept;
    public string answerReject;

    [Header("Progress Dialogues")]
    public string questionInProgress;
    public string answerFinish;
    public string answerNotFinish;

    [Header("Rewards")]
    public int rewardItemId;
    public int rewardExp;
}