using UnityEngine;
using UnityEngine.Rendering;

public class LeftClick : MonoBehaviour
{
    public static LeftClick instance;

    private Camera cam;

    [SerializeField] private Character curChar;
    public Character CurChar { get { return curChar; } }

    [SerializeField] private LayerMask layerMask;

    public RectTransform renderTextureUI;

    void Start()
    {
        instance = this;
        cam = Camera.main;
        layerMask = LayerMask.GetMask("Ground", "Character", "Building", "Item");
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ClearEverything();
        }

        if (Input.GetMouseButtonUp(0))
        {
            TrySelect(Input.mousePosition);
        }
    }

    private void SelectCharacter(RaycastHit hit)
    {
        curChar = hit.collider.GetComponent<Character>();
        Debug.Log("Selected Char: " + hit.collider.gameObject);

        if (curChar != null)
        {
            curChar.ToggleringSelection(true);
        }
    }

    private void TrySelect(Vector2 screenPos)
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
                    case "Player":
                    case "Hero":
                        SelectCharacter(hit);
                        break;
                }
            }

        }
            
    }

    private void ClearRingSelection()
    {
        if (curChar != null)
        {
            curChar.ToggleringSelection(false);
        }
    }

    private void ClearEverything()
    {
        ClearRingSelection();
        curChar = null;
    }
}
