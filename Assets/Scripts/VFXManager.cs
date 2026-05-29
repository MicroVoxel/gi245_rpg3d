using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// VFXManager: ผู้ควบคุมเอฟเฟกต์และทัศนียภาพทางเวทมนตร์ทั้งหมดภายในเกม 
/// รองรับกระสุนนำวิถีติดตามเป้าหมายแบบไดนามิก (Homing Projectile) และระบบสลายตัวตามธรรมชาติ (Fade Out)
/// </summary>
public class VFXManager : MonoBehaviour
{
    public static VFXManager instance;

    #region === REFERENCES ===
    [Header("Movement Markers")]
    [SerializeField] private GameObject doubleRingMarker;
    public GameObject DoubleRingMarker => doubleRingMarker;

    [Header("Visual Effects Databases")]
    [SerializeField] private GameObject[] magicVFX;
    public GameObject[] MagicVFX => magicVFX;

    [SerializeField] private MagicData[] magicDatas;
    public MagicData[] MagicDatas => magicDatas;
    #endregion

    #region === UNITY CALLBACKS ===
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void OnEnable()
    {
        MyActions.onLoadMagic += LoadMagic;
        MyActions.onShootMagic += ShootMagic;
        MyActions.onCreateMagic += CreateMagic;
    }

    private void OnDisable()
    {
        MyActions.onLoadMagic -= LoadMagic;
        MyActions.onShootMagic -= ShootMagic;
        MyActions.onCreateMagic -= CreateMagic;
    }
    #endregion

    #region === VFX LOGIC ===
    /// <summary>
    /// โหลดวงเวทย์เตรียมการร่ายชั่วคราว ณ ตำแหน่งร่าย พร้อมผูกติดเข้ากับตัวละครผู้ร่าย
    /// </summary>
    public void LoadMagic(int id, Vector3 posA, float time)
    {
        if (id < 0 || id >= magicVFX.Length || magicVFX[id] == null) return;

        GameObject objLoad = Instantiate(magicVFX[id], posA, Quaternion.identity);

        // [BUG FIX #6] ใช้ Character.AllCharacters แทน FindObjectsByType ที่แพงกว่า
        Character caster = FindClosestCharacter(posA, 2.0f);
        if (caster != null)
        {
            objLoad.transform.SetParent(caster.transform);
        }

        Destroy(objLoad, time);
    }

    /// <summary>
    /// สั่งเสกหรือยิงเวทมนตร์ โดยรองรับระบบนำวิถีติดตามตัวละคร และเอฟเฟกต์สถานะติดตามเป้าหมายแบบเรียลไทม์
    /// </summary>
    public void ShootMagic(int id, Vector3 posA, Vector3 posB, float time)
    {
        if (id < 0 || id >= magicVFX.Length || magicVFX[id] == null) return;

        // [BUG FIX #9] ดึง MagicData และใช้ Type เป็น routing หลักแทน time threshold
        // เดิม: time <= 0.05f เป็นตัวตัดสิน → SpawnOnEnemy ที่มี ShootTime=1.5 ถูกเข้าใจผิดเป็น Projectile
        // ใหม่: MagicType เป็นตัวตัดสินเสมอ ทำให้ SpawnOnEnemy ทุก ShootTime ทำงานถูกต้อง
        MagicData data = FindMagicDataByShootId(id);
        if (data == null)
        {
            Debug.LogWarning($"[VFXManager] ShootMagic: ไม่พบ MagicData สำหรับ shootId={id} — ข้าม VFX ชิ้นนี้ไป กรุณาตรวจสอบการกำหนดค่าใน Inspector");
            return;
        }

        MagicType magicType = data.type;

        // [BUG FIX #6] ใช้ Character.AllCharacters แทน FindObjectsByType
        Character targetChar = FindClosestCharacter(posB, 2.5f);

        // [PROJECTILE ONLY]: มีเพียง Projectile เท่านั้นที่บินข้ามอากาศ
        // SpawnOnEnemy / Buff / Debuff ทั้งหมดให้ spawn ทันทีที่เป้าหมายเสมอ ไม่ว่า ShootTime จะเป็นเท่าไร
        if (magicType == MagicType.Projectile)
        {
            GameObject objShoot = Instantiate(magicVFX[id], posA, Quaternion.identity);

            if (posA != posB)
            {
                objShoot.transform.LookAt(posB);
            }

            StartCoroutine(MoveProjectileCoroutine(objShoot, posA, posB, targetChar, time));
            return;
        }

        // [SPAWN_ON_ENEMY / BUFF / DEBUFF]: spawn ทันที ณ ตำแหน่งเป้าหมาย
        Vector3 spawnPos = posB;

        // SpawnOnEnemy + Debuff: ย้ายพิกัดลงที่เท้าตัวละครเป้าหมาย ป้องกัน VFX ลอยฟ้า
        // Buff ไม่ต้องทำเพราะ posB == posA (หาตัวเองเป็นเป้า)
        if (magicType != MagicType.Buff && targetChar != null)
        {
            spawnPos = targetChar.transform.position;
        }

        GameObject objInstant = Instantiate(magicVFX[id], spawnPos, Quaternion.identity);

        // SetParent เฉพาะ Buff และ Debuff ให้ VFX ขยับตามตัวละคร
        // SpawnOnEnemy (วงระเบิด, สายฟ้า ฯลฯ) ตรึงไว้กับพื้น ไม่ขยับตาม
        if ((magicType == MagicType.Buff || magicType == MagicType.Debuff) && targetChar != null)
        {
            objInstant.transform.SetParent(targetChar.transform);
        }

        Destroy(objInstant, 2f);
    }

    /// <summary>
    /// คอร์รูทีนสำหรับควบคุมทิศทางกระสุนนำวิถี (Homing Tracker)
    /// </summary>
    private IEnumerator MoveProjectileCoroutine(GameObject projectile, Vector3 start, Vector3 staticEnd, Character target, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (projectile == null) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // [HOMING SYSTEM]: อัปเดตพิกัดปลายทางแบบ real-time ตามการเคลื่อนที่ของเป้าหมาย
            Vector3 currentTargetPos = (target != null && target.State != CharState.Die)
                ? target.transform.position + Vector3.up * 1f
                : staticEnd;

            projectile.transform.position = Vector3.Lerp(start, currentTargetPos, t);

            if (projectile.transform.position != currentTargetPos)
            {
                projectile.transform.LookAt(currentTargetPos);
            }

            yield return null;
        }

        // [FADE OUT]: หยุดพ่นอนุภาค ซ่อน Mesh แล้วรอสลายตัวตามธรรมชาติ
        if (projectile != null)
        {
            ParticleSystem[] particles = projectile.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particles)
            {
                ps.Stop();
            }

            MeshRenderer[] renderers = projectile.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer mr in renderers)
            {
                mr.enabled = false;
            }

            Destroy(projectile, 2f);
        }
    }

    /// <summary>
    /// ค้นหาตัวละครที่อยู่ใกล้ตำแหน่งที่กำหนดมากที่สุด
    /// [BUG FIX #6] ใช้ Character.AllCharacters (HashSet) แทน FindObjectsByType ที่ต้องสแกนทั้ง scene
    /// </summary>
    private Character FindClosestCharacter(Vector3 searchPosition, float maxDistance)
    {
        Character closestCharacter = null;
        float closestDistance = maxDistance;

        foreach (Character character in Character.AllCharacters)
        {
            // AllCharacters ลงทะเบียนผ่าน OnEnable/OnDisable จึงไม่มี null entry
            // แต่ตรวจสอบ Die state ยังคงจำเป็น
            if (character.State == CharState.Die) continue;

            float distance = Vector3.Distance(searchPosition, character.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCharacter = character;
            }
        }

        return closestCharacter;
    }

    /// <summary>
    /// สร้างข้อมูลเวทมนตร์รันไทม์ขึ้นมาจากเทมเพลต ScriptableObject ภายในอาเรย์
    /// </summary>
    public Magic CreateMagic(int id)
    {
        if (id < 0 || id >= magicDatas.Length || magicDatas[id] == null)
        {
            Debug.LogError($"[VFXManager] ไม่พบข้อมูล Magic Data ที่ระบุรหัส ID: {id} ในฐานข้อมูล!");
            return null;
        }

        return new Magic(magicDatas[id]);
    }

    /// <summary>
    /// ค้นหาข้อมูล MagicData ต้นแบบเพื่อตรวจสอบประเภทของสกิล
    /// ตรวจ Index ก่อน (O1) แล้ว fallback เป็น Linear Search เพื่อความปลอดภัย
    /// </summary>
    private MagicData FindMagicDataByShootId(int shootId)
    {
        if (magicDatas != null && shootId >= 0 && shootId < magicDatas.Length)
        {
            if (magicDatas[shootId] != null && magicDatas[shootId].shootId == shootId)
            {
                return magicDatas[shootId];
            }
        }

        if (magicDatas != null)
        {
            foreach (MagicData data in magicDatas)
            {
                if (data != null && data.shootId == shootId)
                {
                    return data;
                }
            }
        }

        return null;
    }
    #endregion
}