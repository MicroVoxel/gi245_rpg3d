using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    [SerializeField] private List<Character> members = new List<Character>();
    public List<Character> Members { get { return members; } }

    [SerializeField] private List<Character> selectChars = new List<Character>();
    public List<Character> SelectChars { get { return selectChars; } }

    [SerializeField] private List<Quest> questList = new List<Quest>();
    public List<Quest> QuestList { get { return questList; } }

    [SerializeField] private int partyMoney = 1000;
    public int PartyMoney { get { return partyMoney; } set { partyMoney = value; } }

    [SerializeField] private int totalExp;

    [SerializeField] protected HeroData[] heroData;
    public HeroData[] HeroData { get { return heroData; } }

    public static PartyManager instance;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        //foreach (Character c in members)
        //{
        //    c.CharInit(VFXManager.instance, UIManager.instance, InventoryManager.instance,this);
        //}

        SelectSingleHero(0);

        //for (int i = 0; i < members.Count; i++)
        //{
        //    members[i].MagicSkills.Add(new Magic(VFXManager.instance.MagicDatas[0]));
        //    members[i].MagicSkills.Add(new Magic(VFXManager.instance.MagicDatas[1]));
        //    members[i].MagicSkills.Add(new Magic(VFXManager.instance.MagicDatas[2]));

        //    InventoryManager.instance.AddItem(members[i], 0);
        //    InventoryManager.instance.AddItem(members[i], 1);
        //    InventoryManager.instance.AddItem(members[i], 2);
        //    InventoryManager.instance.AddItem(members[i], 3);
        //    InventoryManager.instance.AddItem(members[i], 4);
        //    InventoryManager.instance.AddItem(members[i], 5);
        //    InventoryManager.instance.AddItem(members[i], 6);
        //    InventoryManager.instance.AddItem(members[i], 7);
        //    InventoryManager.instance.AddItem(members[i], 8);
        //    InventoryManager.instance.AddItem(members[i], 9);
        //    InventoryManager.instance.AddItem(members[i], 10);

        //}

        UIManager.instance.ShowMagicToggles();

    }
    void Update()
    {

    }

    public void SelectSingleHero(int i)
    {
        foreach (Character c in selectChars)
        {
            c.ToggleRingSelection(false);
        }

        selectChars.Clear();

        selectChars.Add(members[i]);
        selectChars[0].ToggleRingSelection(true);
    }

    public void HeroSelectMagicSkill(int i)
    {
        if (selectChars.Count <= 0 ||
            i >= selectChars[0].MagicSkills.Count)
        { return; }

        selectChars[0].IsMagicMode = true;
        selectChars[0].CurMagicCast = selectChars[0].MagicSkills[i];
    }

    public int FindIndexFromClass(Character hero)
    {
        for (int i = 0; i < members.Count; i++)
        {
            if (members[i] == hero)
            {
                return i;
            }
        }
        return 0;
    }

    public void SelectSingleHeroByToggle(int i)
    {
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
        //if (selectChars.Count <= 1)
        //{
        //    UIManager.instance.ToggleAvatar[i].isOn = true;
        //    return;
        //}

        if (selectChars.Contains(members[i]))
        {
            selectChars.Remove(members[i]);
            members[i].ToggleRingSelection(false);
        }
    }

    public void RemoveHeroFromParty(int id)
    {
        if (id == -1 || id == 0) return;

        if (selectChars.Contains(members[id]))
        {
            selectChars.Remove(members[id]);
        }

        members.Remove(members[id]);
    }

    public void DistributeTotalExp(int n)
    {
        totalExp = n;
        int eachHeroExp = totalExp / members.Count;

        foreach (Hero hero in members)
        {
            hero.ReceiveExp(eachHeroExp);
        }
    }

    public bool HeroJoinParty(Character hero)
    {
        if (members.Count >= 6)
        {
            return false;
        }

        hero.CharInit(VFXManager.instance, UIManager.instance, InventoryManager.instance, this);

        members.Add(hero);
        return true;
    }

    public void SaveAllHeroData()
    {
        for (int i = 0; i < members.Count; i++)
        {
            Hero hero = (Hero)members[i];
            heroData[i].prefabId = hero.PrefabID;
            heroData[i].curHp = hero.CurHp;

            for (int j = 0; j < hero.MagicSkills.Count; j++)
            {
                heroData[i].magicIds[j] = hero.MagicSkills[j].ID;
            }

            for(int k = 0; k < hero.InventoryItems.Length; k++)
            {
                if (hero.InventoryItems[k] == null)
                {
                    heroData[i].inventoryItemIds[k] = -1;
                }
                else
                {
                    heroData[i].inventoryItemIds[k] = hero.InventoryItems[k].ID;
                }
            }

            heroData[i].attackDamage = hero.AttackDamage;
            heroData[i].defensePower = hero.DefensePower;
            heroData[i].exp = hero.Exp;
            heroData[i].level = hero.Level;
            heroData[i].nextExp = hero.NextExp;
        }
    }

    public void LoadAllHeroData()
    {
        int enterId = Settings.enterPointId;
        Vector3 pos = MapManager.instance.EnterPoints[enterId].position;

        for (int i = 0; i < Settings.partyCount; i++)
        {
            GameObject heroObj = Instantiate(
                GameManager.instance.HeroPrefabs[heroData[i].prefabId],
                pos,
                Quaternion.identity
                );

            if(i == 0)
            {
                heroObj.gameObject.tag = "Player";
            }

            Hero hero = heroObj.GetComponent<Hero>();
            hero.CharInit(VFXManager.instance, UIManager.instance, InventoryManager.instance, this);
            hero.CurHp = heroData[i].curHp;

            for (int j = 0; j < heroData[i].magicIds.Count; j++)
            {
                int magicId = heroData[i].magicIds[j];
                hero.MagicSkills.Add(new Magic(VFXManager.instance.MagicDatas[magicId]));
            }

            for (int k = 0; k < heroData[i].inventoryItemIds.Length; k++)
            {
                int itemId = heroData[i].inventoryItemIds[k];
                if(itemId != -1)
                {
                    hero.InventoryItems[k] = new Item(InventoryManager.instance.ItemData[itemId]);
                }
            }

            hero.AttackDamage = heroData[i].attackDamage;
            hero.DefensePower = heroData[i].defensePower;
            hero.Exp = heroData[i].exp;
            hero.Level = heroData[i].level;
            hero.NextExp = heroData[i].nextExp;
            members.Add(hero);
        }
    }

}
