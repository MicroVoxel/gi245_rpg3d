using UnityEngine;

public class WarpPoint : MonoBehaviour
{
    [SerializeField] private string toMapName;

    [SerializeField] private int enterPointId;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            MapManager.instance.GoToMap(toMapName, enterPointId);
        }
    }
}
