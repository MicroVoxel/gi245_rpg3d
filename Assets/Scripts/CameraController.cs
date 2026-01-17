using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera cam;

    [Header("Move")]
    [SerializeField] private float moveSpeed;

    [SerializeField] private Transform corner1;
    [SerializeField] private Transform corner2;

    [SerializeField] private float xInput;
    [SerializeField] private float zInput;

    [Header("Zoom")]
    [SerializeField] private float zoomModifier;

    public static CameraController instance;

    void Awake()
    {
        instance = this;
        cam = Camera.main;
    }

    void Start()
    {
        moveSpeed = 50;
    }

    void Update()
    {
        MoveByKB();
        Zoom();
        MoveByMouse();
        transform.position = Clamp(corner1.position, corner2.position);

    }

    private void MoveByKB()
    {
        xInput = Input.GetAxis("Horizontal");
        zInput = Input.GetAxis("Vertical");

        Vector3 dir = (transform.forward * zInput) + (transform.right * xInput);

        transform.position += dir * moveSpeed * Time.deltaTime;
    }

    private Vector3 Clamp(Vector3 lowerLeft, Vector3 topRight)
    {
        Vector3 pos = new Vector3(Mathf.Clamp(transform.position.x, lowerLeft.x, topRight.x),
                                  transform.position.y,
                                  Mathf.Clamp(transform.position.z, lowerLeft.z, topRight.z));

        return pos;
    }

    private void Zoom()
    {
        zoomModifier = -Input.GetAxis("Mouse ScrollWheel");
        if (Input.GetKey(KeyCode.Z))
            zoomModifier = -0.1f;
        if (Input.GetKey(KeyCode.X))
            zoomModifier = 0.1f;

        cam.orthographicSize += zoomModifier;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, 4, 10);
    }

    private void MoveByMouse()
    {
        float edgeTolerance = 10f;

        // Horizontal Movement
        if (Input.mousePosition.x >= Screen.width - edgeTolerance)
            transform.Translate(Vector3.right * moveSpeed * Time.deltaTime, Space.World);
        else if (Input.mousePosition.x <= edgeTolerance)
            transform.Translate(Vector3.left * moveSpeed * Time.deltaTime, Space.World);

        // Vertical Movement
        if (Input.mousePosition.y >= Screen.height - edgeTolerance)
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.World);
        else if (Input.mousePosition.y <= edgeTolerance)
            transform.Translate(Vector3.back * moveSpeed * Time.deltaTime, Space.World);
    }

}
