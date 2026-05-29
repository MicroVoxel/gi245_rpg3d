using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems; // ต้องเพิ่ม Library นี้เพื่อใช้เช็ค UI

/// <summary>
/// จัดการ Right Click: สั่งการ Party (เดิน / โจมตี / คุย NPC)
/// มีการตรวจสอบสถานะ UI เพื่อป้องกันการสั่งงานทะลุ Panel และป้องกันการสั่งการตัวละครที่ตายแล้ว
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
            if (EventSystem.current != null && !EventSystem.current.IsPointerOverGameObject())
            {
                TryCommand(Mouse.current.position.value);
            }
        }
    }
    #endregion

    #region === COMMAND DISPATCH ===
    private void TryCommand(Vector2 screenPos)
    {
        if (renderTextureUI == null || cam == null) return;

        // เช็คว่าเมาส์อยู่ในขอบเขตของ RenderTexture UI หรือไม่
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            renderTextureUI, screenPos, null, out Vector2 localPoint)) return;

        Vector2 normalizedPoint = Rect.PointToNormalized(renderTextureUI.rect, localPoint);
        Ray ray = cam.ViewportPointToRay(normalizedPoint);

        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f, layerMask)) return;

        if (PartyManager.instance == null) return;

        // ดึงรายชื่อฮีโร่ที่ถูกเลือกทั้งหมดในปัจจุบัน
        List<Character> selected = PartyManager.instance.SelectChars;

        // [CRITICAL FIX]: คัดกรองเฉพาะฮีโร่ที่ยังคงมีชีวิตอยู่เท่านั้น (Alive Filter)
        // เพื่อป้องกันไม่ให้ฮีโร่ที่ตาย (State == Die หรือ HP <= 0) รับคำสั่งเคลื่อนที่หรือต่อสู้ใดๆ
        List<Character> aliveHeroes = new List<Character>();
        foreach (Character h in selected)
        {
            if (h != null && h.State != CharState.Die && h.CurHp > 0)
            {
                aliveHeroes.Add(h);
            }
        }

        // หากไม่มีฮีโร่ที่เหลือรอดชีวิตอยู่เลยในกลุ่มที่เลือก ไม่ต้องทำคำสั่งใดๆ ต่อไป
        if (aliveHeroes.Count == 0) return;

        switch (hit.collider.tag)
        {
            case "Ground":
                CommandWalk(hit, aliveHeroes);
                break;

            case "Enemy":
                CommandAttack(hit, aliveHeroes);
                break;

            case "NPC":
                CommandTalkToNPC(hit, aliveHeroes);
                break;

            case "Hero":
                CommandInteractWithHero(hit, aliveHeroes);
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

        if (VFXManager.instance != null)
        {
            SpawnVFX(hit.point, VFXManager.instance.DoubleRingMarker);
        }
    }

    private void CommandAttack(RaycastHit hit, List<Character> heroes)
    {
        Character target = hit.collider.GetComponent<Character>();
        if (target == null || target.State == CharState.Die) return; // ป้องกันการสั่งโจมตีศพที่ตายแล้ว

        foreach (Character h in heroes)
        {
            if (h != null)
                h.ToAttackCharacter(target);
        }
    }

    private void CommandTalkToNPC(RaycastHit hit, List<Character> heroes)
    {
        if (heroes.Count <= 0) return;
        if (UIManager.instance != null && UIManager.instance.IsDialogueOpen) return;

        Character npc = hit.collider.GetComponent<Character>();
        if (npc == null || npc.State == CharState.Die) return; // ป้องกันการไปคุยกับ NPC ที่ตายแล้ว

        // สั่งให้ฮีโร่ที่มีชีวิตอยู่ตัวแรกเดินไปคุย
        heroes[0].ToTalkToNPC(npc);
    }

    private void CommandInteractWithHero(RaycastHit hit, List<Character> heroes)
    {
        if (heroes.Count <= 0) return;
        if (UIManager.instance != null && UIManager.instance.IsDialogueOpen) return;

        Character targetHero = hit.collider.GetComponent<Character>();
        if (targetHero == null || targetHero.State == CharState.Die) return; // ป้องกันการกดคุยกับฮีโร่ที่ตายแล้ว
        if (PartyManager.instance != null && PartyManager.instance.IsMember(targetHero)) return;
        if (PartyManager.instance != null && PartyManager.instance.Members.Count >= 6) return;

        // สั่งให้ฮีโร่ที่มีชีวิตอยู่ตัวแรกเดินไปคุยเพื่อเชิญเข้าตระกูล/ปาร์ตี้
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