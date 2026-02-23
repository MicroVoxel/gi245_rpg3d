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
            members[i].MagicSkills.Add(new Magic(0, "Fire", 10f, 100, 3f, 1.5f, 0, 1));
            members[i].MagicSkills.Add(new Magic(1, "Glow", 10f, 150, 3f, 1.5f, 0, 2));
            members[i].MagicSkills.Add(new Magic(2, "Ice", 10f, 150, 3f, 1.5f, 0, 3));
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
