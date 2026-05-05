using UnityEngine;

public class Hero : Character
{
    [Header("Prefab ID")]
    [SerializeField] private int prefabId;
    public int PrefabID { get { return prefabId; } }

    #region === STATS (RPG) ===
    [Header("Progression")]

    [SerializeField] private int exp;
    [SerializeField] private int level;
    [SerializeField] private int nextExp;

    public int Exp { get { return exp; } set { exp = value; } }
    public int Level { get { return level; } set { level = value; } }
    public int NextExp { get { return nextExp; } set { nextExp = value; } }
    #endregion

    #region === ATTRIBUTES ===
    [Header("Attributes")]

    [SerializeField] private int strength;
    [SerializeField] private int dexterity;
    [SerializeField] private int constitution;
    [SerializeField] private int intelligence;
    [SerializeField] private int wisdom;
    [SerializeField] private int charisma;

    public int Strength { get { return strength; } set { strength = value; } }
    public int Dexterity { get { return dexterity; } set { dexterity = value; } }
    public int Constitution { get { return constitution; } set { constitution = value; } }
    public int Intelligence { get { return intelligence; } set { intelligence = value; } }
    public int Wisdom { get { return wisdom; } set { wisdom = value; } }
    public int Charisma { get { return charisma; } set { charisma = value; } }
    #endregion

    #region === UNITY CALLBACKS ===
    private void Update()
    {
        switch (state)
        {
            case CharState.Walk:
                WalkUpdate();
                break;

            case CharState.WalkToEnemy:
                WalkToEnemyUpdate();
                break;

            case CharState.Attack:
                AttackUpdate();
                break;

            case CharState.WalkToMagicCast:
                WalkToMagicCastUpadate();
                break;

            case CharState.WalkToNPC:
                WalkToNPCUpdate();
                break;
        }
    }
    #endregion

    #region === NPC INTERACTION ===
    protected void WalkToNPCUpdate()
    {
        if (curCharTarget == null) return;

        float distance = Vector3.Distance(transform.position, curCharTarget.transform.position);

        if (distance > 2f) return;

        navAgent.isStopped = true;
        SetState(CharState.Idle);

        Npc npc = curCharTarget.GetComponent<Npc>();
        if (npc != null)
        {
            if (npc.IsShopKeeper)
            {
                uiManager.PrepareShopPanel(npc, this);
                return;
            }

            Quest interactableQuest = npc.GetInteractableQuest();

            if (interactableQuest != null)
            {
                uiManager.PrepareDialogueBox(npc);
                return;
            }

            return;
        }

        Hero hero = curCharTarget.GetComponent<Hero>();
        if (hero != null)
        {
            uiManager.PrepareHeroJoinParty(hero);
            return;
        }
    }
    #endregion

    #region === INVENTORY ===
    public void SaveItemInInventory(Item item)
    {
        for (int i = 0; i < 16; i++)
        {
            if (InventoryItems[i] == null)
            {
                InventoryItems[i] = item;
                return;
            }
        }
    }
    #endregion

    #region === EXP & LEVEL ===
    public void ReceiveExp(int n)
    {
        exp += n;
        CheckLevel(exp);
    }

    private void CheckLevel(int exp)
    {
        nextExp = level * 30;

        if (exp >= nextExp)
        {
            level++;
            nextExp = level * 30;
            UpdateStat();

            switch (level)
            {
                case 5:
                    magicSkills.Add(new Magic(vfxManager.MagicDatas[0]));
                    uiManager.ShowMagicToggles();
                    break;
            }

        }
    }
    #endregion

    #region === STAT CALCULATION ===
    private void UpdateStat()
    {
        attackDamage++;
        defensePower++;
        maxHP++;

        if (strength >= Random.Range(1, 20))
        {
            attackDamage++;
        }

        if (dexterity >= Random.Range(1, 20))
        {
            defensePower++;
        }

        if (constitution >= Random.Range(1, 20))
        {
            maxHP++;
        }
    }
    #endregion
}