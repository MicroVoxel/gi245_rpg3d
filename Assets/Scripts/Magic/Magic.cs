using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class Magic
{
    #region Var
    [SerializeField] private int id;
    public int ID { get { return id; } }

    [SerializeField] private string name;
    public string Name { get { return name; } }

    [SerializeField] private Sprite icon;
    public Sprite Icon { get { return icon; } }

    [SerializeField] private float range;
    public float Range { get { return range; } }

    [SerializeField] private int power;
    public int Power { get { return power; } }

    [SerializeField] private float loadTime;
    public float LoadTime { get { return loadTime; } }

    [SerializeField] private float shootTime;
    public float ShootTime { get { return shootTime; } }

    [SerializeField] private int loadID;
    public int LoadID { get { return loadID; } }

    [SerializeField] private int shootId;
    public int ShootId { get { return shootId; } }

    #endregion

    public Magic(MagicData data)
    {
        this.id = data.id;
        this.name = data.magicName;
        this.icon = data.icon;
        this.range = data.range;
        this.power = data.power;
        this.loadTime = data.loadTime;
        this.shootTime = data.shootTime;
        this.loadID = data.loadId;
        this.shootId = data.shootId;
    }


}
