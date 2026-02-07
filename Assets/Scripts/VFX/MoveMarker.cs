using UnityEngine;

public class MoveMarker : MonoBehaviour
{
    [SerializeField] private float lifeTime = 1;
    void Start()
    {
        Destroy(gameObject,lifeTime);
    }

    void Update()
    {
        
    }
}
