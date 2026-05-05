using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;

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

    [SerializeField] private GameObject grayImage;

    [SerializeField] private GameObject itemDialog;

    [SerializeField] private GameObject itemUIPrefab;

    [SerializeField] private GameObject[] slots;

    [SerializeField] private ItemDrag curItemDrag;

    [SerializeField] private int curSlotId;

    [SerializeField] private GameObject downPanel;

    [SerializeField] private GameObject npcDialoguePanel;

    [SerializeField] private Image npcImage;

    [SerializeField] private TMP_Text npcNameText;

    [SerializeField] private TMP_Text dialogueText;

    [SerializeField] private int index;

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

    [SerializeField] private Toggle[] toggleAvatar;
    public Toggle[] ToggleAvatar { get { return toggleAvatar; } set { toggleAvatar = value; } }

    [SerializeField] private GameObject charPanel;

    [SerializeField] private TMP_Text charNameText;

    [SerializeField] private TMP_Text statText;

    [SerializeField] private TMP_Text abilityText;

    [SerializeField] private Image heroImage;

    [SerializeField] private GameObject partyPanel;

    [SerializeField] private Toggle[] toggleRemove;

    [SerializeField] private int idToRemove = -1;

    [SerializeField] private Button removeButton;

    [SerializeField] private GameObject confirmPanel;

    [SerializeField] private Hero curHeroToJoin = null;

    [SerializeField] private GameObject btnJoinParty;

    [SerializeField] private GameObject btnNotJoinParty;

    [Header("Reward UI Elements")]
    [SerializeField] private GameObject rewardPanel;

    [SerializeField] private Image rewardIconImage;

    [SerializeField] private TMP_Text rewardNameText;

    [Header("Shop")]

    [SerializeField]
    private GameObject shopPanel;
    public GameObject ShopPanel { get { return shopPanel; } }

    [SerializeField] private TMP_Text npcShopNameText;

    [SerializeField] private Transform shopListParent;

    [SerializeField] private Transform partyListParent;

    [SerializeField] private TMP_Text shopMoneyText;

    [SerializeField] private TMP_Text heroMoneyText;

    [SerializeField] private GameObject itemInShopPrefab;

    [SerializeField] private List<GameObject> shopItemList = new List<GameObject>();

    [SerializeField] private List<GameObject> partyItemList = new List<GameObject>();

    [SerializeField] private int totalCost;

    [SerializeField] private int totalPrice;

    [SerializeField] private Npc curShopNpc = null;

    [SerializeField] private Hero curShopHero = null;

    [SerializeField] private TMP_Text heroNameText;

    private bool _isInternalUpdating = false;

    public static UIManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        ResetMagicToggles();
        InitSlots();
        MapToggleAvatar();
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
            ItemDrag itemInSlot = slots[i].GetComponentInChildren<ItemDrag>();
            if (itemInSlot != null)
            {
                Destroy(itemInSlot.transform.gameObject);
            }
        }
    }

    public void ShowInventory()
    {
        if (PartyManager.instance.SelectChars.Count <= 0) return;

        Character hero = PartyManager.instance.SelectChars[0];

        for (int i = 0; i < InventoryManager.MAXSLOT; i++)
        {
            if (hero.InventoryItems[i] != null)
            {
                GameObject itemObj = Instantiate(itemUIPrefab, slots[i].transform);
                ItemDrag itemDrag = itemObj.GetComponent<ItemDrag>();

                itemDrag.UIManager = this;

                itemDrag.Item = hero.InventoryItems[i];
                itemDrag.IconParent = slots[i].transform;
                itemDrag.Image.sprite = hero.InventoryItems[i].Icon;

            }
        }

    }

    private void InitSlots()
    {
        for (int i = 0; i < InventoryManager.MAXSLOT; i++)
        {
            slots[i].GetComponent<InventorySlot>().ID = i;
        }
    }

    public void SetCurItemInUse(ItemDrag itemDrag, int index)
    {
        curItemDrag = itemDrag;
        curSlotId = index;
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
        InventoryManager.instance.DrinkConsumableItem(curItemDrag.Item, curSlotId);
        DeleteItemIcon();
        ToggleItemDialog(false);
    }

    private void ClearDialogueBox()
    {
        npcImage.sprite = null;

        npcNameText.text = "";
        dialogueText.text = "";

        btnNextText.text = "";
        btnNext.SetActive(false);

        btnAcceptText.text = "";
        btnAccept.SetActive(false);

        btnRejectText.text = "";
        btnReject.SetActive(false);

        btnFinishText.text = "";
        btnFinish.SetActive(false);

        btnNotFinishText.text = "";
        btnNotFinish.SetActive(false);

        btnJoinParty.SetActive(false);
        btnNotJoinParty.SetActive(false);
    }

    private void StartQuestDialogue(Quest quest)
    {
        dialogueText.text = quest.QuestDialogue[index];

        btnNext.SetActive(true);
        btnNextText.text = quest.AnswerNext[index];

        btnAccept.SetActive(false);
        btnReject.SetActive(false);
    }

    private void SetupDialoguePanel(Npc npc)
    {
        index = 0;

        npcImage.sprite = npc.AvartarPic;
        npcNameText.text = npc.CharName;

        Quest inProgressQuest = QuestManager.instance.CheckForQuest(npc, QuestStatus.InProgress);

        if (inProgressQuest != null) //There is an In-Progress Quest going on
        {
            Debug.Log($"in-progress: {inProgressQuest}");
            dialogueText.text = inProgressQuest.QuestionInProgress;

            bool hasItem = QuestManager.instance.CheckIfFinishQuest();
            Debug.Log(hasItem);

            if (hasItem) //has item to finish quest
            {
                btnFinishText.text = inProgressQuest.AnswerFinish;
                btnFinish.SetActive(true);
            }
            else
            {
                btnNotFinishText.text = inProgressQuest.AnswerNotFinish;
                btnNotFinish.SetActive(true);
            }
        }

        else //Check for New Quest
        {
            Quest newQuest = QuestManager.instance.CheckForQuest(npc, QuestStatus.New);
            //Debug.Log(newQuest);

            if (newQuest != null) //There is a new Quest
                StartQuestDialogue(newQuest);
        }
    }

    private void ToggleDialogueBox(bool flag)
    {
        downPanel.SetActive(!flag);
        npcDialoguePanel.SetActive(flag);
        togglePauseUnpause.isOn = flag;
    }

    public void PrepareDialogueBox(Npc npc)
    {
        ClearDialogueBox();
        SetupDialoguePanel(npc);
        ToggleDialogueBox(true);
    }

    public void AnswerNext()
    {
        index++;
        dialogueText.text = QuestManager.instance.NextDialogue(index);

        if (QuestManager.instance.CheckLastDialogue(index))
        {
            btnNext.SetActive(false);

            btnAcceptText.text = QuestManager.instance.CurQuest.AnswerAccept;
            btnAccept.SetActive(true);

            btnRejectText.text = QuestManager.instance.CurQuest.AnswerReject;
            btnReject.SetActive(true);
        }
        else
        {
            btnNextText.text = QuestManager.instance.CurQuest.AnswerNext[index];
            btnNext.SetActive(true);
        }
    }

    public void AnswerReject()
    {
        QuestManager.instance.RejectQuest();
        ToggleDialogueBox(false);
    }

    public void AnswerAccept()
    {
        QuestManager.instance.AcceptQuest();
        ToggleDialogueBox(false);
    }

    public void AnswerFinish()
    {
        Debug.Log("Can Finish Quest");
        bool success = QuestManager.instance.DeliverItem();

        if (success)
        {
            // ดึงไอเทมรางวัลมาใช้งาน
            if (QuestManager.instance.NpcGiveReward(out Item receivedItem, out int receivedEXP))
            {
                Debug.Log("Quest Completed");
                ToggleDialogueBox(false);

                // โชว์ป๊อปอัปรางวัลหลังจากปิดหน้าต่างสนทนา
                ShowRewardPopup(receivedItem, receivedEXP); 
            }
        }
    }

    public void AnswerNotFinish()
    {
        Debug.Log("Cannot Finish Quest");
        ToggleDialogueBox(false);
    }

    public void ShowRewardPopup(Item item, int exp)
    {
        if (item == null || rewardPanel == null) return;

        rewardIconImage.sprite = item.Icon;
        rewardNameText.text = 
            $"Item: +{item.ItemName} \n" +
            $"EXP : +{exp}";

        rewardPanel.SetActive(true);
    }

    public void CloseRewardPopup()
    {
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
        }
    }

    public void MapToggleAvatar()
    {
        foreach (Toggle t in toggleAvatar)
        {
            t.gameObject.SetActive(false);
        }

        for (int i = 0; i < PartyManager.instance.Members.Count; i++)
        {
            toggleAvatar[i].gameObject.SetActive(true);
        }
        toggleAvatar[0].isOn = true;
    }

    public void SelectHeroByAvatar(int i)
    {
        if (toggleAvatar[i].isOn)
        {
            PartyManager.instance.SelectSingleHeroByToggle(i);
        }
        else
        {
            PartyManager.instance.UnSelectSingleHeroByToggle(i);
        }
    }

    public void ClearCharPanel()
    {
        charNameText.text = "";
        statText.text = "";
        abilityText.text = "";
        heroImage.sprite = null;
    }

    public void ShowCharPanel()
    {
        if (PartyManager.instance.SelectChars.Count == 0)
        { 
            return; 
        }
        Hero hero = (Hero)PartyManager.instance.SelectChars[0];

        charNameText.text = hero.CharName;

        string stat = string.Format
        ("Level: {0}\nExperience: {1}\n" +
        "Attack Damage: {2}\nDefense Power: {3}"
        , hero.Level, hero.Exp,
        hero.AttackDamage, hero.DefensePower);

        statText.text = stat;

        string ability = string.Format
        ("Strength: {0}\nDexterity: {1}\n" +
        "Constitution: {2}\nIntelligence: {3}\n" +
        "Wisdom: {4}\nCharisma: {5}"
        , hero.Strength, hero.Dexterity,
        hero.Constitution, hero.Intelligence,
        hero.Wisdom, hero.Charisma);

        abilityText.text = ability;

        heroImage.sprite = hero.AvartarPic;
    }

    public void ToggleCharPanel()
    {
        if (!charPanel.activeInHierarchy)
        {
            charPanel.SetActive(true);
            blackImage.SetActive(true);
            ShowCharPanel();
        }
        else
        {
            charPanel.SetActive(false);
            blackImage.SetActive(false);
            ClearCharPanel();
        }
    }

    public void MapToggleRemove()
    {
        foreach (Toggle t in toggleRemove)
        {
            t.gameObject.SetActive(false);
        }

        List<Character> members = PartyManager.instance.Members;

        for (int i = 1; i < members.Count; i++)
        {
            toggleRemove[i - 1].gameObject.SetActive(true);
            toggleRemove[i - 1].targetGraphic.GetComponent<Image>().sprite = members[i].AvartarPic;
        }
    }

    private void CheckRemoveButton()
    {
        switch (idToRemove)
        {
            case -1:
            case 0:
                removeButton.interactable = false;
                break;
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
                removeButton.interactable = true; 
                break;
            default:
                removeButton.interactable = false;
                break;
        }
    }

    public void TogglePartyPanel(bool flag)
    {
        charPanel.SetActive(!flag);
        partyPanel.SetActive(flag);
        MapToggleRemove();
        CheckRemoveButton();
    }

    public void SelectToRemove(int i)
    {
        if (toggleRemove[i - 1].isOn)
        {
            idToRemove = i;
        }
        else
        {
            idToRemove = -1;
        }
        CheckRemoveButton();
    }

    public void ToggleConfirmPanel(bool flag)
    {
        if (flag == false)
        {
            MapToggleRemove();
            idToRemove = -1;
            CheckRemoveButton();
        }
        partyPanel.SetActive(!flag);
        confirmPanel.SetActive(flag);
    }

    public void RemoveMemberFromParty()
    {
        toggleAvatar[idToRemove].isOn = false;
        PartyManager.instance.RemoveHeroFromParty(idToRemove);
        MapToggleAvatar();
        ToggleConfirmPanel(false);
    }

    private void ClearShopPanel()
    {
        curShopNpc = null;
        curShopHero = null;

        npcShopNameText.text = "";

        shopMoneyText.text = "";
        heroMoneyText.text = "";

        foreach(GameObject obj in shopItemList)
        {
            Destroy(obj);
        }
        shopItemList.Clear();

        foreach(GameObject obj in partyItemList)
        {
            Destroy (obj);
        }
        partyItemList.Clear();
    }

    private void SetupShopItems(Npc npc)
    {
        curShopNpc = npc;
        npcShopNameText.text = curShopNpc.CharName;
        shopMoneyText.text = curShopNpc.NpcMoney.ToString();

        for (int i = 0; i < curShopNpc.ShopItems.Count; i++)
        {
            GameObject itemObj = Instantiate(itemInShopPrefab, shopListParent);
            shopItemList.Add(itemObj);
            ItemInShop itemInShop = itemObj.GetComponent<ItemInShop>();

            itemInShop.ID = i;
            itemInShop.Item = curShopNpc.ShopItems[i];
            itemInShop.SetupItemInShop(this, 1f);
        }
    }

    private void SetupPartyItems(Hero hero) 
    {
        curShopHero = hero;
        heroNameText.text = hero.CharName;
        heroMoneyText.text = PartyManager.instance.PartyMoney.ToString();

        for (int i = 0; i <  16; i++)
        {
            if (hero.InventoryItems[i] != null)
            {
                GameObject itemObj = Instantiate(itemInShopPrefab, partyListParent);
                partyItemList.Add(itemObj);
                ItemInShop itemInShop = itemObj.GetComponent<ItemInShop>();

                itemInShop.ID = i;
                itemInShop.Item = hero.InventoryItems[i];
                itemInShop.SetupItemInShop(this, 0.8f);
            }
        }
    }

    public void ToggleShopPanel(bool flag)
    {
        shopPanel.SetActive(flag);
    }

    public void PrepareShopPanel(Npc npc, Hero hero)
    {
        ClearShopPanel();
        SetupShopItems(npc);
        SetupPartyItems(hero);
        ToggleShopPanel(true);
    }

    public void SellItemToShop()
    {
        totalPrice = 0;
        List<GameObject> toSellCardList = new List<GameObject>();

        foreach (GameObject obj in partyItemList)
        {
            ItemInShop itemInShop = obj.GetComponent<ItemInShop>();
            if (itemInShop.IconToggle.isOn)
            {
                toSellCardList.Add(obj);
                totalPrice += (int)(itemInShop.Item.NormalPrice * 0.8f);
            }
        }

        if (toSellCardList.Count == 0) return;

        if (curShopNpc.NpcMoney >= totalPrice)
        {
            foreach(GameObject obj in toSellCardList)
            {
                obj.transform.SetParent(shopListParent);
                ItemInShop itemInShop = obj.GetComponent<ItemInShop>();
                itemInShop.IconToggle.isOn = false;
                itemInShop.SetupItemInShop(this, 1f);

                partyItemList.Remove(obj);
                shopItemList.Add(obj);
                curShopHero.InventoryItems[itemInShop.ID] = null;
                curShopNpc.ShopItems.Add(itemInShop.Item);
            }
            curShopNpc.NpcMoney -= totalPrice;
            PartyManager.instance.PartyMoney += totalPrice;

            shopMoneyText.text = curShopNpc.NpcMoney.ToString();
            heroMoneyText.text = PartyManager.instance.PartyMoney.ToString();
        }

    }

    public void BuyItemFromShop() 
    { 
        totalCost = 0;
        List<GameObject> toBuyCradList = new List<GameObject>();

        foreach(GameObject obj in shopItemList)
        {
            ItemInShop itemInShop = obj.GetComponent<ItemInShop>();
            if (itemInShop.IconToggle.isOn)
            {
                toBuyCradList.Add(obj);
                totalCost += itemInShop.Item.NormalPrice;
            }
        }

        if (toBuyCradList.Count == 0) return;

        if (PartyManager.instance.PartyMoney >= totalCost)
        {
            foreach(GameObject obj in toBuyCradList)
            {
                obj.transform.SetParent(partyListParent);
                ItemInShop itemInShop = obj.GetComponent<ItemInShop>();
                itemInShop.IconToggle.isOn = false;
                itemInShop.SetupItemInShop(this, 0.8f);

                shopItemList.Remove(obj);
                partyItemList.Add(obj);
                curShopNpc.ShopItems.Remove(itemInShop.Item);
                curShopHero.SaveItemInInventory(itemInShop.Item);
            }
            curShopNpc.NpcMoney += totalCost;
            PartyManager.instance.PartyMoney -= totalCost;

            shopMoneyText.text = curShopNpc.NpcMoney.ToString();
            heroMoneyText.text = PartyManager.instance.PartyMoney.ToString();
        }

    }

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

}