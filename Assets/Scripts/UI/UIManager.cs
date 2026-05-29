using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UIManager: ผู้ควบคุมระบบติดต่อผู้ใช้งาน (UI) ทั้งหมดภายในเกมแบบ Data-Driven
/// รองรับหน้าจอตัวละคร, ระบบร้านค้า, กระเป๋าเป้, บทสนทนาเควส และสกิลเวทมนตร์
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    #region === GENERAL UI ===
    [Header("GENERAL UI")]
    [SerializeField] private RectTransform selectionBox;
    public RectTransform SelectionBox => selectionBox;

    [SerializeField] private Toggle togglePauseUnpause;
    [SerializeField] private GameObject blackImage;
    [SerializeField] private GameObject grayImage;
    [SerializeField] private GameObject downPanel;

    private int _overlayCount = 0;
    #endregion

    #region === AVATAR TOGGLES ===
    [Header("AVATAR TOGGLES")]
    [SerializeField] private Toggle[] toggleAvatar;
    public Toggle[] ToggleAvatar
    {
        get { return toggleAvatar; }
        set { toggleAvatar = value; }
    }
    #endregion

    #region === MAGIC TOGGLES ===
    [Header("MAGIC TOGGLES")]
    [SerializeField] private Toggle[] toggleMagic;
    public Toggle[] ToggleMagic => toggleMagic;

    [SerializeField] private int curToggleMagicID = -1;
    private bool _isInternalUpdating = false;
    #endregion

    #region === DIALOGUE ===
    [Header("Dialogue")]
    [SerializeField] private GameObject npcDialoguePanel;
    [SerializeField] private Image npcImage;
    [SerializeField] private TMP_Text npcNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private int dialogueIndex;

    [Header("Dialogue Buttons")]
    [SerializeField] private GameObject btnNext;
    [SerializeField] private TMP_Text btnNextText;
    [SerializeField] private GameObject btnAccept;
    [SerializeField] private TMP_Text btnAcceptText;
    [SerializeField] private GameObject btnReject;
    [SerializeField] private TMP_Text btnRejectText;
    [SerializeField] private GameObject btnFinish;
    [SerializeField] private TMP_Text btnFinishText;
    [SerializeField] private GameObject btnNotFinish;
    [SerializeField] private TMP_Text btnNotFinishText;
    [SerializeField] private GameObject btnJoinParty;
    [SerializeField] private GameObject btnNotJoinParty;

    public bool IsDialogueOpen => npcDialoguePanel.activeInHierarchy;
    #endregion

    #region === HERO JOIN PARTY ===
    [Header("HERO JOIN PARTY")]
    [SerializeField] private Hero curHeroToJoin = null;
    #endregion

    #region === CHARACTER PANEL ===
    [Header("CHARACTER PANEL")]
    [SerializeField] private GameObject charPanel;
    [SerializeField] private TMP_Text charNameText;
    [SerializeField] private TMP_Text statText;
    [SerializeField] private TMP_Text abilityText;
    [SerializeField] private Image heroImage;
    #endregion

    #region === PARTY PANEL ===
    [Header("REFORM PARTY PANEL")]
    [SerializeField] private GameObject partyPanel;
    [SerializeField] private Toggle[] toggleRemove;
    [SerializeField] private int idToRemove = -1;
    [SerializeField] private Button removeButton;
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private TMP_Text confirmText;
    #endregion

    #region === INVENTORY & EQUIPMENT UI ===
    [Header("INVENTORY UI")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject itemUIPrefab;
    [SerializeField] private GameObject itemDialog;
    [SerializeField] private ItemDrag curItemDrag;
    [SerializeField] private int curSlotId;

    [Tooltip("ใส่เฉพาะช่องกระเป๋าเก็บของทั่วไป (0-15)")]
    [SerializeField] private GameObject[] inventorySlots;

    [Header("EQUIPMENT UI")]
    [Tooltip("ใส่ช่องสวมใส่อาวุธหลัก")]
    [SerializeField] private GameObject weaponSlotUI;
    [Tooltip("ใส่ช่องสวมใส่โล่")]
    [SerializeField] private GameObject shieldSlotUI;
    #endregion

    [Header("Item Dialog UI")]
    [SerializeField] private Image dialogItemIcon;
    [SerializeField] private TMP_Text dialogItemName;
    [SerializeField] private TMP_Text dialogItemDesc;

    #region === REWARD ===
    [Header("Reward")]
    [SerializeField] private GameObject rewardPanel;
    [SerializeField] private Image rewardIconImage;
    [SerializeField] private TMP_Text rewardNameText;
    #endregion

    #region === SHOP ===
    [Header("Shop Settings")]
    [SerializeField] private GameObject shopPanel;
    public GameObject ShopPanel => shopPanel;

    [SerializeField] private TMP_Text npcShopNameText;
    [SerializeField] private Transform shopListParent;
    [SerializeField] private Transform partyListParent;
    [SerializeField] private TMP_Text shopMoneyText;
    [SerializeField] private TMP_Text heroMoneyText;
    [SerializeField] private TMP_Text heroNameText;
    [SerializeField] private GameObject itemInShopPrefab;

    private List<GameObject> shopItemList = new List<GameObject>();
    private List<GameObject> partyItemList = new List<GameObject>();

    private int totalCost;
    private int totalPrice;
    private Npc curShopNpc = null;
    private Hero curShopHero = null;
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

        // [CRITICAL FIX 1]: ย้ายระบบทำความสะอาดสเตตัสเริ่มต้นมาไว้ใน Awake()
        // เพื่อรับประกันว่า UI จะถูกเคลียร์เสร็จเรียบร้อย 100% ก่อนที่สคริปต์ของ GameManager หรือ PartyManager
        // จะสั่งเสกตัวละครและโหลดทับข้อมูลสกิลขึ้นจอมือถือในเฟส Start()
        ResetMagicToggles();
        InitInventorySlots();
    }

    private void Start()
    {
        // บังคับสแกนข้อมูลเพื่อแสดงไอคอนสกิลรอบแรกสุดสำหรับตัวละครหลักอย่างปลอดภัย
        ShowMagicToggles();
    }
    #endregion

    #region === OVERLAY HELPER ===
    private void SetOverlay(bool open)
    {
        _overlayCount += open ? 1 : -1;
        _overlayCount = Mathf.Max(0, _overlayCount);
        if (blackImage != null)
        {
            blackImage.SetActive(_overlayCount > 0);
        }
    }
    #endregion

    #region === GAME STATE ===
    public void SetToggleWithoutNotify(Toggle toggle, bool isOn)
    {
        if (toggle == null) return;
        toggle.SetIsOnWithoutNotify(isOn);
    }

    public void PauseUnpause(bool isOn)
    {
        Time.timeScale = isOn ? 0 : 1;
    }

    public void ToggleAI(bool isOn)
    {
        if (PartyManager.instance == null) return;

        foreach (Character member in PartyManager.instance.Members)
        {
            if (member != null && member.TryGetComponent<AttackAI>(out var ai))
                ai.enabled = isOn;
        }
    }

    public void SelectAll()
    {
        if (PartyManager.instance == null) return;

        PartyManager.instance.SelectChars.Clear();

        foreach (Character member in PartyManager.instance.Members)
        {
            if (member == null || member.CurHp <= 0) continue;

            member.ToggleRingSelection(true);
            PartyManager.instance.SelectChars.Add(member);
        }
    }
    #endregion

    #region === AVATAR TOGGLES & VISUALS ===
    public void MapToggleAvatar()
    {
        if (PartyManager.instance == null) return;

        _isInternalUpdating = true;

        for (int i = 0; i < toggleAvatar.Length; i++)
        {
            toggleAvatar[i].gameObject.SetActive(false);

            if (toggleAvatar[i].TryGetComponent<PortraitUIHandler>(out var handler))
            {
                handler.SetupIndex(-1);
            }
        }

        for (int i = 0; i < PartyManager.instance.Members.Count; i++)
        {
            if (i >= toggleAvatar.Length) break;

            toggleAvatar[i].gameObject.SetActive(true);
            toggleAvatar[i].targetGraphic.GetComponent<Image>().sprite = PartyManager.instance.Members[i].AvartarPic;

            if (toggleAvatar[i].TryGetComponent<PortraitUIHandler>(out var handler))
            {
                handler.SetupIndex(i);
            }
        }

        _isInternalUpdating = false;
        SyncToggleVisuals();
    }

    public void SyncToggleVisuals()
    {
        if (PartyManager.instance == null) return;

        _isInternalUpdating = true;

        for (int i = 0; i < PartyManager.instance.Members.Count; i++)
        {
            Character hero = PartyManager.instance.Members[i];
            bool isSelected = PartyManager.instance.SelectChars.Contains(hero);

            if (i < toggleAvatar.Length)
            {
                toggleAvatar[i].SetIsOnWithoutNotify(isSelected);
            }
        }

        if (PartyManager.instance.SelectChars.Count == 0 && PartyManager.instance.Members.Count > 0)
        {
            PartyManager.instance.SelectSingleHero(0);
            toggleAvatar[0].SetIsOnWithoutNotify(true);
        }

        _isInternalUpdating = false;
        ShowMagicToggles();
    }

    public void SelectHeroByAvatar(int i)
    {
        if (_isInternalUpdating || PartyManager.instance == null) return;

        if (toggleAvatar[i].isOn)
        {
            PartyManager.instance.SelectSingleHeroByToggle(i);
        }
        else
        {
            PartyManager.instance.UnSelectSingleHeroByToggle(i);
        }

        if (PartyManager.instance.SelectChars.Count == 0)
        {
            PartyManager.instance.SelectSingleHero(0);
            SetToggleAvatarWithoutNotify(0, true);
        }

        ShowMagicToggles();
    }

    public void SetToggleAvatarWithoutNotify(int index, bool isOn)
    {
        if (index >= 0 && index < toggleAvatar.Length)
        {
            _isInternalUpdating = true;
            toggleAvatar[index].SetIsOnWithoutNotify(isOn);
            _isInternalUpdating = false;
        }
    }

    public void SetAllAvatarTogglesWithoutNotify(bool isOn)
    {
        _isInternalUpdating = true;
        foreach (Toggle t in toggleAvatar)
        {
            if (t != null) t.SetIsOnWithoutNotify(isOn);
        }
        _isInternalUpdating = false;
    }
    #endregion

    #region === MAGIC TOGGLES ===
    /// <summary>
    /// อัปเดตและแสดงผลรายการเวทมนตร์ของฮีโร่ที่กำลังถูกเลือก
    /// </summary>
    public void ShowMagicToggles()
    {
        if (PartyManager.instance == null || PartyManager.instance.SelectChars.Count <= 0) return;

        Character hero = PartyManager.instance.SelectChars[0];
        if (hero == null) return;

        _isInternalUpdating = true;

        for (int i = 0; i < toggleMagic.Length; i++)
        {
            if (toggleMagic[i] == null) continue;

            bool hasSkill = i < hero.MagicSkills.Count;

            toggleMagic[i].interactable = hasSkill;
            toggleMagic[i].SetIsOnWithoutNotify(false);

            // [CRITICAL FIX 2]: เพิ่มระบบตรวจสอบความปลอดภัย (Defensive Null-Guards) ป้องกัน Error กรณีไม่มี Text คอมโพเนนต์ใต้ปุ่ม
            var textComp = toggleMagic[i].GetComponentInChildren<TextMeshProUGUI>();
            if (textComp != null)
            {
                textComp.text = hasSkill ? hero.MagicSkills[i].Name : " ";
            }

            // [CRITICAL FIX 3]: ป้องกันข้อผิดพลาดตอนดึงสไปรต์ภาพกรณีที่ Target Graphic มีโครงสร้างซับซ้อน
            if (toggleMagic[i].targetGraphic != null)
            {
                var img = toggleMagic[i].targetGraphic.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = hasSkill ? hero.MagicSkills[i].Icon : null;
                }
            }
        }

        _isInternalUpdating = false;
    }

    public void ResetMagicToggles()
    {
        _isInternalUpdating = true;

        foreach (var toggle in toggleMagic)
        {
            if (toggle == null) continue;

            toggle.SetIsOnWithoutNotify(false);
            toggle.interactable = false;

            var text = toggle.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.text = " ";

            var image = toggle.targetGraphic.GetComponent<Image>();
            if (image != null) image.sprite = null;
        }

        _isInternalUpdating = false;
    }

    public void OnMagicToggleSelected(int i)
    {
        if (_isInternalUpdating) return;
        if (toggleMagic[i].isOn) SelectMagicSkill(i);
    }

    public void SelectMagicSkill(int i)
    {
        if (i < 0 || i >= toggleMagic.Length || _isInternalUpdating || PartyManager.instance == null) return;
        if (PartyManager.instance.SelectChars.Count == 0) return;
        if (i >= PartyManager.instance.SelectChars[0].MagicSkills.Count) return;

        _isInternalUpdating = true;
        curToggleMagicID = i;

        PartyManager.instance.HeroSelectMagicSkill(i);

        for (int j = 0; j < toggleMagic.Length; j++)
            toggleMagic[j].isOn = (j == i);

        _isInternalUpdating = false;
    }

    public void IsOnCurToggleMagic(bool flag)
    {
        if (curToggleMagicID < 0 || curToggleMagicID >= toggleMagic.Length) return;

        _isInternalUpdating = true;
        toggleMagic[curToggleMagicID].isOn = flag;
        _isInternalUpdating = false;
    }
    #endregion

    #region === DIALOGUE ===
    private void ClearDialogueBox()
    {
        if (npcImage != null) npcImage.sprite = null;
        if (npcNameText != null) npcNameText.text = "";
        if (dialogueText != null) dialogueText.text = "";

        SetDialogueButton(btnNext, btnNextText, false, "");
        SetDialogueButton(btnAccept, btnAcceptText, false, "");
        SetDialogueButton(btnReject, btnRejectText, false, "");
        SetDialogueButton(btnFinish, btnFinishText, false, "");
        SetDialogueButton(btnNotFinish, btnNotFinishText, false, "");

        if (btnJoinParty != null) btnJoinParty.SetActive(false);
        if (btnNotJoinParty != null) btnNotJoinParty.SetActive(false);
    }

    private void SetDialogueButton(GameObject btn, TMP_Text label, bool active, string text)
    {
        if (btn != null) btn.SetActive(active);
        if (label != null) label.text = text;
    }

    private void ToggleDialogueBox(bool flag)
    {
        if (downPanel != null) downPanel.SetActive(!flag);
        if (npcDialoguePanel != null) npcDialoguePanel.SetActive(flag);

        if (togglePauseUnpause != null)
        {
            togglePauseUnpause.SetIsOnWithoutNotify(flag);
        }
        PauseUnpause(flag);
    }

    private void SetupNpcDialogue(Npc npc)
    {
        if (QuestManager.instance == null) return;

        dialogueIndex = 0;
        npcImage.sprite = npc.AvartarPic;
        npcNameText.text = npc.CharName;

        Quest inProgressQuest = QuestManager.instance.CheckForQuest(npc, QuestStatus.InProgress);

        if (inProgressQuest != null)
        {
            dialogueText.text = inProgressQuest.QuestionInProgress;
            bool canFinish = QuestManager.instance.CheckIfFinishQuest();

            if (canFinish)
                SetDialogueButton(btnFinish, btnFinishText, true, inProgressQuest.AnswerFinish);
            else
                SetDialogueButton(btnNotFinish, btnNotFinishText, true, inProgressQuest.AnswerNotFinish);
        }
        else
        {
            Quest newQuest = QuestManager.instance.CheckForQuest(npc, QuestStatus.New);
            if (newQuest != null)
            {
                StartQuestDialogue(newQuest);
            }
            else
            {
                dialogueText.text = "...";
                SetDialogueButton(btnReject, btnRejectText, true, "Goodbye");
            }
        }
    }

    private void StartQuestDialogue(Quest quest)
    {
        if (quest.QuestDialogue == null || quest.QuestDialogue.Length == 0) return;
        dialogueText.text = quest.QuestDialogue[dialogueIndex];
        SetDialogueButton(btnNext, btnNextText, true, quest.AnswerNext[dialogueIndex]);
    }

    public void PrepareDialogueBox(Npc npc)
    {
        ClearDialogueBox();
        SetupNpcDialogue(npc);
        ToggleDialogueBox(true);
    }

    public void AnswerNext()
    {
        if (QuestManager.instance == null || QuestManager.instance.CurQuest == null) return;

        dialogueIndex++;
        dialogueText.text = QuestManager.instance.NextDialogue(dialogueIndex);

        if (QuestManager.instance.CheckLastDialogue(dialogueIndex))
        {
            if (btnNext != null) btnNext.SetActive(false);
            SetDialogueButton(btnAccept, btnAcceptText, true, QuestManager.instance.CurQuest.AnswerAccept);
            SetDialogueButton(btnReject, btnRejectText, true, QuestManager.instance.CurQuest.AnswerReject);
        }
        else
        {
            SetDialogueButton(btnNext, btnNextText, true, QuestManager.instance.CurQuest.AnswerNext[dialogueIndex]);
        }
    }

    public void AnswerAccept()
    {
        if (QuestManager.instance != null) QuestManager.instance.AcceptQuest();
        ToggleDialogueBox(false);
    }

    public void AnswerReject()
    {
        if (QuestManager.instance != null) QuestManager.instance.RejectQuest();
        ToggleDialogueBox(false);
    }

    public void AnswerFinish()
    {
        if (QuestManager.instance == null) return;

        bool success = QuestManager.instance.DeliverItem();
        if (!success) return;

        if (QuestManager.instance.NpcGiveReward(out Item receivedItem, out int receivedEXP))
        {
            ToggleDialogueBox(false);
            ShowRewardPopup(receivedItem, receivedEXP);
        }
    }

    public void AnswerNotFinish()
    {
        ToggleDialogueBox(false);
    }
    #endregion

    #region === HERO JOIN PARTY ===
    private void SetupHeroJoinPartyPanel(Hero hero)
    {
        curHeroToJoin = hero;
        npcImage.sprite = hero.AvartarPic;
        npcNameText.text = hero.CharName;
        dialogueText.text = "I want to join your party.";

        if (btnJoinParty != null) btnJoinParty.SetActive(true);
        if (btnNotJoinParty != null) btnNotJoinParty.SetActive(true);
    }

    public void PrepareHeroJoinParty(Hero hero)
    {
        ClearDialogueBox();
        SetupHeroJoinPartyPanel(hero);
        ToggleDialogueBox(true);
    }

    public void AnswerJoinParty()
    {
        if (curHeroToJoin == null || PartyManager.instance == null) return;

        PartyManager.instance.HeroJoinParty(curHeroToJoin);
        MapToggleAvatar();
        curHeroToJoin = null;
        ToggleDialogueBox(false);
    }

    public void AnswerNotJoinParty()
    {
        curHeroToJoin = null;
        ToggleDialogueBox(false);
    }
    #endregion

    #region === REWARD ===
    public void ShowRewardPopup(Item item, int exp)
    {
        if (item == null || rewardPanel == null) return;

        rewardIconImage.sprite = item.Icon;
        rewardNameText.text = $"Item: +{item.ItemName}\nEXP : +{exp}";
        rewardPanel.SetActive(true);
    }

    public void CloseRewardPopup()
    {
        if (rewardPanel != null)
            rewardPanel.SetActive(false);
    }
    #endregion

    #region === INVENTORY & EQUIPMENT HANDLING ===
    private void InitInventorySlots()
    {
        // 1. ตรวจสอบกระเป๋าปกติ
        for (int i = 0; i < InventoryManager.INVENTORY_CAPACITY; i++)
        {
            if (i < inventorySlots.Length && inventorySlots[i] != null)
            {
                var slotComp = inventorySlots[i].GetComponent<InventorySlot>();
                if (slotComp != null)
                {
                    slotComp.ID = i;
                }
                else
                {
                    Debug.LogWarning($"Slot ที่ {i} ไม่มี Script 'InventorySlot' ติดอยู่!");
                }
            }
            else
            {
                Debug.LogWarning($"Slot ที่ {i} ใน UIManager ไม่ได้ถูกระบุ GameObject หรือเป็น null!");
            }
        }

        // 2. ตรวจสอบช่องสวมใส่
        if (weaponSlotUI != null)
        {
            var weaponComp = weaponSlotUI.GetComponent<InventorySlot>();
            if (weaponComp != null) weaponComp.ID = InventoryManager.WEAPON_SLOT;
        }

        if (shieldSlotUI != null)
        {
            var shieldComp = shieldSlotUI.GetComponent<InventorySlot>();
            if (shieldComp != null) shieldComp.ID = InventoryManager.SHIELD_SLOT;
        }
    }

    public void ToggleInventoryPanel()
    {
        bool open = !inventoryPanel.activeInHierarchy;
        inventoryPanel.SetActive(open);
        SetOverlay(open);

        if (open) ShowInventory();
        else ClearInventory();
    }

    private void ShowInventory()
    {
        if (PartyManager.instance == null || PartyManager.instance.SelectChars.Count <= 0) return;

        Character hero = PartyManager.instance.SelectChars[0];

        // 1. แสดงไอเทมในกระเป๋าปกติ (ตรวจสอบให้แน่ใจว่าไม่ใช่ไอเทมผี)
        for (int i = 0; i < InventoryManager.INVENTORY_CAPACITY; i++)
        {
            if (i >= inventorySlots.Length || hero.InventoryItems[i] == null || string.IsNullOrEmpty(hero.InventoryItems[i].ItemName)) continue;
            CreateItemUI(hero.InventoryItems[i], inventorySlots[i].transform);
        }

        // 2. แสดงไอเทมสวมใส่ (ตรวจสอบให้แน่ใจว่าไม่ใช่ไอเทมผี)
        if (hero.MainWeapon != null && !string.IsNullOrEmpty(hero.MainWeapon.ItemName) && weaponSlotUI != null)
        {
            CreateItemUI(hero.MainWeapon, weaponSlotUI.transform);
        }

        if (hero.Shield != null && !string.IsNullOrEmpty(hero.Shield.ItemName) && shieldSlotUI != null)
        {
            CreateItemUI(hero.Shield, shieldSlotUI.transform);
        }
    }

    private void CreateItemUI(Item item, Transform parentTransform)
    {
        if (itemUIPrefab == null || parentTransform == null) return;

        GameObject itemObj = Instantiate(itemUIPrefab, parentTransform);
        ItemDrag itemDrag = itemObj.GetComponent<ItemDrag>();

        if (itemDrag != null)
        {
            itemDrag.UIManager = this;
            itemDrag.Item = item;
            itemDrag.IconParent = parentTransform;
            if (itemDrag.Image != null)
            {
                itemDrag.Image.sprite = item.Icon;
            }
        }
    }

    private void ClearInventory()
    {
        if (inventorySlots != null)
        {
            foreach (GameObject slot in inventorySlots)
                ClearSlotUI(slot);
        }

        ClearSlotUI(weaponSlotUI);
        ClearSlotUI(shieldSlotUI);
    }

    private void ClearSlotUI(GameObject slot)
    {
        if (slot == null) return;
        ItemDrag itemInSlot = slot.GetComponentInChildren<ItemDrag>();
        if (itemInSlot != null)
            Destroy(itemInSlot.gameObject);
    }

    public void SetCurItemInUse(ItemDrag itemDrag, int i)
    {
        curItemDrag = itemDrag;
        curSlotId = i;
    }

    public void ToggleItemDialog(bool flag, Item item = null)
    {
        if (flag && item != null)
        {
            if (dialogItemIcon != null) dialogItemIcon.sprite = item.Icon;
            if (dialogItemName != null) dialogItemName.text = item.ItemName;
            if (dialogItemDesc != null) dialogItemDesc.text = $"Type: {item.Type}\nPower: {item.Power}";
        }

        if (grayImage != null) grayImage.SetActive(flag);
        if (itemDialog != null) itemDialog.SetActive(flag);
    }

    public void CloseItemDialog()
    {
        ToggleItemDialog(false);
    }

    public void DeleteItemIcon()
    {
        if (curItemDrag != null) Destroy(curItemDrag.gameObject);
    }

    public void ClickDrinkConsumable()
    {
        if (curItemDrag == null || InventoryManager.instance == null) return;

        Item itemToUse = curItemDrag.Item;
        int slotToUse = curSlotId;

        DeleteItemIcon();
        InventoryManager.instance.DrinkConsumableItem(itemToUse, slotToUse);
        ToggleItemDialog(false);
    }
    #endregion

    #region === CHARACTER PANEL ===
    public void ToggleCharPanel()
    {
        bool open = !charPanel.activeInHierarchy;
        charPanel.SetActive(open);
        SetOverlay(open);

        if (open) ShowCharPanel();
        else ClearCharPanel();
    }

    private void ShowCharPanel()
    {
        if (PartyManager.instance == null || PartyManager.instance.SelectChars.Count == 0) return;

        Hero hero = PartyManager.instance.SelectChars[0] as Hero;
        if (hero == null) return;

        charNameText.text = hero.CharName;
        heroImage.sprite = hero.AvartarPic;

        string expDisplay = hero.Level >= 20 ? "MAX" : $"{hero.Exp}/{hero.NextExp}";

        statText.text = string.Format(
            "Level: {0}\nExp: {1}\nAttack: {2}\nDefense: {3}",
            hero.Level, expDisplay, hero.AttackDamage, hero.BaseDefense);

        abilityText.text = string.Format(
            "Strength: {0}\nDexterity: {1}\nConstitution: {2}\nIntelligence: {3}\nWisdom: {4}\nCharisma: {5}",
            hero.Strength, hero.Dexterity, hero.Constitution,
            hero.Intelligence, hero.Wisdom, hero.Charisma);
    }

    private void ClearCharPanel()
    {
        charNameText.text = "";
        statText.text = "";
        abilityText.text = "";
        heroImage.sprite = null;
    }
    #endregion

    #region === PARTY PANEL ===
    public void TogglePartyPanel(bool flag)
    {
        if (charPanel != null) charPanel.SetActive(!flag);
        if (partyPanel != null) partyPanel.SetActive(flag);
        MapToggleRemove();
        RefreshRemoveButton();

        if (!flag)
        {
            ShowCharPanel();
        }
    }

    public void MapToggleRemove()
    {
        if (PartyManager.instance == null) return;

        foreach (Toggle t in toggleRemove)
        {
            if (t != null) t.gameObject.SetActive(false);
        }

        var members = PartyManager.instance.Members;

        for (int i = 1; i < members.Count; i++)
        {
            if (i - 1 >= toggleRemove.Length) break;
            toggleRemove[i - 1].gameObject.SetActive(true);
            toggleRemove[i - 1].targetGraphic.GetComponent<Image>().sprite = members[i].AvartarPic;
        }
    }

    public void SelectToRemove(int i)
    {
        if (i - 1 >= toggleRemove.Length) return;
        idToRemove = toggleRemove[i - 1].isOn ? i : -1;
        RefreshRemoveButton();
    }

    private void RefreshRemoveButton()
    {
        if (PartyManager.instance == null || removeButton == null) return;
        int maxIndex = PartyManager.instance.Members.Count - 1;
        removeButton.interactable = (idToRemove >= 1 && idToRemove <= maxIndex);
    }

    public void ToggleConfirmPanel(bool flag)
    {
        if (PartyManager.instance == null) return;

        if (flag)
        {
            if (idToRemove >= 1 && idToRemove < PartyManager.instance.Members.Count)
            {
                string heroName = PartyManager.instance.Members[idToRemove].CharName;
                if (confirmText != null)
                {
                    confirmText.text = $"Are you sure you want {heroName} to leave the party?";
                }
            }
        }
        else
        {
            MapToggleRemove();
            idToRemove = -1;
            RefreshRemoveButton();
        }

        if (partyPanel != null) partyPanel.SetActive(!flag);
        if (confirmPanel != null) confirmPanel.SetActive(flag);
    }

    public void RemoveMemberFromParty()
    {
        if (PartyManager.instance == null) return;

        SetToggleAvatarWithoutNotify(idToRemove, false);
        PartyManager.instance.RemoveHeroFromParty(idToRemove);
        MapToggleAvatar();
        ToggleConfirmPanel(false);
    }
    #endregion

    #region === SHOP (DATA-DRIVEN) ===
    public void PrepareShopPanel(Npc npc, Hero hero)
    {
        ClearShopPanel();
        SetupShopItems(npc);
        SetupPartyItems(hero);
        shopPanel.SetActive(true);
        SetOverlay(true);
    }

    public void ToggleShopPanel(bool flag)
    {
        shopPanel.SetActive(flag);
        SetOverlay(flag);
    }

    private void ClearShopPanel()
    {
        curShopNpc = null;
        curShopHero = null;
        npcShopNameText.text = "";
        shopMoneyText.text = "";
        heroMoneyText.text = "";
        heroNameText.text = "";

        // ป้องกันหน่วยความจำรั่วไหลด้วยการเคลียร์ GameObjects เก่าทิ้ง
        foreach (GameObject obj in shopItemList) Destroy(obj);
        foreach (GameObject obj in partyItemList) Destroy(obj);

        shopItemList.Clear();
        partyItemList.Clear();
    }

    private void SetupShopItems(Npc npc)
    {
        curShopNpc = npc;
        npcShopNameText.text = npc.CharName;
        shopMoneyText.text = npc.NpcMoney.ToString();

        for (int i = 0; i < npc.ShopItems.Count; i++)
        {
            if (npc.ShopItems[i] == null) continue;

            GameObject itemObj = Instantiate(itemInShopPrefab, shopListParent);
            ItemInShop itemInShop = itemObj.GetComponent<ItemInShop>();

            if (itemInShop != null)
            {
                // [CRITICAL BUG FIX 1]: เรียกฟังก์ชัน Setup ข้อมูลก่อน แล้วค่อยเขียนทับ ID เป็นลำดับสุดท้าย
                // เพื่อแก้ไขปัญหาร้านค้าเขียนทับตัวแปร ID ของ UI Slot ด้วย Item Database ID จนทำให้ตรวจดัชนีกระเป๋าเป้คลาดเคลื่อน
                itemInShop.SetupItemInShop(npc.ShopItems[i], this, 1f);
                itemInShop.ID = i;

                shopItemList.Add(itemObj);
            }
        }
    }

    private void SetupPartyItems(Hero hero)
    {
        curShopHero = hero;
        heroNameText.text = hero.CharName;
        heroMoneyText.text = PartyManager.instance.PartyMoney.ToString();

        for (int i = 0; i < InventoryManager.INVENTORY_CAPACITY; i++)
        {
            if (hero.InventoryItems[i] == null || string.IsNullOrEmpty(hero.InventoryItems[i].ItemName)) continue;

            GameObject itemObj = Instantiate(itemInShopPrefab, partyListParent);
            ItemInShop itemInShop = itemObj.GetComponent<ItemInShop>();

            if (itemInShop != null)
            {
                ItemData data = InventoryManager.instance.ItemData[hero.InventoryItems[i].ID];

                // [CRITICAL BUG FIX 2]: เรียกฟังก์ชัน Setup ข้อมูลให้เรียบร้อยก่อน แล้วค่อยบันทึก ID เป็นลำดับสุดท้าย
                // ทำให้ค่า ID สามารถชี้ไปยังพิกัดสล็อตช่องเก็บของหลัก (0 - 15) ของผู้เล่นได้อย่างถูกต้องและแม่นยำที่สุด
                itemInShop.SetupItemInShop(data, this, 0.8f);
                itemInShop.ID = i;

                partyItemList.Add(itemObj);
            }
        }
    }

    /// <summary>
    /// ขายไอเทมที่เลือกให้แก่ร้านค้า NPC
    /// </summary>
    public void SellItemToShop()
    {
        if (curShopNpc == null || curShopHero == null || InventoryManager.instance == null || PartyManager.instance == null) return;

        List<ItemInShop> toSell = GetSelectedShopItems(partyItemList);
        totalPrice = toSell.Sum(item => (int)(item.Item.NormalPrice * 0.8f));

        if (toSell.Count == 0 || curShopNpc.NpcMoney < totalPrice) return;

        // 1. จัดการข้อมูลหลังบ้าน (Data Layer) ป้องกันปัญหากระเป๋าสลับและลบผิดชิ้น
        foreach (ItemInShop itemInShop in toSell)
        {
            ItemData originalData = InventoryManager.instance.ItemData[itemInShop.Item.ID];

            // เคลียร์ข้อมูลช่องเป้าหมายตาม slot index ทันทีอย่างแม่นยำ (ตอนนี้ได้รับการการันตีว่าค่า ID บันทึกตำแหน่งสล็อตที่ถูกต้องแล้ว)
            curShopHero.InventoryItems[itemInShop.ID] = null;
            curShopNpc.ShopItems.Add(originalData);
        }

        curShopNpc.NpcMoney -= totalPrice;
        PartyManager.instance.PartyMoney += totalPrice;

        // 2. บังคับอัปเดตการวาด UI ทั้งหมดตามข้อมูลจริง (View Layer Refresh)
        RefreshShopPanel();
    }

    /// <summary>
    /// ซื้อไอเทมที่เลือกจากร้านค้า NPC เข้าตัวละครผู้ร่าย
    /// </summary>
    public void BuyItemFromShop()
    {
        if (curShopNpc == null || curShopHero == null || InventoryManager.instance == null || PartyManager.instance == null) return;

        List<ItemInShop> toBuy = GetSelectedShopItems(shopItemList);
        totalCost = toBuy.Sum(item => item.Item.NormalPrice);

        if (toBuy.Count == 0 || PartyManager.instance.PartyMoney < totalCost) return;

        // 1. จัดการข้อมูลหลังบ้าน (Data Layer)
        foreach (ItemInShop itemInShop in toBuy)
        {
            ItemData originalData = InventoryManager.instance.ItemData[itemInShop.Item.ID];

            curShopNpc.ShopItems.Remove(originalData);

            Item newItem = new Item(originalData);
            curShopHero.SaveItemInInventory(newItem);
        }

        curShopNpc.NpcMoney += totalCost;
        PartyManager.instance.PartyMoney -= totalCost;

        // 2. บังคับโหลด UI ใหม่
        RefreshShopPanel();
    }

    /// <summary>
    /// ตรวจสอบและดึงข้อมูลเฉพาะวัตถุที่ทำการกดติ๊กเลือกไว้ในระบบซื้อ/ขาย
    /// </summary>
    private List<ItemInShop> GetSelectedShopItems(List<GameObject> sourceList)
    {
        var result = new List<ItemInShop>();
        foreach (GameObject obj in sourceList)
        {
            if (obj == null) continue;
            ItemInShop itemInShop = obj.GetComponent<ItemInShop>();
            if (itemInShop != null && itemInShop.IconToggle.isOn)
            {
                result.Add(itemInShop);
            }
        }
        return result;
    }

    /// <summary>
    /// บังคับการรีเฟรชหน้าจอแสดงผลร้านค้าทั้งหมดขึ้นมาใหม่ เพื่อป้องกันความคลาดเคลื่อนของไอดีและสเกล
    /// </summary>
    private void RefreshShopPanel()
    {
        if (curShopNpc != null && curShopHero != null)
        {
            Npc tempNpc = curShopNpc;
            Hero tempHero = curShopHero;

            ClearShopPanel();
            SetupShopItems(tempNpc);
            SetupPartyItems(tempHero);
        }
    }
    #endregion
}