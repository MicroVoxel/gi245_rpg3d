using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// จัดการ Shortcut คีย์ลัดทั้งหมดในเกม (Input Controller)
/// ปรับปรุงให้สั่ง Toggle UI ของปุ่ม Pause ไปพร้อมกับการหยุดเกม
/// </summary>
public class ShortcutManager : MonoBehaviour
{
    public static ShortcutManager instance;

    [Header("UI Shortcuts")]
    [SerializeField] private Key inventoryKey = Key.I;
    [SerializeField] private Key characterKey = Key.C;

    [Header("Game State Shortcuts")]
    [SerializeField] private Key pauseKey = Key.Space;
    [SerializeField] private UnityEngine.UI.Toggle pauseToggle; // ลาก Toggle รูปตามาใส่ที่นี่

    [Header("Magic Skill Keys")]
    private Key[] digitKeys = new Key[]
    {
        Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4
    };

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        HandleUIShortcuts();
        HandleGameStateShortcuts();
        HandleMagicShortcuts();
    }

    private void HandleUIShortcuts()
    {
        if (Keyboard.current[inventoryKey].wasPressedThisFrame)
            UIManager.instance.ToggleInventoryPanel();

        if (Keyboard.current[characterKey].wasPressedThisFrame)
            UIManager.instance.ToggleCharPanel();
    }

    private void HandleGameStateShortcuts()
    {
        if (Keyboard.current[pauseKey].wasPressedThisFrame)
        {
            // เช็คสถานะปัจจุบันจาก Toggle
            bool shouldPause = !pauseToggle.isOn;

            // อัปเดต UI Toggle ให้ Sync กันแบบไม่มี Event (ใช้ SetIsOnWithoutNotify ตามที่เราตกลงกัน)
            UIManager.instance.SetToggleWithoutNotify(pauseToggle, shouldPause);

            // สั่งหยุดเกม
            UIManager.instance.PauseUnpause(shouldPause);
        }
    }

    private void HandleMagicShortcuts()
    {
        for (int i = 0; i < digitKeys.Length; i++)
        {
            if (Keyboard.current[digitKeys[i]].wasPressedThisFrame)
            {
                UIManager.instance.SelectMagicSkill(i);
            }
        }
    }
}