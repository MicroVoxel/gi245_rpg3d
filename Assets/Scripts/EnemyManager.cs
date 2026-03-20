using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private List<Enemy> monsters;
    public List<Enemy> Monsters {  get { return monsters; } }

    public static EnemyManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        foreach(Character m in monsters)
        {
            m.charInit(VFXManager.instance,UIManager.instance,InventoryManager.instance);
        }

        InventoryManager.instance.AddItem(monsters[0], 0);
        InventoryManager.instance.AddItem(monsters[0], 1);
        InventoryManager.instance.AddItem(monsters[0], 2);

    }

}
