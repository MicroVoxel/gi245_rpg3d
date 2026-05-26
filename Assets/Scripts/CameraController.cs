using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;

    [SerializeField] private Camera cam;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 25f;
    [Tooltip("ตัวคูณชดเชยความเร็วแกน Z (ขึ้น/ลง) เพื่อแก้ภาพลวงตาจากมุมก้มกล้องจอ 16:9")]
    [SerializeField] private float depthMultiplier = 1.5f;

    [SerializeField] private Transform corner1;
    [SerializeField] private Transform corner2;

    [SerializeField] private float xInput;
    [SerializeField] private float zInput;

    private InputAction moveAction;
    private Vector2 moveValue;

    [Header("Zoom")]
    [SerializeField] private float zoomModifier;
    [SerializeField] private float zoomSpeed = 0.05f;

    private InputAction zoomAction;
    private Vector2 zoomValue;

    [Header("Follow Target (Double Click)")]
    [SerializeField] private Transform targetToFollow;
    [SerializeField] private float followLerpSpeed = 10f;

    void Awake()
    {
        instance = this;
        // ป้องกันกรณีที่ลืมลากกล้องมาใส่ใน Inspector ให้หาระบบกล้องหลักอัตโนมัติ
        if (cam == null)
        {
            cam = Camera.main;
        }
    }

    void Start()
    {
        // ดึงข้อมูล Action จากระบบ Input System Actions ของ Unity
        moveAction = InputSystem.actions.FindAction("Move");
        zoomAction = InputSystem.actions.FindAction("Zoom");

        // ต้องเปิดใช้งาน (Enable) เสมอเพื่อให้สคริปต์รับ Input จากอุปกรณ์ได้
        moveAction?.Enable();
        zoomAction?.Enable();
    }

    void LateUpdate()
    {
        HandleInput();

        if (targetToFollow != null)
        {
            FollowTargetUpdate();
        }
        else
        {
            MoveByKB();
        }

        Zoom();
        transform.position = Clamp(corner1.position, corner2.position);
    }

    private void HandleInput()
    {
        if (moveAction == null) return;

        moveValue = moveAction.ReadValue<Vector2>();
        xInput = moveValue.x;
        zInput = moveValue.y;

        // ยกเลิกล็อคกล้องทันที หากผู้เล่นพยายามกดปุ่มเดินกล้องด้วยตัวเอง (WASD หรือ ปุ่มลูกศร)
        if (Mathf.Abs(xInput) > 0.1f || Mathf.Abs(zInput) > 0.1f)
        {
            targetToFollow = null;
        }
    }

    private void MoveByKB()
    {
        if (cam == null) return;

        // 1. หาแกนอ้างอิงจากกล้อง (ตัดแกน Y ออกให้อยู่ระนาบพื้น)
        Vector3 forward = cam.transform.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 right = cam.transform.right;
        right.y = 0;
        right.Normalize();

        // 2. Normalize ค่า Input ก่อน เพื่อให้เวลาผู้เล่นกดเดินเฉียง (เช่น W+A) ความเร็ว Input จะไม่เกิน 1
        Vector2 inputDir = new Vector2(xInput, zInput).normalized;

        // 3. นำ Input ไปคูณทิศทาง โดยเพิ่ม depthMultiplier เข้าไปที่แกนเดินขึ้น/ลง (Forward)
        Vector3 moveDir = (forward * (inputDir.y * depthMultiplier)) + (right * inputDir.x);

        // 4. เอา moveDir ไปคูณความเร็วได้เลย (ห้ามใช้ moveDir.normalized ตรงนี้ ไม่งั้น depthMultiplier จะถูกล้างค่าทิ้ง)
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    /// <summary>
    /// สั่งให้กล้องล็อคเป้าไปที่ตัวละคร
    /// </summary>
    public void SetFollowTarget(Transform target)
    {
        targetToFollow = target;
    }

    private void FollowTargetUpdate()
    {
        if (targetToFollow == null || cam == null) return;

        // -----------------------------------------------------------------
        // คำนวณหาตำแหน่งกล้องที่ทำให้ตัวละครเป้าหมายอยู่ตรงกลางหน้าจอพอดี
        // -----------------------------------------------------------------
        float targetY = targetToFollow.position.y;
        float camY = transform.position.y;

        Vector3 camForward = cam.transform.forward;

        // ป้องกันการหารด้วยศูนย์ หากกล้องอยู่ในแนวระนาบพอดีขนานกับพื้นดิน
        if (Mathf.Abs(camForward.y) < 0.001f) return;

        // 1. คำนวณหาอัตราส่วนระยะห่าง (t)
        float t = (targetY - camY) / camForward.y;

        // 2. โปรเจกต์พิกัด
        float desiredX = targetToFollow.position.x - (t * camForward.x);
        float desiredZ = targetToFollow.position.z - (t * camForward.z);

        // 3. ประกอบร่างพิกัดปลายทาง
        Vector3 desiredPosition = new Vector3(desiredX, camY, desiredZ);

        // เลื่อน Rig ตัวแม่พาตัวกล้องลูกเลื่อนไปหาเป้าหมายอย่างสมูทไร้รอยต่อ
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * followLerpSpeed);
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
        if (zoomAction == null) return;

        zoomValue = zoomAction.ReadValue<Vector2>();
        zoomModifier = zoomValue.y * 5f;

        if (Keyboard.current.zKey.isPressed)
            zoomModifier = -1f;
        if (Keyboard.current.xKey.isPressed)
            zoomModifier = 1f;

        cam.orthographicSize += zoomModifier * zoomSpeed;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, 4, 10);
    }
}