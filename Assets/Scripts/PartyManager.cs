using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// จัดการสมาชิก Party ทั้งหมด: การเลือก, EXP, Save/Load, Formation
/// ปรับปรุงให้รองรับฐานข้อมูล HeroData ใหม่ที่ใช้ baseDefense แทน defensePower
/// </summary>
public class PartyManager : MonoBehaviour
{
    public static PartyManager instance;

    #region === PARTY DATA ===
    [SerializeField] private List<Character> members = new List<Character>();
    [SerializeField] private List<Character> selectChars = new List<Character>();
    [SerializeField] private List<Quest> questList = new List<Quest>();

    [SerializeField] private int partyMoney = 1000;
    [SerializeField] private int totalExp;

    [SerializeField] protected HeroData[] heroData;

    public List<Character> Members { get { return members; } }
    public List<Character> SelectChars { get { return selectChars; } }
    public List<Quest> QuestList { get { return questList; } }
    public HeroData[] HeroData { get { return heroData; } }

    public int PartyMoney
    {
        get { return partyMoney; }
        set { partyMoney = value; }
    }
    #endregion

    #region === UNITY CALLBACKS ===
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        SelectSingleHero(0);
        UIManager.instance.ShowMagicToggles();
    }
    #endregion

    #region === SELECTION ===
    public void SelectSingleHero(int i)
    {
        if (i < 0 || i >= members.Count) return;

        foreach (Character c in selectChars)
            c.ToggleRingSelection(false);

        selectChars.Clear();
        selectChars.Add(members[i]);
        members[i].ToggleRingSelection(true);

        UIManager.instance.ShowMagicToggles();
    }

    public void ToggleHeroSelectionMulti(int i)
    {
        if (i < 0 || i >= members.Count) return;

        Character targetHero = members[i];

        if (selectChars.Contains(targetHero))
        {
            if (selectChars.Count > 1)
            {
                selectChars.Remove(targetHero);
                targetHero.ToggleRingSelection(false);
            }
        }
        else
        {
            selectChars.Add(targetHero);
            targetHero.ToggleRingSelection(true);
        }

        UIManager.instance.ShowMagicToggles();
    }

    public void SelectSingleHeroByToggle(int i)
    {
        if (i < 0 || i >= members.Count) return;

        if (selectChars.Contains(members[i]))
        {
            members[i].ToggleRingSelection(true);
            UIManager.instance.ShowMagicToggles();
        }
        else
        {
            selectChars.Add(members[i]);
            members[i].ToggleRingSelection(true);
            UIManager.instance.ShowMagicToggles();
        }
    }

    public void UnSelectSingleHeroByToggle(int i)
    {
        if (i < 0 || i >= members.Count) return;
        if (!selectChars.Contains(members[i])) return;

        selectChars.Remove(members[i]);
        members[i].ToggleRingSelection(false);
    }

    public int FindIndexFromClass(Character hero)
    {
        for (int i = 0; i < members.Count; i++)
        {
            if (members[i] == hero) return i;
        }
        return -1;
    }

    public bool IsMember(Character character)
    {
        return members.Contains(character);
    }
    #endregion

    #region === PARTY FORMATION (SWAP) ===
    public void SwapPartyMembers(int indexA, int indexB)
    {
        if (indexA == indexB) return;
        if (indexA < 0 || indexA >= members.Count || indexB < 0 || indexB >= members.Count) return;

        Character temp = members[indexA];
        members[indexA] = members[indexB];
        members[indexB] = temp;

        UIManager.instance.MapToggleAvatar();
    }
    #endregion

    #region === MAGIC ===
    public void HeroSelectMagicSkill(int i)
    {
        if (selectChars.Count <= 0) return;
        if (i >= selectChars[0].MagicSkills.Count) return;

        selectChars[0].IsMagicMode = true;
        selectChars[0].CurMagicCast = selectChars[0].MagicSkills[i];
    }
    #endregion

    #region === PARTY MANAGEMENT ===
    public bool HeroJoinParty(Character hero)
    {
        if (members.Count >= 6) return false;
        if (members.Contains(hero)) return false;

        hero.CharInit(UIManager.instance, InventoryManager.instance, this);

        if (!hero.CompareTag("Player"))
            hero.gameObject.tag = "Hero";

        members.Add(hero);
        return true;
    }

    public void RemoveHeroFromParty(int id)
    {
        if (id <= 0 || id >= members.Count) return;

        if (selectChars.Contains(members[id]))
        {
            members[id].ToggleRingSelection(false);
            selectChars.Remove(members[id]);
        }

        members.RemoveAt(id);
    }
    #endregion

    #region === EXP ===
    public void DistributeTotalExp(int amount)
    {
        totalExp = amount;
        int eachHeroExp = totalExp / members.Count;

        foreach (Hero hero in members)
            hero.ReceiveExp(eachHeroExp);
    }
    #endregion

    #region === SAVE / LOAD ===
    /// <summary>
    /// บันทึกข้อมูล Hero ทุกคนลง HeroData Array โดยใช้ baseDefense
    /// </summary>
    public void SaveAllHeroData()
    {
        for (int i = 0; i < members.Count; i++)
        {
            Hero hero = (Hero)members[i];

            heroData[i].prefabId = hero.PrefabID;
            heroData[i].curHp = hero.CurHp;
            heroData[i].maxHP = hero.MaxHP;

            // Magic Skills
            for (int j = 0; j < hero.MagicSkills.Count; j++)
                heroData[i].magicIds[j] = hero.MagicSkills[j].ID;

            // Inventory
            for (int k = 0; k < hero.InventoryItems.Length; k++)
                heroData[i].inventoryItemIds[k] = hero.InventoryItems[k]?.ID ?? -1;

            heroData[i].attackDamage = hero.AttackDamage;
            heroData[i].baseDefense = hero.BaseDefense; // อัปเดตการเก็บข้อมูล Base Defense
            heroData[i].exp = hero.Exp;
            heroData[i].level = hero.Level;
            heroData[i].nextExp = hero.NextExp;

            // เก็บค่า Attributes
            heroData[i].strength = hero.Strength;
            heroData[i].dexterity = hero.Dexterity;
            heroData[i].constitution = hero.Constitution;
            heroData[i].intelligence = hero.Intelligence;
            heroData[i].wisdom = hero.Wisdom;
            heroData[i].charisma = hero.Charisma;
        }
    }

    /// <summary>
    /// โหลดข้อมูล Hero จาก HeroData Array
    /// </summary>
    public void LoadAllHeroData()
    {
        int enterId = Settings.enterPointId;
        Vector3 spawnPos = MapManager.instance.EnterPoints[enterId].position;

        for (int i = 0; i < Settings.partyCount; i++)
        {
            GameObject heroObj = Instantiate(
                GameManager.instance.HeroPrefabs[heroData[i].prefabId],
                spawnPos,
                Quaternion.identity
            );

            heroObj.tag = (i == 0) ? "Player" : "Hero";

            Hero hero = heroObj.GetComponent<Hero>();
            hero.CharInit(UIManager.instance, InventoryManager.instance, this);

            // Stats
            hero.CurHp = heroData[i].curHp;
            hero.AttackDamage = heroData[i].attackDamage;
            hero.BaseDefense = heroData[i].baseDefense; // โหลดค่า Base Defense
            hero.Exp = heroData[i].exp;
            hero.Level = heroData[i].level;
            hero.NextExp = heroData[i].nextExp;

            // โหลด Attributes
            hero.Strength = heroData[i].strength;
            hero.Dexterity = heroData[i].dexterity;
            hero.Constitution = heroData[i].constitution;
            hero.Intelligence = heroData[i].intelligence;
            hero.Wisdom = heroData[i].wisdom;
            hero.Charisma = heroData[i].charisma;

            // Magic Skills
            for (int j = 0; j < heroData[i].magicIds.Count; j++)
            {
                int magicId = heroData[i].magicIds[j];
                hero.MagicSkills.Add(new Magic(VFXManager.instance.MagicDatas[magicId]));
            }

            // Inventory
            for (int k = 0; k < heroData[i].inventoryItemIds.Length; k++)
            {
                int itemId = heroData[i].inventoryItemIds[k];
                if (itemId != -1)
                    hero.InventoryItems[k] = new Item(InventoryManager.instance.ItemData[itemId]);
            }

            members.Add(hero);
        }
    }
    #endregion
}