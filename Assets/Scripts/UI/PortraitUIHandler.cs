using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// จัดการ Event ของเมาส์บนรูป Portrait ตัวละคร (ดักจับ Click, Double Click, Drag & Drop)
/// รองรับการทำงานร่วมกับ Layout Group อย่างสมบูรณ์
/// </summary>
[RequireComponent(typeof(CanvasGroup))] // บังคับให้มี CanvasGroup เพื่อใช้ปรับความโปร่งใส
public class PortraitUIHandler : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private int myIndex = -1;
    private RectTransform myRect;
    private CanvasGroup myCanvasGroup;

    // Static fields สำหรับจัดการชิ้นส่วนกราฟิกจำลองตอนลาก (Ghost Image)
    private static GameObject dragGhostObj;
    private static RectTransform dragGhostRect;
    private static Image dragGhostImage;

    private void Awake()
    {
        myRect = GetComponent<RectTransform>();
        myCanvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// UIManager จะเป็นคนเรียกฟังก์ชันนี้เพื่อบอกว่ากรอบนี้คือตัวละคร Index ที่เท่าไหร่
    /// </summary>
    public void SetupIndex(int index)
    {
        myIndex = index;
    }

    #region === CLICK INTERACTION ===
    public void OnPointerClick(PointerEventData eventData)
    {
        // ป้องกันการทริกเกอร์คลิกหากเพิ่งลากเสร็จ
        if (eventData.dragging) return;

        // ถ้า UI นี้ยังไม่ได้ผูกกับตัวละคร ข้ามไป
        if (myIndex < 0 || myIndex >= PartyManager.instance.Members.Count) return;

        // --- DOUBLE CLICK: Camera Lock ---
        if (eventData.clickCount == 2)
        {
            Transform target = PartyManager.instance.Members[myIndex].transform;
            CameraController.instance.SetFollowTarget(target);
            return;
        }

        // --- SINGLE CLICK (with or without Shift) ---
        if (eventData.clickCount == 1)
        {
            // ยกเลิกกล้องล็อคเป้า เมื่อมีการคลิกเปลี่ยนตัว
            CameraController.instance.SetFollowTarget(null);

            if (Keyboard.current.shiftKey.isPressed)
            {
                // Shift + Click: เพิ่มเข้ากลุ่ม
                PartyManager.instance.ToggleHeroSelectionMulti(myIndex);
            }
            else
            {
                // Click ธรรมดา: เลือกตัวเดียว
                PartyManager.instance.SelectSingleHero(myIndex);
            }

            // สั่งอัปเดตแสงสี Toggle
            UIManager.instance.SyncToggleVisuals();
        }
    }
    #endregion

    #region === DRAG & DROP (SWAP FORMATION) ===
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (myIndex < 0 || myIndex >= PartyManager.instance.Members.Count) return;

        // 1. สร้างภาพจำลอง (Ghost) ไว้ลากเล่น โดยไม่ยุ่งกับวัตถุจริงที่อยู่ใน Layout Group
        if (dragGhostObj == null)
        {
            dragGhostObj = new GameObject("PortraitGhost");
            dragGhostObj.transform.SetParent(UIManager.instance.transform); // ให้อยู่ระดับรากของ UI
            dragGhostRect = dragGhostObj.AddComponent<RectTransform>();
            dragGhostImage = dragGhostObj.AddComponent<Image>();

            // ทำให้แสงส่องผ่านได้ จะได้ไม่ขวาง Raycast ตอนปล่อยเมาส์เพื่อตรวจจับเป้าหมาย
            CanvasGroup cg = dragGhostObj.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            cg.alpha = 0.7f; // ให้ภาพตอนลากโปร่งแสงเล็กน้อย
        }

        dragGhostObj.SetActive(true);
        // ดึงให้ภาพที่ถูกลากอยู่ชั้นบนสุดเสมอ จะได้ไม่โดน Panel อื่นบัง
        dragGhostObj.transform.SetAsLastSibling();

        dragGhostImage.sprite = PartyManager.instance.Members[myIndex].AvartarPic;

        // ปรับขนาดให้เท่าต้นฉบับ
        dragGhostRect.sizeDelta = myRect.sizeDelta;
        UpdateGhostPosition(eventData.position);

        // ทำให้ภาพต้นฉบับที่อยู่ใน Layout Group จางลง เพื่อแสดงสถานะว่ากำลังถูกดึงออกไป
        myCanvasGroup.alpha = 0.3f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragGhostObj != null && dragGhostObj.activeSelf)
        {
            UpdateGhostPosition(eventData.position);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragGhostObj != null)
        {
            dragGhostObj.SetActive(false);
        }

        // คืนค่าความชัดของรูปภาพต้นฉบับกลับมาเหมือนเดิม
        myCanvasGroup.alpha = 1f;

        // 2. ใช้ Raycast หาสิ่งที่อยู่ใต้เมาส์ตอนปล่อย ว่าโดน Portrait ช่องอื่นไหม
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            PortraitUIHandler targetPortrait = result.gameObject.GetComponentInParent<PortraitUIHandler>();

            // ถ้าไปปล่อยทับ Portrait ช่องอื่น และไม่ใช่ช่องตัวเอง
            if (targetPortrait != null && targetPortrait != this && targetPortrait.myIndex != -1)
            {
                // สั่งสลับตำแหน่ง (Data-Driven Swap)
                PartyManager.instance.SwapPartyMembers(this.myIndex, targetPortrait.myIndex);
                break;
            }
        }
    }

    private void UpdateGhostPosition(Vector2 screenPosition)
    {
        // ย้ายภาพจำลองตามเมาส์
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            UIManager.instance.transform as RectTransform,
            screenPosition,
            null,
            out Vector2 localPoint);

        dragGhostRect.localPosition = localPoint;
    }
    #endregion
}