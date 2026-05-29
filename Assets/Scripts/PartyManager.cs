using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// จัดการสมาชิก Party ทั้งหมด: การเลือก, EXP, Save/Load, Formation
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

    public List<Character> Members => members;
    public List<Character> SelectChars => selectChars;
    public List<Quest> QuestList => questList;
    public HeroData[] HeroData => heroData;

    public int PartyMoney
    {
        get => partyMoney;
        set => partyMoney = value;
    }
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
        SelectSingleHero(0);
        if (UIManager.instance != null)
        {
            UIManager.instance.ShowMagicToggles();
        }
    }
    #endregion

    #region === SELECTION ===
    public void SelectSingleHero(int i)
    {
        if (i < 0 || i >= members.Count || members[i] == null) return;

        foreach (Character c in selectChars)
        {
            if (c != null)
            {
                c.ToggleRingSelection(false);
            }
        }

        selectChars.Clear();
        selectChars.Add(members[i]);
        members[i].ToggleRingSelection(true);

        if (UIManager.instance != null)
        {
            UIManager.instance.ShowMagicToggles();
        }
    }

    public void ToggleHeroSelectionMulti(int i)
    {
        if (i < 0 || i >= members.Count || members[i] == null) return;

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

        if (UIManager.instance != null)
        {
            UIManager.instance.ShowMagicToggles();
        }
    }

    public void SelectSingleHeroByToggle(int i)
    {
        if (i < 0 || i >= members.Count || members[i] == null) return;

        if (selectChars.Contains(members[i]))
        {
            members[i].ToggleRingSelection(true);
        }
        else
        {
            selectChars.Add(members[i]);
            members[i].ToggleRingSelection(true);
        }

        if (UIManager.instance != null)
        {
            UIManager.instance.ShowMagicToggles();
        }
    }

    public void UnSelectSingleHeroByToggle(int i)
    {
        if (i < 0 || i >= members.Count || members[i] == null) return;
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

        if (UIManager.instance != null)
        {
            UIManager.instance.MapToggleAvatar();
        }
    }
    #endregion

    #region === MAGIC ===
    public void HeroSelectMagicSkill(int i)
    {
        if (selectChars.Count <= 0 || selectChars[0] == null) return;
        if (i < 0 || i >= selectChars[0].MagicSkills.Count) return;

        selectChars[0].IsMagicMode = true;
        selectChars[0].CurMagicCast = selectChars[0].MagicSkills[i];
    }
    #endregion

    #region === PARTY MANAGEMENT ===
    public bool HeroJoinParty(Character hero)
    {
        if (hero == null) return false;
        if (members.Count >= 6) return false;
        if (members.Contains(hero)) return false;

        hero.CharInit(UIManager.instance, InventoryManager.instance, this);

        if (!hero.CompareTag("Player"))
        {
            hero.gameObject.tag = "Hero";
        }

        members.Add(hero);
        return true;
    }

    public void RemoveHeroFromParty(int id)
    {
        if (id <= 0 || id >= members.Count) return;

        if (selectChars.Contains(members[id]))
        {
            if (members[id] != null)
            {
                members[id].ToggleRingSelection(false);
            }
            selectChars.Remove(members[id]);
        }

        members.RemoveAt(id);
    }
    #endregion

    #region === EXP ===
    public void DistributeTotalExp(int amount)
    {
        if (members.Count == 0) return;

        totalExp = amount;
        int eachHeroExp = totalExp / members.Count;

        foreach (Character character in members)
        {
            if (character is Hero hero)
            {
                hero.ReceiveExp(eachHeroExp);
            }
        }
    }
    #endregion

    #region === SAVE / LOAD ===
    /// <summary>
    /// บันทึกข้อมูล Hero ทุกคนลง HeroData Array
    /// </summary>
    public void SaveAllHeroData()
    {
        for (int i = 0; i < members.Count; i++)
        {
            if (members[i] is Hero hero)
            {
                if (i >= heroData.Length || heroData[i] == null) continue;

                heroData[i].prefabId = hero.PrefabID;
                heroData[i].curHp = hero.CurHp;
                heroData[i].maxHP = hero.MaxHP;

                // =====================================================================
                // [BUG FIX] Magic Skills — เดิมใช้ if (j < heroData[i].magicIds.Count)
                // ซึ่งไม่ทำงานเลยเพราะ magicIds เริ่มต้นเป็น List ว่าง (Count=0)
                // ทำให้เงื่อนไขเป็น false ตลอด → ไม่มี magic ถูก save
                // แก้: Clear แล้ว Add ใหม่ทั้งหมด
                // =====================================================================
                heroData[i].magicIds.Clear();
                foreach (Magic magic in hero.MagicSkills)
                {
                    heroData[i].magicIds.Add(magic.ID);
                }

                // Inventory
                for (int k = 0; k < InventoryManager.INVENTORY_CAPACITY; k++)
                {
                    if (k < heroData[i].inventoryItemIds.Length)
                    {
                        heroData[i].inventoryItemIds[k] = hero.InventoryItems[k]?.ID ?? -1;
                    }
                }

                heroData[i].mainWeaponId = hero.MainWeapon != null ? hero.MainWeapon.ID : -1;
                heroData[i].shieldId = hero.Shield != null ? hero.Shield.ID : -1;

                heroData[i].attackDamage = hero.AttackDamage;
                heroData[i].baseDefense = hero.BaseDefense;
                heroData[i].exp = hero.Exp;
                heroData[i].level = hero.Level;
                heroData[i].nextExp = hero.NextExp;

                heroData[i].strength = hero.Strength;
                heroData[i].dexterity = hero.Dexterity;
                heroData[i].constitution = hero.Constitution;
                heroData[i].intelligence = hero.Intelligence;
                heroData[i].wisdom = hero.Wisdom;
                heroData[i].charisma = hero.Charisma;
            }
        }
    }

    /// <summary>
    /// โหลดข้อมูล Hero จาก HeroData Array
    /// </summary>
    public void LoadAllHeroData()
    {
        if (MapManager.instance == null || GameManager.instance == null) return;

        int enterId = Settings.enterPointId;
        Vector3 spawnPos = MapManager.instance.EnterPoints[enterId].position;

        for (int i = 0; i < Settings.partyCount; i++)
        {
            if (i >= heroData.Length || heroData[i] == null) continue;

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
            hero.BaseDefense = heroData[i].baseDefense;
            hero.Exp = heroData[i].exp;
            hero.Level = heroData[i].level;
            hero.NextExp = heroData[i].nextExp;

            hero.Strength = heroData[i].strength;
            hero.Dexterity = heroData[i].dexterity;
            hero.Constitution = heroData[i].constitution;
            hero.Intelligence = heroData[i].intelligence;
            hero.Wisdom = heroData[i].wisdom;
            hero.Charisma = heroData[i].charisma;

            // =====================================================================
            // [BUG FIX] Magic Skills Load
            // เดิม: Clear magicSkills แล้วโหลดจาก magicIds ที่ว่าง → magic หายหมด
            // แก้:  Clear แล้วโหลด ถูกต้องแต่ต้องมีข้อมูล save ที่ถูกต้องก่อน (fix ฝั่ง Save ด้วย)
            //
            // [BUG FIX 2] ค้นหา MagicData ด้วย id field แทนการใช้ index โดยตรง
            // เดิม: MagicDatas[magicId] → อาจเข้าถึง element ผิดถ้า id ไม่ตรงกับ index
            // แก้:  หา MagicData ที่มี .id == magicId เพื่อความถูกต้อง
            // =====================================================================
            hero.MagicSkills.Clear();

            if (VFXManager.instance != null)
            {
                foreach (int magicId in heroData[i].magicIds)
                {
                    // ค้นหา MagicData ด้วย id field ไม่ใช่ array index
                    MagicData foundData = System.Array.Find(
                        VFXManager.instance.MagicDatas,
                        d => d != null && d.id == magicId
                    );

                    if (foundData != null)
                    {
                        hero.MagicSkills.Add(new Magic(foundData));
                    }
                    else
                    {
                        Debug.LogWarning($"[PartyManager] LoadAllHeroData: ไม่พบ MagicData ที่มี id={magicId} — ข้ามสกิลนี้ไป");
                    }
                }
            }

            // Load Inventory
            for (int k = 0; k < heroData[i].inventoryItemIds.Length; k++)
            {
                int itemId = heroData[i].inventoryItemIds[k];
                if (itemId != -1 && k < InventoryManager.INVENTORY_CAPACITY && InventoryManager.instance != null)
                {
                    if (itemId >= 0 && itemId < InventoryManager.instance.ItemData.Length)
                    {
                        hero.InventoryItems[k] = new Item(InventoryManager.instance.ItemData[itemId]);
                    }
                }
            }

            // Load Equipment
            if (heroData[i].mainWeaponId != -1 && InventoryManager.instance != null)
            {
                int weaponId = heroData[i].mainWeaponId;
                if (weaponId >= 0 && weaponId < InventoryManager.instance.ItemData.Length)
                {
                    Item weapon = new Item(InventoryManager.instance.ItemData[weaponId]);
                    hero.EquipWeapon(weapon);
                }
            }

            if (heroData[i].shieldId != -1 && InventoryManager.instance != null)
            {
                int shieldId = heroData[i].shieldId;
                if (shieldId >= 0 && shieldId < InventoryManager.instance.ItemData.Length)
                {
                    Item shield = new Item(InventoryManager.instance.ItemData[shieldId]);
                    hero.EquipShield(shield);
                }
            }

            members.Add(hero);
        }
    }
    #endregion
}