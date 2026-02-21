using UnityEngine;

[System.Serializable]
public class Magic
{
    #region Var
    [SerializeField] private int id;
    public int ID { get { return id; } }

    [SerializeField] private string name;
    public string Name { get { return name; } }

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

    public Magic(int id,string name, float range, int power, float loadTime, float shootTime, int loadId, int shootId)
    {
        this.id = id;
        this.name = name;
        this.range = range;
        this.power = power;
        this.loadTime = loadTime;
        this.shootTime = shootTime;
        this.loadID = loadId;
        this.shootId = shootId;
    }


}
