using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [SerializeField] private RectTransform selectionBox;
    public RectTransform SelectionBox => selectionBox;

    [SerializeField] private Toggle togglePauseUnpause;
    [SerializeField] private Toggle[] toggleMagic;
    public Toggle[] ToggleMagic => toggleMagic;

    [SerializeField] private int curToggleMagicID = -1;

    [SerializeField] private GameObject blackImage;

    [SerializeField] protected GameObject inventoryPanel;

    [SerializeField] private GameObject itemUIPrefab;

    [SerializeField] private GameObject[] slots;

    private bool _isInternalUpdating = false;

    public static UIManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        ResetMagicToggles();
    }

    private void Update()
    {
        HandleKeyboardInput();
    }

    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            togglePauseUnpause.isOn = !togglePauseUnpause.isOn;
        }

        int skillCount = toggleMagic.Count();

        for (int i = 0; i < skillCount; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectMagicSkill(i);
            }
        }
    }

    public void ToggleAI(bool isOn)
    {
        foreach (Character member in PartyManager.instance.Members)
        {
            if (member.TryGetComponent<AttackAI>(out var ai))
            {
                ai.enabled = isOn;
            }
        }
    }

    public void SelectAll()
    {
        PartyManager.instance.SelectChars.Clear();
        foreach (Character member in PartyManager.instance.Members)
        {
            if (member.CurHp > 0)
            {
                member.ToggleRingSelection(true);
                PartyManager.instance.SelectChars.Add(member);
            }
        }
    }

    public void PauseUnpause(bool isOn)
    {
        Time.timeScale = isOn ? 0 : 1;
    }

    public void ShowMagicToggles()
    {
        if (PartyManager.instance.SelectChars.Count <= 0) return;

        Character hero = PartyManager.instance.SelectChars[0];
        _isInternalUpdating = true;

        for (int i = 0; i < toggleMagic.Length; i++)
        {
            if (i < hero.MagicSkills.Count)
            {
                toggleMagic[i].interactable = true;
                toggleMagic[i].SetIsOnWithoutNotify(false);
                toggleMagic[i].GetComponentInChildren<TextMeshProUGUI>().text = hero.MagicSkills[i].Name;
                toggleMagic[i].targetGraphic.GetComponent<Image>().sprite = hero.MagicSkills[i].Icon;
            }
            else
            {
                toggleMagic[i].interactable = false;
                toggleMagic[i].GetComponentInChildren<TextMeshProUGUI>().text = " ";
                toggleMagic[i].targetGraphic.GetComponent<Image>().sprite = null;
            }
        }

        _isInternalUpdating = false;
    }

    public void ResetMagicToggles()
    {
        _isInternalUpdating = true;
        foreach (var toggle in toggleMagic)
        {
            toggle.SetIsOnWithoutNotify(false);
            toggle.interactable = false;

            var text = toggle.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.text = " ";

            var image = toggle.targetGraphic.GetComponent<Image>();
            if (image != null) image.sprite = null;
        }
        _isInternalUpdating = false;
    }

    public void OnMagicToggleSelected(int index)
    {
        if (_isInternalUpdating) return;

        if (toggleMagic[index].isOn)
        {
            SelectMagicSkill(index);
        }
    }

    public void SelectMagicSkill(int i)
    {
        if (i < 0 || i >= toggleMagic.Length || _isInternalUpdating) { return; }
        if (i >= PartyManager.instance.SelectChars[0].MagicSkills.Count) { return; }

        _isInternalUpdating = true;

        curToggleMagicID = i;
        PartyManager.instance.HeroSelectMagicSkill(i);

        for (int j = 0; j < toggleMagic.Length; j++)
        {
            toggleMagic[j].isOn = (j == i);
        }

        _isInternalUpdating = false;
    }

    public void IsOnCurToggleMagic(bool flag)
    {
        if (curToggleMagicID >= 0 && curToggleMagicID < toggleMagic.Length)
        {
            _isInternalUpdating = true;
            toggleMagic[curToggleMagicID].isOn = flag;
            _isInternalUpdating = false;
        }
    }

    public void ToggleInventoryPanel()
    {
        if (!inventoryPanel.activeInHierarchy)
        {
            inventoryPanel.SetActive(true);
            blackImage.SetActive(true);
            ShowInventory();
        }
        else
        {
            inventoryPanel.SetActive(false);
            blackImage.SetActive(false);
            ClearInventory();
        }
    }

    public void ClearInventory()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].transform.childCount > 2)
            {
                Transform child = slots[i].transform.GetChild(2);
                Destroy(child.gameObject);
            }
        }
    }

    public void ShowInventory()
    {
        if (PartyManager.instance.SelectChars.Count <= 0) return;

        Character hero = PartyManager.instance.SelectChars[0];

        for (int i = 0; i < hero.InventoryItems.Length; i++)
        {
            if (hero.InventoryItems[i] != null)
            {
                GameObject itemObj = Instantiate(itemUIPrefab, slots[i].transform);
                itemObj.GetComponent<Image>().sprite = hero.InventoryItems[i].Icon;
            }
        }

    }

}