using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private List<Enemy> monsters;
    public List<Enemy> Monsters { get { return monsters; } }

    public static EnemyManager instance;

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
        foreach (Character m in monsters)
        {
            m.CharInit(UIManager.instance, InventoryManager.instance, PartyManager.instance);
        }
    }
}