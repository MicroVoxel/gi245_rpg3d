using UnityEngine;

public enum QuestType
{
    Delivery,
    KillCount
}

public enum QuestStatus
{
    New,
    InProgress,
    Finish,
    Reject
}

[System.Serializable]
public class Quest
{
    [SerializeField] private int questId;
    public int QuestID => questId;

    [SerializeField] private QuestType type;
    public QuestType Type => type;

    [SerializeField] private QuestStatus status;
    public QuestStatus Status { get => status; set => status = value; }

    [SerializeField] private string questName;
    public string QuestName => questName;

    [SerializeField] private string questDetail;
    public string QuestDetail => questDetail;

    [Header("Delivery Requirement")]
    [SerializeField] private int questItemId;
    public int QuestItemId => questItemId;

    [Header("Kill Count Requirement")]
    [SerializeField] private int targetEnemyId;
    public int TargetEnemyId => targetEnemyId;

    [SerializeField] private int requiredKillCount;
    public int RequiredKillCount => requiredKillCount;

    [SerializeField] private int currentKillCount;
    public int CurrentKillCount { get => currentKillCount; set => currentKillCount = value; }

    [Header("Dialogues")]
    [SerializeField] private string[] questDialogue;
    public string[] QuestDialogue => questDialogue;

    [SerializeField] private string[] answerNext;
    public string[] AnswerNext => answerNext;

    [SerializeField] private string answerAccept;
    public string AnswerAccept => answerAccept;

    [SerializeField] private string answerReject;
    public string AnswerReject => answerReject;

    [SerializeField] private string questionInProgress;
    public string QuestionInProgress => questionInProgress;

    [SerializeField] private string answerFinish;
    public string AnswerFinish => answerFinish;

    [SerializeField] private string answerNotFinish;
    public string AnswerNotFinish => answerNotFinish;

    [Header("Rewards")]
    [SerializeField] private int rewardItemId;
    public int RewardItemId => rewardItemId;

    [SerializeField] private int rewardExp;
    public int RewardExp => rewardExp;

    public Quest(QuestData questData)
    {
        questId = questData.questId;
        type = questData.type;
        status = questData.status;
        questName = questData.questName;
        questDetail = questData.questDetail;

        questItemId = questData.questItemId;

        targetEnemyId = questData.targetEnemyId;
        requiredKillCount = questData.requiredKillCount;
        currentKillCount = 0; // เริ่มต้นที่ 0 เสมอ

        questDialogue = questData.questDialogue;
        answerNext = questData.answerNext;
        answerAccept = questData.anwerAccept;
        answerReject = questData.answerReject;
        rewardItemId = questData.rewardItemId;
        rewardExp = questData.rewardExp;
        questionInProgress = questData.questionInProgress;
        answerFinish = questData.answerFinish;
        answerNotFinish = questData.answerNotFinish;
    }
}