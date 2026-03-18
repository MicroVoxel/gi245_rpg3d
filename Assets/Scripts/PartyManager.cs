using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    [SerializeField] private List<Character> members = new List<Character>();
    public List<Character> Members { get { return members; } }

    [SerializeField] private List<Character> selectChars = new List<Character>();
    public List<Character> SelectChars { get { return selectChars; } }

    public static PartyManager instance;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        foreach (Character c in members)
        {
            c.charInit(VFXManager.instance, UIManager.instance);
        }

        //SelectSingleHero(0);

        for (int i = 0; i < members.Count; i++)
        {
            members[i].MagicSkills.Add(new Magic(VFXManager.instance.MagicDatas[0]));
            members[i].MagicSkills.Add(new Magic(VFXManager.instance.MagicDatas[1]));
            members[i].MagicSkills.Add(new Magic(VFXManager.instance.MagicDatas[2]));

            InventoryManager.instance.AddItem(members[i], 0);
            InventoryManager.instance.AddItem(members[i], 1);
            InventoryManager.instance.AddItem(members[i], 2);
            InventoryManager.instance.AddItem(members[i], 3);
            InventoryManager.instance.AddItem(members[i], 4);
            InventoryManager.instance.AddItem(members[i], 5);
            InventoryManager.instance.AddItem(members[i], 6);
            InventoryManager.instance.AddItem(members[i], 7);
            InventoryManager.instance.AddItem(members[i], 8);
            InventoryManager.instance.AddItem(members[i], 9);

        }

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

}
