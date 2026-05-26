using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;

/// <summary>
/// จัดการ UI ทั้งหมดของเกม: Avatar, Magic, Dialogue, Shop, Inventory, Quest Reward
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    #region === GENERAL UI ===
    [SerializeField] private RectTransform selectionBox;
    public RectTransform SelectionBox => selectionBox;

    [SerializeField] private Toggle togglePauseUnpause;
    [SerializeField] private GameObject blackImage;
    [SerializeField] private GameObject grayImage;
    [SerializeField] private GameObject downPanel;

    private int _overlayCount = 0;
    #endregion

    #region === AVATAR TOGGLES ===
    [SerializeField] private Toggle[] toggleAvatar;
    public Toggle[] ToggleAvatar
    {
        get { return toggleAvatar; }
        set { toggleAvatar = value; }
    }
    #endregion

    #region === MAGIC TOGGLES ===
    [SerializeField] private Toggle[] toggleMagic;
    public Toggle[] ToggleMagic => toggleMagic;

    [SerializeField] private int curToggleMagicID = -1;
    private bool _isInternalUpdating = false;
    #endregion

    #region === DIALOGUE ===
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
    [SerializeField] private Hero curHeroToJoin = null;
    #endregion

    #region === CHARACTER PANEL ===
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

    #region === INVENTORY ===
    [Header("INVENTORY")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject itemUIPrefab;
    [SerializeField] private GameObject[] slots;
    [SerializeField] private GameObject itemDialog;
    [SerializeField] private ItemDrag curItemDrag;
    [SerializeField] private int curSlotId;
    #endregion

    #region === REWARD ===
    [Header("Reward")]
    [SerializeField] private GameObject rewardPanel;
    [SerializeField] private Image rewardIconImage;
    [SerializeField] private TMP_Text rewardNameText;
    #endregion

    #region === SHOP ===
    [Header("Shop")]
    [SerializeField] private GameObject shopPanel;
    public GameObject ShopPanel { get { return shopPanel; } }

    [SerializeField] private TMP_Text npcShopNameText;
    [SerializeField] private Transform shopListParent;
    [SerializeField] private Transform partyListParent;
    [SerializeField] private TMP_Text shopMoneyText;
    [SerializeField] private TMP_Text heroMoneyText;
    [SerializeField] private TMP_Text heroNameText;
    [SerializeField] private GameObject itemInShopPrefab;

    [SerializeField] private List<GameObject> shopItemList = new List<GameObject>();
    [SerializeField] private List<GameObject> partyItemList = new List<GameObject>();

    [SerializeField] private int totalCost;
    [SerializeField] private int totalPrice;
    [SerializeField] private Npc curShopNpc = null;
    [SerializeField] private Hero curShopHero = null;
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
        ResetMagicToggles();
        InitInventorySlots();
    }
    #endregion

    #region === OVERLAY HELPER ===
    private void SetOverlay(bool open)
    {
        _overlayCount += open ? 1 : -1;
        _overlayCount = Mathf.Max(0, _overlayCount);
        blackImage.SetActive(_overlayCount > 0);
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
        foreach (Character member in PartyManager.instance.Members)
        {
            if (member.TryGetComponent<AttackAI>(out var ai))
                ai.enabled = isOn;
        }
    }

    public void SelectAll()
    {
        PartyManager.instance.SelectChars.Clear();

        foreach (Character member in PartyManager.instance.Members)
        {
            if (member.CurHp <= 0) continue;

            member.ToggleRingSelection(true);
            PartyManager.instance.SelectChars.Add(member);
        }
    }
    #endregion

    #region === AVATAR TOGGLES & VISUALS ===
    public void MapToggleAvatar()
    {
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
        if (_isInternalUpdating) return;

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
            t.SetIsOnWithoutNotify(isOn);
        }
        _isInternalUpdating = false;
    }
    #endregion

    #region === MAGIC TOGGLES ===
    public void ShowMagicToggles()
    {
        if (PartyManager.instance.SelectChars.Count <= 0) return;

        Character hero = PartyManager.instance.SelectChars[0];
        _isInternalUpdating = true;

        for (int i = 0; i < toggleMagic.Length; i++)
        {
            bool hasSkill = i < hero.MagicSkills.Count;

            toggleMagic[i].interactable = hasSkill;
            toggleMagic[i].SetIsOnWithoutNotify(false);
            toggleMagic[i].GetComponentInChildren<TextMeshProUGUI>().text =
                hasSkill ? hero.MagicSkills[i].Name : " ";
            toggleMagic[i].targetGraphic.GetComponent<Image>().sprite =
                hasSkill ? hero.MagicSkills[i].Icon : null;
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

    public void OnMagicToggleSelected(int i)
    {
        if (_isInternalUpdating) return;
        if (toggleMagic[i].isOn) SelectMagicSkill(i);
    }

    public void SelectMagicSkill(int i)
    {
        if (i < 0 || i >= toggleMagic.Length || _isInternalUpdating) return;
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
        npcImage.sprite = null;
        npcNameText.text = "";
        dialogueText.text = "";

        SetDialogueButton(btnNext, btnNextText, false, "");
        SetDialogueButton(btnAccept, btnAcceptText, false, "");
        SetDialogueButton(btnReject, btnRejectText, false, "");
        SetDialogueButton(btnFinish, btnFinishText, false, "");
        SetDialogueButton(btnNotFinish, btnNotFinishText, false, "");

        btnJoinParty.SetActive(false);
        btnNotJoinParty.SetActive(false);
    }

    private void SetDialogueButton(GameObject btn, TMP_Text label, bool active, string text)
    {
        btn.SetActive(active);
        label.text = text;
    }

    private void ToggleDialogueBox(bool flag)
    {
        downPanel.SetActive(!flag);
        npcDialoguePanel.SetActive(flag);

        togglePauseUnpause.SetIsOnWithoutNotify(flag);
        PauseUnpause(flag);
    }

    private void SetupNpcDialogue(Npc npc)
    {
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
        dialogueIndex++;
        dialogueText.text = QuestManager.instance.NextDialogue(dialogueIndex);

        if (QuestManager.instance.CheckLastDialogue(dialogueIndex))
        {
            btnNext.SetActive(false);
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
        QuestManager.instance.AcceptQuest();
        ToggleDialogueBox(false);
    }

    public void AnswerReject()
    {
        QuestManager.instance.RejectQuest();
        ToggleDialogueBox(false);
    }

    public void AnswerFinish()
    {
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

        btnJoinParty.SetActive(true);
        btnNotJoinParty.SetActive(true);
    }

    public void PrepareHeroJoinParty(Hero hero)
    {
        ClearDialogueBox();
        SetupHeroJoinPartyPanel(hero);
        ToggleDialogueBox(true);
    }

    public void AnswerJoinParty()
    {
        if (curHeroToJoin == null) return;

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

    #region === INVENTORY ===
    private void InitInventorySlots()
    {
        for (int i = 0; i < InventoryManager.MAXSLOT; i++)
        {
            if (i < slots.Length && slots[i] != null)
                slots[i].GetComponent<InventorySlot>().ID = i;
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
        if (PartyManager.instance.SelectChars.Count <= 0) return;

        Character hero = PartyManager.instance.SelectChars[0];

        // [FIX] ต้องวนลูปถึง MAXSLOT (18 ช่อง) เพื่อให้โค้ดสร้างรูปภาพไอเทมให้กับช่องโล่และอาวุธด้วย
        // แม้ในหน้าจอ UI คุณจะแยกมันไปไว้ในส่วน Equipment UI แล้วก็ตาม แต่มันยังคงถูกควบคุมผ่าน slots เดียวกัน
        for (int i = 0; i < InventoryManager.MAXSLOT; i++)
        {
            if (i >= slots.Length || hero.InventoryItems[i] == null) continue;

            GameObject itemObj = Instantiate(itemUIPrefab, slots[i].transform);
            ItemDrag itemDrag = itemObj.GetComponent<ItemDrag>();

            itemDrag.UIManager = this;
            itemDrag.Item = hero.InventoryItems[i];
            itemDrag.IconParent = slots[i].transform;
            itemDrag.Image.sprite = hero.InventoryItems[i].Icon;
        }
    }

    private void ClearInventory()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            ItemDrag itemInSlot = slots[i].GetComponentInChildren<ItemDrag>();
            if (itemInSlot != null)
                Destroy(itemInSlot.gameObject);
        }
    }

    public void SetCurItemInUse(ItemDrag itemDrag, int i)
    {
        curItemDrag = itemDrag;
        curSlotId = i;
    }

    public void ToggleItemDialog(bool flag)
    {
        grayImage.SetActive(flag);
        itemDialog.SetActive(flag);
    }

    public void DeleteItemIcon()
    {
        Destroy(curItemDrag.gameObject);
    }

    public void ClickDrinkConsumable()
    {
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
        if (PartyManager.instance.SelectChars.Count == 0) return;

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
        charPanel.SetActive(!flag);
        partyPanel.SetActive(flag);
        MapToggleRemove();
        RefreshRemoveButton();

        if (!flag)
        {
            ShowCharPanel();
        }
    }

    public void MapToggleRemove()
    {
        foreach (Toggle t in toggleRemove)
            t.gameObject.SetActive(false);

        var members = PartyManager.instance.Members;

        for (int i = 1; i < members.Count; i++)
        {
            toggleRemove[i - 1].gameObject.SetActive(true);
            toggleRemove[i - 1].targetGraphic.GetComponent<Image>().sprite = members[i].AvartarPic;
        }
    }

    public void SelectToRemove(int i)
    {
        idToRemove = toggleRemove[i - 1].isOn ? i : -1;
        RefreshRemoveButton();
    }

    private void RefreshRemoveButton()
    {
        int maxIndex = PartyManager.instance.Members.Count - 1;
        removeButton.interactable = (idToRemove >= 1 && idToRemove <= maxIndex);
    }

    public void ToggleConfirmPanel(bool flag)
    {
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

        partyPanel.SetActive(!flag);
        confirmPanel.SetActive(flag);
    }

    public void RemoveMemberFromParty()
    {
        SetToggleAvatarWithoutNotify(idToRemove, false);
        PartyManager.instance.RemoveHeroFromParty(idToRemove);
        MapToggleAvatar();
        ToggleConfirmPanel(false);
    }
    #endregion

    #region === SHOP ===
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
            GameObject itemObj = Instantiate(itemInShopPrefab, shopListParent);
            ItemInShop itemInShop = itemObj.GetComponent<ItemInShop>();

            itemInShop.ID = i;
            itemInShop.Item = npc.ShopItems[i];
            itemInShop.SetupItemInShop(this, 1f);
            shopItemList.Add(itemObj);
        }
    }

    private void SetupPartyItems(Hero hero)
    {
        curShopHero = hero;
        heroNameText.text = hero.CharName;
        heroMoneyText.text = PartyManager.instance.PartyMoney.ToString();

        // ตรงนี้วนลูปแค่ INVENTORY_CAPACITY ถูกต้องแล้ว ป้องกันช่องสวมใส่โผล่ในร้านค้า
        for (int i = 0; i < InventoryManager.INVENTORY_CAPACITY; i++)
        {
            if (hero.InventoryItems[i] == null) continue;

            GameObject itemObj = Instantiate(itemInShopPrefab, partyListParent);
            ItemInShop itemInShop = itemObj.GetComponent<ItemInShop>();

            itemInShop.ID = i;
            itemInShop.Item = hero.InventoryItems[i];
            itemInShop.SetupItemInShop(this, 0.8f);
            partyItemList.Add(itemObj);
        }
    }

    public void SellItemToShop()
    {
        var toSell = GetSelectedShopItems(partyItemList);
        totalPrice = toSell.Sum(obj => (int)(obj.GetComponent<ItemInShop>().Item.NormalPrice * 0.8f));

        if (toSell.Count == 0 || curShopNpc.NpcMoney < totalPrice) return;

        foreach (GameObject obj in toSell)
        {
            ItemInShop itemInShop = obj.GetComponent<ItemInShop>();

            obj.transform.SetParent(shopListParent);
            itemInShop.IconToggle.isOn = false;
            itemInShop.SetupItemInShop(this, 1f);

            partyItemList.Remove(obj);
            shopItemList.Add(obj);

            InventoryManager.instance.RemoveItemFromHeroBag(curShopHero, itemInShop.ID);
            curShopNpc.ShopItems.Add(itemInShop.Item);
        }

        curShopNpc.NpcMoney -= totalPrice;
        PartyManager.instance.PartyMoney += totalPrice;
        RefreshShopMoneyUI();
    }

    public void BuyItemFromShop()
    {
        var toBuy = GetSelectedShopItems(shopItemList);
        totalCost = toBuy.Sum(obj => obj.GetComponent<ItemInShop>().Item.NormalPrice);

        if (toBuy.Count == 0 || PartyManager.instance.PartyMoney < totalCost) return;

        foreach (GameObject obj in toBuy)
        {
            ItemInShop itemInShop = obj.GetComponent<ItemInShop>();

            obj.transform.SetParent(partyListParent);
            itemInShop.IconToggle.isOn = false;
            itemInShop.SetupItemInShop(this, 0.8f);

            shopItemList.Remove(obj);
            partyItemList.Add(obj);
            curShopNpc.ShopItems.Remove(itemInShop.Item);
            curShopHero.SaveItemInInventory(itemInShop.Item);
        }

        curShopNpc.NpcMoney += totalCost;
        PartyManager.instance.PartyMoney -= totalCost;
        RefreshShopMoneyUI();
    }

    private List<GameObject> GetSelectedShopItems(List<GameObject> sourceList)
    {
        var result = new List<GameObject>();
        foreach (GameObject obj in sourceList)
        {
            if (obj.GetComponent<ItemInShop>().IconToggle.isOn)
                result.Add(obj);
        }
        return result;
    }

    private void RefreshShopMoneyUI()
    {
        shopMoneyText.text = curShopNpc.NpcMoney.ToString();
        heroMoneyText.text = PartyManager.instance.PartyMoney.ToString();
    }
    #endregion
}