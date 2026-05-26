using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject[] heroPrefabs;
    public GameObject[] HeroPrefabs { get { return heroPrefabs; } }

    public static GameManager instance;

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
        if (Settings.isNewGame)
        {
            Settings.isNewGame = false;
            GeneratePlayerHero();
            AudioManager.instance.PlayBGM(1);
        }

        if (Settings.isWarping)
        {
            Settings.isWarping = false;
            WarpPlayers();
        }
    }

    private void GeneratePlayerHero()
    {
        int i = Settings.playerPrefabId;
        GameObject heroObj = Instantiate(
            heroPrefabs[i],
            new Vector3(46f, 10f, 38f),
            Quaternion.identity
        );
        heroObj.tag = "Player";

        Character hero = heroObj.GetComponent<Character>();
        PartyManager.instance.Members.Add(hero);
        hero.CharInit(UIManager.instance, InventoryManager.instance, PartyManager.instance);

        InventoryManager.instance.AddItem(hero, 0);
        InventoryManager.instance.AddItem(hero, 1);
        InventoryManager.instance.AddItem(hero, 2);

        // เรียกหลัง Add Hero เสร็จแล้ว
        UIManager.instance.MapToggleAvatar();
    }

    private void WarpPlayers()
    {
        PartyManager.instance.LoadAllHeroData();
        // เรียกหลัง Load Hero ทั้งหมดเสร็จแล้ว
        UIManager.instance.MapToggleAvatar();
    }
}