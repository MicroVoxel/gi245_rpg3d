using UnityEngine;

public class VFXManager : MonoBehaviour
{
    [SerializeField] private GameObject doubleRingMarker;
    public GameObject DoubleRingMarker { get { return doubleRingMarker; } }

    [SerializeField] private GameObject[] magicVFX;
    public GameObject[] MagicVFX { get { return magicVFX; } }

    [SerializeField] private MagicData[] magicDatas;
    public MagicData[] MagicDatas { get { return magicDatas; } }

    public static VFXManager instance;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {

    }

    void Update()
    {
        
    }

    public void LoadMagic(int id, Vector3 posA, float time)
    {
        if (MagicVFX[id] == null) { return; }

        GameObject objLoad = Instantiate(MagicVFX[id], posA, Quaternion.identity);
        Destroy(objLoad, time);
    }

    public void ShootMagic(int id, Vector3 posA, Vector3 posB, float time)
    {
        if (MagicVFX[id] == null) { return; }

        GameObject objShoot = Instantiate(MagicVFX[id], posA, Quaternion.identity);
        objShoot.transform.position = Vector3.Lerp(posA, posB, time);
        Destroy(objShoot, time);
    }

}
