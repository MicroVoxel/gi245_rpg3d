using UnityEngine;

[CreateAssetMenu(fileName = "QuestData", menuName = "Scriptable Objects/QuestData")]
public class QuestData : ScriptableObject
{
    public int questId;
    public QuestType type;
    public QuestStatus status;
    public string 
        questName,
        questDetail;
    public int questItemId;
    public string[]
        questDialogue,
        answerNext;
    public string
        anwerAccept,
        answerReject;
    public int
        rewardItemId,
        rewardExp;
    public string
        questionInProgress,
        answerFinish,
        answerNotFinish;

}
