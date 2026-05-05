using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemInShop : MonoBehaviour
{
    [SerializeField] private int id;
    public int ID { get { return id; } set { id = value; } }

    [SerializeField] private Item item;
    public Item Item { get { return item; } set { item = value; } }

    [SerializeField] private Toggle iconToggle;
    public Toggle IconToggle { get { return iconToggle; } }

    [SerializeField] private TMP_Text itemText;

    [SerializeField] private TMP_Text priceText;

    [SerializeField] private UIManager uiMgr;

    public void SetupItemInShop(UIManager uIManager, float discount)
    {
        uiMgr = uIManager;
        iconToggle.targetGraphic.GetComponent<Image>().sprite = item.Icon;
        itemText.text = item.ItemName;
        priceText.text = ((int)(item.NormalPrice * discount)).ToString();
    }

}
