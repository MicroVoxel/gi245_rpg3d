using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// จัดการ Left Click: เลือกตัวละคร (single / box selection)
/// </summary>
public class LeftClick : MonoBehaviour
{
    public static LeftClick instance;

    #region === REFERENCES ===
    private Camera cam;

    [SerializeField] private LayerMask layerMask;
    [SerializeField] private RectTransform boxSelection;

    public RectTransform renderTextureUI;
    #endregion

    #region === BOX SELECTION STATE ===
    private Vector2 startPos;
    private Vector2 lastBoxAnchoredPos;

    // ไว้เช็คว่าเริ่มกดเมาส์บน UI หรือไม่ เพื่อไม่ให้ตอนปล่อยเมาส์มันทำคำสั่งทะลุ UI
    private bool isPointerOverUIOnDown;
    #endregion

    #region === UNITY CALLBACKS ===
    private void Start()
    {
        instance = this;
        cam = Camera.main;
        layerMask = LayerMask.GetMask("Ground", "Character", "Building", "Item");

        if (UIManager.instance != null)
        {
            boxSelection = UIManager.instance.SelectionBox;
        }
    }

    private void Update()
    {
        HandleMouseDown();
        HandleMouseHeld();
        HandleMouseUp();
    }
    #endregion

    #region === INPUT HANDLERS ===
    private void HandleMouseDown()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;

        // เช็คก่อนว่าเมาส์จิ้มอยู่บน UI หรือเปล่า
        isPointerOverUIOnDown = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        if (isPointerOverUIOnDown) return;

        startPos = Mouse.current.position.value;
    }

    private void HandleMouseHeld()
    {
        if (Mouse.current == null) return;

        // หากเริ่มกดจาก UI ห้ามลากกล่องเด็ดขาด
        if (!Mouse.current.leftButton.isPressed || isPointerOverUIOnDown) return;

        UpdateSelectionBox(Mouse.current.position.value);
    }

    private void HandleMouseUp()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasReleasedThisFrame) return;

        // หากเริ่มกดจาก UI ตอนปล่อยเมาส์ไม่ต้อง Raycast หาสิ่งที่อยู่ด้านหลัง
        if (isPointerOverUIOnDown) return;

        ReleaseSelectionBox(Mouse.current.position.value);
        TrySelect(Mouse.current.position.value);
    }
    #endregion

    #region === SELECTION LOGIC ===
    /// <summary>
    /// Raycast จากตำแหน่งหน้าจอเพื่อเลือก Character
    /// </summary>
    private void TrySelect(Vector2 screenPos)
    {
        if (renderTextureUI == null || cam == null) return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            renderTextureUI, screenPos, null, out Vector2 localPoint)) return;

        Vector2 normalizedPoint = Rect.PointToNormalized(renderTextureUI.rect, localPoint);
        Ray ray = cam.ViewportPointToRay(normalizedPoint);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, layerMask))
        {
            switch (hit.collider.tag)
            {
                case "Player":
                case "Hero":
                    SelectCharacter(hit);
                    break;
                case "Item":
                    SelectItem(hit);
                    break;
            }
        }

        if (PartyManager.instance == null || UIManager.instance == null) return;

        // ดึงขยะ Null หรือตัวละครที่ตายแล้วออกจาก List ของ SelectChars ก่อนตรวจสอบจำนวน
        CleanDestroyedSelections();

        // ถ้าคลิกพลาดหรือหลุดการเลือก ให้กลับไปเลือกตัวหลัก (Index 0)
        if (PartyManager.instance.SelectChars.Count == 0)
        {
            PartyManager.instance.SelectSingleHero(0);
            UIManager.instance.SetToggleAvatarWithoutNotify(0, true);
            UIManager.instance.ShowMagicToggles();
        }
    }

    /// <summary>
    /// เลือก Character และ Sync กับ UI Toggle โดยหลีกเลี่ยงการกระตุ้น Event OnValueChanged
    /// </summary>
    private void SelectCharacter(RaycastHit hit)
    {
        if (PartyManager.instance == null || UIManager.instance == null) return;

        ClearAllSelections();

        Character hero = hit.collider.GetComponent<Character>();

        // [SAFETY CHECK]: ตรวจสอบความปลอดภัยของตัวละครที่ถูกคลิกเลือก
        if (hero == null || hero.State == CharState.Die) return;

        int i = PartyManager.instance.FindIndexFromClass(hero);

        // จัดการ Data ตรงๆ ที่ PartyManager
        PartyManager.instance.SelectSingleHero(i);

        // อัปเดต UI Toggle ให้ตรงกันแบบไม่สร้าง Loop Event
        if (i < UIManager.instance.ToggleAvatar.Length)
        {
            UIManager.instance.SetToggleAvatarWithoutNotify(i, true);
        }

        UIManager.instance.ShowMagicToggles();
    }

    /// <summary>
    /// ล้าง Selection ทั้งหมด (Toggle + Ring + List)
    /// </summary>
    private void ClearAllSelections()
    {
        if (PartyManager.instance == null || UIManager.instance == null) return;

        // ปิด Toggle ทุกตัว แบบไม่ส่ง Event
        UIManager.instance.SetAllAvatarTogglesWithoutNotify(false);

        // ปิด Ring ทุกตัว (เพิ่ม Null Check ป้องกัน MissingReferenceException)
        foreach (Character c in PartyManager.instance.SelectChars)
        {
            if (c != null)
            {
                c.ToggleRingSelection(false);
            }
        }

        PartyManager.instance.SelectChars.Clear();
        UIManager.instance.ResetMagicToggles();
    }

    /// <summary>
    /// Helper: กรองข้อมูลขยะ (Null/ตัวที่ตายแล้ว) ออกจากลิสต์การเลือก เพื่อป้องกัน Exception ในสคริปต์อื่น
    /// </summary>
    private void CleanDestroyedSelections()
    {
        if (PartyManager.instance == null) return;

        List<Character> activeSelections = new List<Character>();
        foreach (Character c in PartyManager.instance.SelectChars)
        {
            if (c != null && c.State != CharState.Die)
            {
                activeSelections.Add(c);
            }
        }

        PartyManager.instance.SelectChars.Clear();
        PartyManager.instance.SelectChars.AddRange(activeSelections);
    }
    #endregion

    #region === BOX SELECTION ===
    /// <summary>
    /// อัปเดตขนาดและตำแหน่งของ Selection Box ขณะลาก
    /// </summary>
    private void UpdateSelectionBox(Vector2 mousePos)
    {
        if (boxSelection == null) return;

        if (!boxSelection.gameObject.activeInHierarchy)
            boxSelection.gameObject.SetActive(true);

        float width = mousePos.x - startPos.x;
        float height = mousePos.y - startPos.y;

        boxSelection.anchoredPosition = startPos + new Vector2(width / 2f, height / 2f);
        boxSelection.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));

        lastBoxAnchoredPos = boxSelection.anchoredPosition;
    }

    /// <summary>
    /// ปล่อย Selection Box แล้วเลือก Character ทุกตัวในกรอบ
    /// </summary>
    private void ReleaseSelectionBox(Vector2 mousePos)
    {
        if (boxSelection == null || PartyManager.instance == null || UIManager.instance == null || cam == null) return;

        boxSelection.gameObject.SetActive(false);

        Vector2 halfSize = boxSelection.sizeDelta / 2f;
        Vector2 corner1 = lastBoxAnchoredPos - halfSize; // มุมล่างซ้าย
        Vector2 corner2 = lastBoxAnchoredPos + halfSize; // มุมบนขวา

        bool anyNewCharSelect = false;

        foreach (Character member in PartyManager.instance.Members)
        {
            // 💡 [CRITICAL FIX]: ข้ามการตรวจสอบหากตัวละครถูกทำลาย (เป็น Null) หรือตายแล้ว เพื่อป้องกัน MissingReferenceException
            if (member == null || member.State == CharState.Die)
            {
                if (member != null)
                {
                    member.ToggleRingSelection(false);
                }
                continue;
            }

            Vector2 unitScreenPos = cam.WorldToScreenPoint(member.transform.position);

            bool insideX = unitScreenPos.x > corner1.x && unitScreenPos.x < corner2.x;
            bool insideY = unitScreenPos.y > corner1.y && unitScreenPos.y < corner2.y;

            if (insideX && insideY)
            {
                if (!anyNewCharSelect)
                {
                    anyNewCharSelect = true;
                    ClearAllSelections();
                }

                int i = PartyManager.instance.FindIndexFromClass(member);

                // Add เข้า List โดยตรงหากยังไม่ได้อยู่ใน List
                if (!PartyManager.instance.SelectChars.Contains(member))
                {
                    PartyManager.instance.SelectChars.Add(member);
                    member.ToggleRingSelection(true);
                }

                // Sync UI Toggle
                if (i < UIManager.instance.ToggleAvatar.Length)
                {
                    UIManager.instance.SetToggleAvatarWithoutNotify(i, true);
                }
            }
        }

        if (anyNewCharSelect)
        {
            UIManager.instance.ShowMagicToggles();
        }

        boxSelection.sizeDelta = Vector2.zero;
    }
    #endregion

    private void SelectItem(RaycastHit hit)
    {
        if (PartyManager.instance == null || UIManager.instance == null) return;

        ItemPick itemPick = hit.collider.GetComponent<ItemPick>();

        CleanDestroyedSelections();

        if (PartyManager.instance.SelectChars.Count == 0)
        {
            PartyManager.instance.SelectSingleHero(0);
            UIManager.instance.SetToggleAvatarWithoutNotify(0, true);
            UIManager.instance.ShowMagicToggles();
        }

        if (itemPick != null)
        {
            itemPick.PickUpItem();
        }
    }
}