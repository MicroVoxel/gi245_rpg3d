using UnityEngine;

/// <summary>
/// Hero: ตัวละครที่ผู้เล่นควบคุม มี Progression (EXP/Level) และ Attributes
/// พร้อมระบบคัดกรองขอบเขตช่องเก็บของป้องกันไอเทมไหลเข้าสวมใส่ (Boundary Guard)
/// </summary>
public class Hero : Character
{
    #region === PREFAB ID ===
    [Header("Prefab ID")]
    [SerializeField] private int prefabId;
    public int PrefabID { get { return prefabId; } }
    #endregion

    #region === PROGRESSION ===
    [Header("Progression")]
    [SerializeField] private int exp;
    [SerializeField] private int level;
    [SerializeField] private int nextExp;

    public int Exp { get { return exp; } set { exp = value; } }
    public int Level { get { return level; } set { level = value; } }
    public int NextExp { get { return nextExp; } set { nextExp = value; } }

    private const int MAX_LEVEL = 20;
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

    #region === HERO SOUND EFFECTS ===
    [Header("Hero Specific SFX")]
    [Tooltip("ใส่ Index ของเสียงตอนเลเวลอัป")]
    [SerializeField] private int levelUpSfxIndex = -1;
    #endregion

    #region === UNITY CALLBACKS ===
    private void Update()
    {
        switch (state)
        {
            case CharState.Walk: WalkUpdate(); break;
            case CharState.WalkToEnemy: WalkToEnemyUpdate(); break;
            case CharState.Attack: AttackUpdate(); break;
            case CharState.WalkToMagicCast: WalkToMagicCastUpadate(); break;
            case CharState.WalkToNPC: WalkToNPCUpdate(); break;
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
            HandleNpcInteraction(npc);
            curCharTarget = null;
            return;
        }

        Hero hero = curCharTarget.GetComponent<Hero>();
        if (hero != null)
        {
            HandleHeroInteraction(hero);
            curCharTarget = null;
        }
    }

    private void HandleNpcInteraction(Npc npc)
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
        }
    }

    private void HandleHeroInteraction(Hero hero)
    {
        if (partyManager.IsMember(hero)) return;
        if (partyManager.Members.Count >= 6) return;

        uiManager.PrepareHeroJoinParty(hero);
    }
    #endregion

    #region === INVENTORY ===
    /// <summary>
    /// บันทึกไอเทมเข้ากระเป๋าจากการซื้อขายหรือสปอว์น 
    /// [CRITICAL FIX]: บังคับตรวจเช็คจำกัดวงที่ INVENTORY_CAPACITY (16 ช่องแรก) เท่านั้น 
    /// และเพิ่มเงื่อนไขป้องกันการแสกนทะลุเกินความยาวจริงของอาเรย์อย่างสมบูรณ์แบบ
    /// </summary>
    public void SaveItemInInventory(Item item)
    {
        for (int i = 0; i < InventoryManager.INVENTORY_CAPACITY; i++)
        {
            if (i >= InventoryItems.Length) break;

            if (InventoryItems[i] == null)
            {
                InventoryItems[i] = item;
                return;
            }
        }
        Debug.LogWarning($"Inventory full! Could not add {item.ItemName}.");
    }
    #endregion

    #region === EXP & LEVEL ===
    public void ReceiveExp(int amount)
    {
        if (level >= MAX_LEVEL) return;

        exp += amount;
        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        nextExp = level * 30;

        bool hasLeveledUp = false;

        while (exp >= nextExp && level < MAX_LEVEL)
        {
            level++;
            nextExp = level * 30;
            UpdateStats();
            OnLevelUp(level);
            hasLeveledUp = true;
        }

        if (hasLeveledUp)
        {
            PlayCharacterSFX(levelUpSfxIndex);
        }

        if (level >= MAX_LEVEL)
        {
            exp = nextExp;
        }
    }

    private void OnLevelUp(int newLevel)
    {
        Magic magic;
        switch (newLevel)
        {
            case 5:
                if (MyActions.onCreateMagic != null)
                {
                    magic = MyActions.onCreateMagic(0);
                    magicSkills.Add(magic);
                    uiManager.ShowMagicToggles();
                }
                break;
            case 10:
                if (MyActions.onCreateMagic != null)
                {
                    magic = MyActions.onCreateMagic(1);
                    magicSkills.Add(magic);
                    uiManager.ShowMagicToggles();
                }
                break;
        }
    }
    #endregion

    #region === STAT CALCULATION ===
    private void UpdateStats()
    {
        attackDamage++;
        maxHP++;

        baseDefense++;

        if (strength >= Random.Range(1, 20)) attackDamage++;
        if (dexterity >= Random.Range(1, 20)) baseDefense++;
        if (constitution >= Random.Range(1, 20)) maxHP++;
    }
    #endregion
}