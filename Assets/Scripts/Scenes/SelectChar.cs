using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectChar : MonoBehaviour
{
    [SerializeField] private Image charImage;
    [SerializeField] private TMP_Text charNameText;

    [Header("Stat")]
    [SerializeField] private TMP_Text strengthText;
    [SerializeField] private TMP_Text dexterityText;
    [SerializeField] private TMP_Text constitutionText;
    [SerializeField] private TMP_Text intelligenceText;
    [SerializeField] private TMP_Text wisdomText;
    [SerializeField] private TMP_Text charismaText;

    [SerializeField] private GameObject[] heroPrefabs;

    [SerializeField] private int curId = 0;

    private void Start()
    {
        LoadChar();
    }

    private void LoadChar()
    {
        Hero hero = heroPrefabs[curId].GetComponent<Hero>();
        charImage.sprite = hero.AvartarPic;
        charNameText.text = hero.CharName;

        strengthText.text = hero.Strength.ToString();
        dexterityText.text = hero.Dexterity.ToString();
        constitutionText.text = hero.Constitution.ToString();
        intelligenceText.text = hero.Intelligence.ToString();
        wisdomText.text = hero.Wisdom.ToString();
        charismaText.text = hero.Charisma.ToString();

    }

    public void SelectNextChar()
    {
        curId++;

        if(curId >= heroPrefabs.Length)
        {
            curId = 0;
        }

        LoadChar();
    }

    public void SelectPreviousChar()
    {
        curId--;

        if(curId < 0)
        {
            curId = heroPrefabs.Length - 1;
        }

        LoadChar();
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void BeginGame()
    {
        Settings.playerPrefabId = curId;
        Settings.isNewGame = true;
        SceneManager.LoadScene("VillageScene");
    }

}
