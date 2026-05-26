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

    private void OnEnable()
    {
        MyActions.onLoadMagic += LoadMagic;
        MyActions.onShootMagic += ShootMagic;
        MyActions.onCreateMagic += CreateMagic;
    }

    private void OnDisable()
    {
        MyActions.onLoadMagic -= LoadMagic;
        MyActions.onShootMagic -= ShootMagic;
        MyActions.onCreateMagic -= CreateMagic;
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

    public Magic CreateMagic(int id)
    {
        return new Magic(magicDatas[id]);
    }

}
