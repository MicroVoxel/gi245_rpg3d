using UnityEngine;

public class RightClick : MonoBehaviour
{
    public static RightClick instance;

    private Camera cam;
    public LayerMask layerMask;

    private LeftClick leftClick;

    public RectTransform renderTextureUI;

    private void Awake()
    {
        leftClick = GetComponent<LeftClick>();
    }

    void Start()
    {
        instance = this;
        cam = Camera.main;
        layerMask = LayerMask.GetMask("Ground", "Character", "Building", "Item");
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(1))
        {
            TryCommand(Input.mousePosition);
        }
    }

    private void CommandToWalk(RaycastHit hit, Character c)
    {
        if (c != null)
        {
            c.WalkToPosition(hit.point);
        }
    }

    private void TryCommand(Vector2 screenPos)
    {
        //Ray ray = cam.ScreenPointToRay(screenPos);
        //RaycastHit hit;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(renderTextureUI, screenPos, null, out Vector2 localPoint))
        {
            // 2. แปลงเป็นพิกัด Normalized (0 ถึง 1)
            Vector2 normalizedPoint = Rect.PointToNormalized(renderTextureUI.rect, localPoint);

            // 3. สร้าง Ray จากกล้องของ RenderTexture โดยใช้พิกัด Viewport (Normalized)
            Ray ray = cam.ViewportPointToRay(normalizedPoint);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000, layerMask))
            {
                switch (hit.collider.tag)
                {
                    case "Ground":
                        CommandToWalk(hit, leftClick.CurChar);
                        break;
                }
            }
        }
    }


}
