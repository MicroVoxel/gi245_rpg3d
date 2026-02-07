using UnityEngine;

public class VFXManager : MonoBehaviour
{
    [SerializeField] private GameObject doubleRingMarker;
    public GameObject DoubleRingMarker { get { return doubleRingMarker; } }

    public static VFXManager instance;

    void Start()
    {
        instance = this;
    }

    void Update()
    {
        
    }
}
