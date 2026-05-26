using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems; // ต้องเพิ่ม Library นี้เพื่อใช้เช็ค UI

/// <summary>
/// จัดการ Right Click: สั่งการ Party (เดิน / โจมตี / คุย NPC)
/// มีการตรวจสอบสถานะ UI เพื่อป้องกันการสั่งงานทะลุ Panel
/// </summary>
public class RightClick : MonoBehaviour
{
    public static RightClick instance;

    #region === REFERENCES ===
    private Camera cam;

    [SerializeField] public LayerMask layerMask;
    [SerializeField] public RectTransform renderTextureUI;
    #endregion

    #region === UNITY CALLBACKS ===
    private void Start()
    {
        instance = this;
        cam = Camera.main;
        layerMask = LayerMask.GetMask("Ground", "Character", "Building", "Item");
    }

    private void Update()
    {
        // เช็คว่ากดเมาส์ขวา และเมาส์ไม่อยู่บน UI ของ Unity (เช่น ปุ่ม หรือ Panel)
        if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                TryCommand(Mouse.current.position.value);
            }
        }
    }
    #endregion

    #region === COMMAND DISPATCH ===
    private void TryCommand(Vector2 screenPos)
    {
        // เช็คว่าเมาส์อยู่ในขอบเขตของ RenderTexture UI หรือไม่
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            renderTextureUI, screenPos, null, out Vector2 localPoint)) return;

        Vector2 normalizedPoint = Rect.PointToNormalized(renderTextureUI.rect, localPoint);
        Ray ray = cam.ViewportPointToRay(normalizedPoint);

        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f, layerMask)) return;

        List<Character> selected = PartyManager.instance.SelectChars;

        switch (hit.collider.tag)
        {
            case "Ground":
                CommandWalk(hit, selected);
                break;

            case "Enemy":
                CommandAttack(hit, selected);
                break;

            case "NPC":
                CommandTalkToNPC(hit, selected);
                break;

            case "Hero":
                CommandInteractWithHero(hit, selected);
                break;
        }
    }
    #endregion

    #region === COMMANDS ===
    private void CommandWalk(RaycastHit hit, List<Character> heroes)
    {
        foreach (Character h in heroes)
        {
            if (h != null)
                h.WalkToPosition(hit.point);
        }

        SpawnVFX(hit.point, VFXManager.instance.DoubleRingMarker);
    }

    private void CommandAttack(RaycastHit hit, List<Character> heroes)
    {
        Character target = hit.collider.GetComponent<Character>();
        if (target == null) return;

        foreach (Character h in heroes)
        {
            if (h != null)
                h.ToAttackCharacter(target);
        }
    }

    private void CommandTalkToNPC(RaycastHit hit, List<Character> heroes)
    {
        if (heroes.Count <= 0) return;
        if (UIManager.instance.IsDialogueOpen) return;

        Character npc = hit.collider.GetComponent<Character>();
        if (npc == null) return;

        heroes[0].ToTalkToNPC(npc);
    }

    private void CommandInteractWithHero(RaycastHit hit, List<Character> heroes)
    {
        if (heroes.Count <= 0) return;
        if (UIManager.instance.IsDialogueOpen) return;

        Character targetHero = hit.collider.GetComponent<Character>();
        if (targetHero == null) return;
        if (PartyManager.instance.IsMember(targetHero)) return;
        if (PartyManager.instance.Members.Count >= 6) return;

        heroes[0].ToTalkToNPC(targetHero);
    }
    #endregion

    #region === VFX ===
    private void SpawnVFX(Vector3 pos, GameObject vfxPrefab)
    {
        if (vfxPrefab == null) return;
        Instantiate(vfxPrefab, pos + new Vector3(0f, 0.1f, 0f), Quaternion.identity);
    }
    #endregion
}