using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#region Enums
public enum CharState
{
    Idle,
    Walk,
    WalkToEnemy,
    Attack,
    WalkToMagicCast,
    MagicCast,
    Hit,
    Die,
    WalkToNPC
}
#endregion

public abstract class Character : MonoBehaviour
{
    #region === COMPONENTS ===
    protected NavMeshAgent navAgent;
    protected Collider _collider;
    protected Animator anim;

    public Animator Anim { get { return anim; } }
    #endregion

    #region === BASIC INFO ===
    [Header("Character Info")]
    [SerializeField] protected string charName;
    [SerializeField] protected Sprite avatarPic;

    public string CharName { get { return charName; } }
    public Sprite AvartarPic { get { return avatarPic; } }
    #endregion

    #region === STATE ===
    [Header("State")]
    [SerializeField] protected CharState state;
    [SerializeField] protected GameObject ringSelection;

    public CharState State { get { return state; } }
    public GameObject RingSelection { get { return ringSelection; } }
    #endregion

    #region === STATS ===
    [Header("Stats")]
    [SerializeField] protected int curHp = 10;
    [SerializeField] protected int maxHP = 100;

    [SerializeField] protected int baseDefense = 5;

    public int CurHp { get { return curHp; } set { curHp = value; } }
    public int MaxHP { get { return maxHP; } }
    public int BaseDefense { get { return baseDefense; } set { baseDefense = value; } }
    #endregion

    #region === TARGETING ===
    [Header("Targeting")]
    [SerializeField] protected Character curCharTarget;
    [SerializeField] protected float findingRange = 20f;

    public Character CurCharTarget { get { return curCharTarget; } set { curCharTarget = value; } }
    public float FindingRange { get { return findingRange; } }
    #endregion

    #region === COMBAT (NORMAL) ===
    [Header("Combat")]
    [SerializeField] protected float attackRange = 2f;
    [SerializeField] protected int attackDamage = 3;
    [SerializeField] protected float attackCooldown = 2f;
    [SerializeField] protected float attackTimer = 0f;

    public float AttackRange { get { return attackRange; } }
    public int AttackDamage { get { return attackDamage; } set { attackDamage = value; } }
    #endregion

    #region === MAGIC ===
    [Header("Magic")]
    [Tooltip("ลากไฟล์ 'MagicData' (ScriptableObject) ที่ต้องการให้เป็นเวทเริ่มต้นมาใส่ที่นี่ได้เลย")]
    [SerializeField] protected List<MagicData> startingMagics = new List<MagicData>();
    public List<MagicData> StartingMagics { get { return startingMagics; } }

    [SerializeField] protected List<Magic> magicSkills = new List<Magic>();
    [SerializeField] protected Magic curMagicCast = null;
    [SerializeField] protected bool isMagicMode = false;

    public List<Magic> MagicSkills { get { return magicSkills; } set { magicSkills = value; } }
    public Magic CurMagicCast { get { return curMagicCast; } set { curMagicCast = value; } }
    public bool IsMagicMode { get { return isMagicMode; } set { isMagicMode = value; } }

    private Coroutine activeBuffCoroutine;
    private Coroutine activeDebuffCoroutine;
    #endregion

    #region === INVENTORY ===
    [Header("Inventory")]
    [SerializeField] protected Item[] inventoryItems;

    public Item[] InventoryItems { get { return inventoryItems; } set { inventoryItems = value; } }
    #endregion

    #region === EQUIPMENT ===
    [Header("Equipment")]

    [SerializeField] protected Item mainWeapon;
    [SerializeField] protected Transform weaponHand;
    [SerializeField] protected GameObject weaponObj;
    [SerializeField] protected int attackPower = 0;

    [SerializeField] protected Item shield;
    [SerializeField] protected Transform shieldHand;
    [SerializeField] protected GameObject shieldObj;
    [SerializeField] protected int defensePower = 0;

    public Item MainWeapon { get { return mainWeapon; } set { mainWeapon = value; } }
    public Item Shield { get { return shield; } set { shield = value; } }
    public int DefensePower { get { return defensePower; } set { defensePower = value; } }
    #endregion

    #region === SOUND EFFECTS (SFX) ===
    [Header("Sound Effects (SFX)")]
    [Tooltip("ใส่ Index ของเสียงโจมตีตามที่ตั้งไว้ใน AudioManager (ใส่ -1 หากไม่มีเสียง)")]
    [SerializeField] protected int attackSfxIndex = -1;
    [Tooltip("ใส่ Index ของเสียงตอนโดนโจมตี")]
    [SerializeField] protected int hitSfxIndex = -1;
    [Tooltip("ใส่ Index ของเสียงตาย")]
    [SerializeField] protected int dieSfxIndex = -1;
    [Tooltip("ใส่ Index ของเสียงร่ายเวทย์")]
    [SerializeField] protected int magicCastSfxIndex = -1;
    #endregion

    #region === MANAGERS ===
    protected UIManager uiManager;
    protected InventoryManager invManager;
    protected PartyManager partyManager;
    #endregion

    // =====================================================================
    // [BUG FIX #6] ระบบลงทะเบียนตัวละครแบบ Static แทน FindObjectsByType
    // ลดต้นทุนการค้นหาจาก O(n) ทุก frame เหลือ O(1) ต่อการเข้า/ออก Scene
    // =====================================================================
    #region === STATIC REGISTRY ===
    private static readonly HashSet<Character> _allCharacters = new HashSet<Character>();

    /// <summary>
    /// คอลเลกชันตัวละครทั้งหมดที่ active อยู่ในฉาก — ใช้แทน FindObjectsByType ที่แพงกว่า
    /// </summary>
    public static IReadOnlyCollection<Character> AllCharacters => _allCharacters;
    #endregion

    #region === UNITY CALLBACKS ===
    protected virtual void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        _collider = GetComponent<Collider>();
    }

    // [BUG FIX #6] ลงทะเบียนตอน Enable / ถอนทะเบียนตอน Disable อัตโนมัติ
    protected virtual void OnEnable()
    {
        _allCharacters.Add(this);
    }

    protected virtual void OnDisable()
    {
        _allCharacters.Remove(this);
    }
    #endregion

    #region === SOUND HELPER ===
    protected void PlayCharacterSFX(int sfxIndex)
    {
        if (sfxIndex >= 0 && AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFXOneShot(sfxIndex);
        }
    }
    #endregion

    #region === INIT & STATE ===
    public virtual void CharInit(UIManager uiM, InventoryManager invM, PartyManager partyM)
    {
        uiManager = uiM;
        invManager = invM;
        partyManager = partyM;

        inventoryItems = new Item[InventoryManager.INVENTORY_CAPACITY];

        magicSkills.Clear();
        if (startingMagics != null)
        {
            foreach (MagicData data in startingMagics)
            {
                if (data != null)
                {
                    magicSkills.Add(new Magic(data));
                }
            }
        }

        if (mainWeapon != null && string.IsNullOrEmpty(mainWeapon.ItemName)) mainWeapon = null;
        if (shield != null && string.IsNullOrEmpty(shield.ItemName)) shield = null;

        Item startingWeapon = mainWeapon;
        Item startingShield = shield;

        mainWeapon = null;
        shield = null;
        attackPower = 0;
        defensePower = 0;

        if (startingWeapon != null) EquipWeapon(startingWeapon);
        if (startingShield != null) EquipShield(startingShield);
    }

    public void SetState(CharState s)
    {
        state = s;

        if (state == CharState.Idle)
        {
            navAgent.isStopped = true;
            navAgent.ResetPath();
        }
    }
    #endregion

    #region === MOVEMENT ===
    public void WalkToPosition(Vector3 des)
    {
        if (state == CharState.MagicCast) return;

        if (navAgent != null)
        {
            navAgent.SetDestination(des);
            navAgent.isStopped = false;
        }
        SetState(CharState.Walk);
    }

    protected void WalkUpdate()
    {
        float distance = Vector3.Distance(transform.position, navAgent.destination);

        if (distance <= navAgent.stoppingDistance)
        {
            SetState(CharState.Idle);
        }
    }

    public void ToggleRingSelection(bool flag)
    {
        ringSelection.SetActive(flag);
    }
    #endregion

    #region === COMBAT LOGIC ===
    public void ToAttackCharacter(Character target)
    {
        if (curHp <= 0 || state == CharState.Die) return;
        if (state == CharState.MagicCast) return;

        curCharTarget = target;

        navAgent.SetDestination(target.transform.position);
        navAgent.isStopped = false;

        if (isMagicMode && state != CharState.MagicCast)
        {
            SetState(CharState.WalkToMagicCast);
        }
        else
        {
            SetState(CharState.WalkToEnemy);
        }
    }

    protected void WalkToEnemyUpdate()
    {
        if (curCharTarget == null)
        {
            SetState(CharState.Idle);
            return;
        }

        navAgent.SetDestination(curCharTarget.transform.position);

        float distance = Vector3.Distance(transform.position, curCharTarget.transform.position);

        if (distance <= attackRange)
        {
            // [BUG FIX #3] Reset timer ก่อนเข้า Attack state เพื่อไม่ให้ cooldown ค้างจากรอบก่อน
            // ทำให้การโจมตีครั้งแรกหลังเดินเข้าถึงมีความสม่ำเสมอเสมอ
            attackTimer = 0f;
            SetState(CharState.Attack);
            Attack();
        }
    }

    protected void Attack()
    {
        transform.LookAt(curCharTarget.transform);
        anim.SetTrigger("Attack");

        float n = Random.Range(0, 4);
        anim.SetFloat("AttackValue", n);

        PlayCharacterSFX(attackSfxIndex);

        AttackLogic();
    }

    protected void AttackUpdate()
    {
        if (curCharTarget == null) return;

        if (curCharTarget.CurHp <= 0)
        {
            SetState(CharState.Idle);
            curCharTarget = null;
            return;
        }

        navAgent.isStopped = true;

        attackTimer += Time.deltaTime;
        if (attackTimer > attackCooldown)
        {
            attackTimer = 0;
            Attack();
        }

        float distance = Vector3.Distance(transform.position, curCharTarget.transform.position);

        if (distance > attackRange)
        {
            SetState(CharState.WalkToEnemy);
            navAgent.SetDestination(curCharTarget.transform.position);
            navAgent.isStopped = false;
        }
    }

    protected void AttackLogic()
    {
        // [BUG FIX #7] curCharTarget เป็น Character อยู่แล้ว ไม่จำเป็นต้อง GetComponent ซ้ำ
        // GetComponent<Character>() บน object ที่เป็น Character อยู่แล้วสิ้นเปลืองโดยเปล่าประโยชน์
        if (curCharTarget == null) return;

        int totalAtkDmg = attackDamage + attackPower;
        curCharTarget.ReceiveDamage(totalAtkDmg);
    }
    #endregion

    #region === DAMAGE & LIFE ===
    protected virtual IEnumerator DestroyObject()
    {
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }

    protected virtual void Die()
    {
        navAgent.isStopped = true;
        SetState(CharState.Die);

        anim.SetTrigger("Die");

        if (_collider != null)
        {
            _collider.enabled = false;
        }

        PlayCharacterSFX(dieSfxIndex);

        invManager.SpawnDropInventory(inventoryItems, transform.position);

        Vector3 weaponDropPos = transform.position + transform.forward * 0.5f + transform.right * 0.3f;
        Vector3 shieldDropPos = transform.position + transform.forward * 0.5f - transform.right * 0.3f;

        if (mainWeapon != null) invManager.SpawnDropItem(mainWeapon, weaponDropPos);
        if (shield != null) invManager.SpawnDropItem(shield, shieldDropPos);

        StartCoroutine(DestroyObject());
    }

    public void ReceiveDamage(int dmg)
    {
        if (curHp <= 0 || state == CharState.Die) return;

        int totalDefense = baseDefense + defensePower;
        int damageAfter = dmg - totalDefense;

        if (damageAfter < 0)
        {
            damageAfter = 0;
        }

        curHp -= damageAfter;

        PlayCharacterSFX(hitSfxIndex);

        if (curHp <= 0)
        {
            curHp = 0;
            Die();
        }
    }

    public void Recover(int n)
    {
        curHp += n;
        if (curHp > maxHP) { curHp = maxHP; }
    }

    public bool IsMyEnemy(string targetTag)
    {
        string myTag = gameObject.tag;

        if ((myTag == "Hero" || myTag == "Player") && targetTag == "Enemy")
            return true;

        if (myTag == "Enemy" && (targetTag == "Player" || targetTag == "Hero"))
            return true;

        return false;
    }
    #endregion

    #region === STATUS BUFF & DEBUFF SYSTEM ===
    public void ApplyBuff(int amount, float duration)
    {
        // [BUG FIX #5] Guard ป้องกัน duration = 0 ที่ทำให้บัฟหายทันทีใน frame ถัดไป
        if (duration <= 0f)
        {
            Debug.LogWarning($"[Buff] {charName}: duration = 0 — บัฟจะไม่มีผล กรุณาตั้งค่า Duration ใน MagicData ให้มากกว่า 0");
            return;
        }

        if (activeBuffCoroutine != null) StopCoroutine(activeBuffCoroutine);
        activeBuffCoroutine = StartCoroutine(BuffDurationCoroutine(amount, duration));
    }

    private IEnumerator BuffDurationCoroutine(int amount, float duration)
    {
        defensePower += amount;
        Debug.Log($"[BUFF] {charName} พลังป้องกันเพิ่มขึ้น {amount} หน่วย เป็นเวลา {duration} วินาที");

        yield return new WaitForSeconds(duration);

        if (state != CharState.Die && curHp > 0)
        {
            defensePower -= amount;
            Debug.Log($"[BUFF END] บัฟของ {charName} หมดเวลาลงแล้ว");
        }
        activeBuffCoroutine = null;
    }

    public void ApplyDebuff(int amount, float duration)
    {
        // [BUG FIX #5] Guard เดียวกัน ป้องกัน duration = 0 บน Debuff ด้วย
        if (duration <= 0f)
        {
            Debug.LogWarning($"[Debuff] {charName}: duration = 0 — ดีบัฟจะไม่มีผล กรุณาตั้งค่า Duration ใน MagicData ให้มากกว่า 0");
            return;
        }

        if (activeDebuffCoroutine != null) StopCoroutine(activeDebuffCoroutine);
        activeDebuffCoroutine = StartCoroutine(DebuffDurationCoroutine(amount, duration));
    }

    private IEnumerator DebuffDurationCoroutine(int amount, float duration)
    {
        attackPower -= amount;
        Debug.Log($"[DEBUFF] {charName} พลังโจมตีลดลง {amount} หน่วย เป็นเวลา {duration} วินาที");

        yield return new WaitForSeconds(duration);

        if (state != CharState.Die && curHp > 0)
        {
            attackPower += amount;
            Debug.Log($"[DEBUFF END] ดีบัฟของ {charName} หมดเวลาลงแล้ว");
        }
        activeDebuffCoroutine = null;
    }
    #endregion

    #region === MAGIC LOGIC ===
    /// <summary>
    /// [BUG FIX #1] รับ lockedTarget เป็น parameter เพื่อป้องกันการใช้ curCharTarget
    /// ที่อาจเปลี่ยนค่าไปแล้วระหว่างที่กระสุนยังบินอยู่ในอากาศ
    /// </summary>
    protected void MagicCastLogic(Magic magic, Character lockedTarget)
    {
        switch (magic.Type)
        {
            case MagicType.Projectile:
            case MagicType.SpawnOnEnemy:
                if (lockedTarget != null && lockedTarget.CurHp > 0)
                {
                    lockedTarget.ReceiveDamage(magic.Power);
                }
                break;

            case MagicType.Buff:
                Recover(magic.Power);
                // [BUG FIX #8] integer division: Power=1 → 1/2=0 ทำให้บัฟไม่มีผล
                // ใช้ Mathf.Max เพื่อการันตีค่าขั้นต่ำ 1 เสมอ
                ApplyBuff(Mathf.Max(1, magic.Power / 2), magic.Duration);
                break;

            case MagicType.Debuff:
                if (lockedTarget != null && lockedTarget.CurHp > 0)
                {
                    // [BUG FIX #8] เดียวกัน — Debuff ก็ต้องการค่าขั้นต่ำ 1
                    lockedTarget.ApplyDebuff(Mathf.Max(1, magic.Power / 2), magic.Duration);
                }
                break;
        }
    }

    private IEnumerator ShootMagicCast(Magic curMagicCast)
    {
        if (curMagicCast.Type != MagicType.Buff && curCharTarget == null)
        {
            SetState(CharState.Idle);
            yield break;
        }

        if (curCharTarget != null && curCharTarget.CurHp <= 0 && curMagicCast.Type != MagicType.Buff)
        {
            SetState(CharState.Idle);
            yield break;
        }

        // [BUG FIX #1] Lock เป้าหมาย ณ เวลายิง ก่อนที่ state จะถูก reset
        // ป้องกันดาเมจเข้าผิดตัวเมื่อ curCharTarget เปลี่ยนระหว่างกระสุนบิน
        Character lockedTarget = curCharTarget;

        Vector3 spawnPosition = transform.position + Vector3.up;

        Vector3 targetPosition = (curMagicCast.Type == MagicType.Buff)
            ? spawnPosition
            : (lockedTarget != null ? GetTargetCenter(lockedTarget) : spawnPosition);

        if (MyActions.onShootMagic != null)
        {
            MyActions.onShootMagic(
                curMagicCast.ShootId,
                spawnPosition,
                targetPosition,
                curMagicCast.ShootTime
            );
        }

        Debug.DrawLine(spawnPosition, targetPosition, Color.red, 2f);

        isMagicMode = false;
        SetState(CharState.Idle);

        if (uiManager != null)
        {
            uiManager.IsOnCurToggleMagic(false);
        }

        PlayCharacterSFX(magicCastSfxIndex);

        // [BUG FIX #9] ปรับ timing ดาเมจให้ตรงกับ VFX ทุกประเภท
        // - Projectile:    รอ ShootTime = เวลาที่กระสุนบิน → ดาเมจตอนกระสุนถึงตัว ✓
        // - SpawnOnEnemy:  รอ ShootTime = เวลา animation effect (เช่น ท้องฟ้าผ่า, อุกกาบาตตก) → ดาเมจตอน VFX ระเบิด ✓
        // - Buff / Debuff: ไม่รอ → ผลสถานะเกิดทันทีพร้อม VFX ✓
        switch (curMagicCast.Type)
        {
            case MagicType.Projectile:
            case MagicType.SpawnOnEnemy:
                yield return new WaitForSeconds(curMagicCast.ShootTime);
                break;
            default:
                yield return null;
                break;
        }

        // [BUG FIX #1] ส่ง lockedTarget ที่ lock ไว้ตั้งแต่ตอนยิง ไม่ใช่ curCharTarget ปัจจุบัน
        MagicCastLogic(curMagicCast, lockedTarget);
    }

    private IEnumerator LoadMagicCast(Magic curMagicCast)
    {
        if (MyActions.onLoadMagic != null)
        {
            MyActions.onLoadMagic(
                CurMagicCast.LoadID,
                transform.position + new Vector3(0, 1f, 0),
                curMagicCast.LoadTime
            );
        }

        yield return new WaitForSeconds(curMagicCast.LoadTime);

        StartCoroutine(ShootMagicCast(curMagicCast));
    }

    private void MagicCast(Magic curMagicCast)
    {
        if (curCharTarget != null)
        {
            transform.LookAt(curCharTarget.transform);
        }

        anim.SetTrigger("MagicAttack");
        StartCoroutine(LoadMagicCast(curMagicCast));
    }

    protected void WalkToMagicCastUpadate()
    {
        if (curMagicCast == null)
        {
            SetState(CharState.Idle);
            return;
        }

        if (curMagicCast.Type == MagicType.Buff)
        {
            navAgent.isStopped = true;
            SetState(CharState.MagicCast);
            MagicCast(curMagicCast);
            return;
        }

        // [BUG FIX #2] ถ้าไม่ใช่ Buff และไม่มีเป้าหมาย ให้ fallback กลับโจมตีปกติ
        // แทนที่จะ SetState(Idle) ซึ่งทำให้ตัวละครหยุดทำอะไรเลย
        if (curCharTarget == null)
        {
            SetState(CharState.Idle);
            return;
        }

        if (curCharTarget.CurHp <= 0)
        {
            SetState(CharState.Idle);
            curCharTarget = null;
            return;
        }

        navAgent.SetDestination(curCharTarget.transform.position);

        float distance = Vector3.Distance(transform.position, curCharTarget.transform.position);

        if (distance <= curMagicCast.Range)
        {
            navAgent.isStopped = true;
            SetState(CharState.MagicCast);
            MagicCast(curMagicCast);
        }
    }

    private Vector3 GetTargetCenter(Character target)
    {
        if (target.TryGetComponent<Collider>(out var col))
        {
            return col.bounds.center + Vector3.up * (col.bounds.extents.y * 0.5f);
        }

        return target.transform.position + Vector3.up;
    }
    #endregion

    #region === EQUIPMENT LOGIC ===
    public void EquipWeapon(Item item)
    {
        UnEquipWeapon();

        if (item == null || item.ItemPrefab == null)
        {
            Debug.LogWarning($"[Character] {charName}: ไม่สามารถสวมใส่ Weapon ได้ เนื่องจาก Item หรือ ItemPrefab ของ '{item?.ItemName ?? "Unknown"}' เป็น Null!");
            return;
        }

        weaponObj = Instantiate(item.ItemPrefab, weaponHand);

        weaponObj.transform.localPosition = new Vector3(6f, 2f, 0f);
        weaponObj.transform.Rotate(0f, 90f, 270f, Space.Self);

        DisableEquipmentPhysics(weaponObj);

        attackPower += item.Power;
        mainWeapon = item;
    }

    public void UnEquipWeapon()
    {
        if (mainWeapon != null)
        {
            attackPower -= mainWeapon.Power;
            mainWeapon = null;
            if (weaponObj != null) Destroy(weaponObj);
        }
    }

    public void EquipShield(Item item)
    {
        UnEquipShield();

        if (item == null || item.ItemPrefab == null)
        {
            Debug.LogWarning($"[Character] {charName}: ไม่สามารถสวมใส่ Shield ได้ เนื่องจาก Item หรือ ItemPrefab ของ '{item?.ItemName ?? "Unknown"}' เป็น Null!");
            return;
        }

        shieldObj = Instantiate(item.ItemPrefab, shieldHand);

        shieldObj.transform.localPosition = new Vector3(-8.5f, -4f, -3f);
        shieldObj.transform.Rotate(-90f, 0f, 180f, Space.Self);

        DisableEquipmentPhysics(shieldObj);

        defensePower += item.Power;
        shield = item;
    }

    public void UnEquipShield()
    {
        if (shield != null)
        {
            defensePower -= shield.Power;
            shield = null;
            if (shieldObj != null) Destroy(shieldObj);
        }
    }

    private void DisableEquipmentPhysics(GameObject obj)
    {
        if (obj == null) return;

        if (obj.TryGetComponent<ItemPick>(out var pick))
        {
            pick.enabled = false;
        }

        if (obj.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        foreach (Collider col in obj.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }
    }
    #endregion

    #region === NPC INTERACTION ===
    public void ToTalkToNPC(Character npc)
    {
        if (curHp <= 0 || state == CharState.Die) return;
        if (state == CharState.MagicCast) return;

        curCharTarget = npc;

        navAgent.SetDestination(npc.transform.position);
        navAgent.isStopped = false;

        SetState(CharState.WalkToNPC);
    }
    #endregion
}