using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    [SerializeField] private List<Character> members = new List<Character>();
    public List<Character> Members { get { return members; } }

    [SerializeField] private List<Character> selectChars = new List<Character>();
    public List<Character> SelectChars {  get { return selectChars; } }

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

        SelectSingleHero(0);

        members[0].MagicSkills.Add(new Magic(0, "Power Glow", 10f, 20, 3f, 1f, 2, 2));
        members[1].MagicSkills.Add(new Magic(0, "Fire ball", 10f, 35, 3f, 4f, 0, 1));

        UIManager.instance.ShowMagicToggles();

    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (selectChars.Count > 0)
            {
                selectChars[0].IsMagicMode = true;
                selectChars[0].CurMagicCast = selectChars[0].MagicSkills[0];
            }
        }
    }

    public void SelectSingleHero(int i)
    {
        foreach (Character c in selectChars) {
            c.ToggleRingSelection(false);
        }

        selectChars.Clear();

        selectChars.Add(members[i]);
        selectChars[0].ToggleRingSelection(true);
    }

    public void HeroSelectMagicSkill(int i)
    {
        if (selectChars.Count <= 0) {  return; }

        selectChars[0].IsMagicMode = true;
        selectChars[0].CurMagicCast = selectChars[0].MagicSkills[i];
    }

}
