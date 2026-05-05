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

    public int CurHp { get { return curHp; } set { curHp = value; } }
    public int MaxHP { get { return maxHP; } }
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
    [SerializeField] protected List<Magic> magicSkills = new List<Magic>();
    [SerializeField] protected Magic curMagicCast = null;
    [SerializeField] protected bool isMagicMode = false;

    public List<Magic> MagicSkills { get { return magicSkills; } set { magicSkills = value; } }
    public Magic CurMagicCast { get { return curMagicCast; } set { curMagicCast = value; } }
    public bool IsMagicMode { get { return isMagicMode; } set { isMagicMode = value; } }
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

    #region === MANAGERS ===
    protected VFXManager vfxManager;
    protected UIManager uiManager;
    protected InventoryManager invManager;
    protected PartyManager partyManager;
    #endregion

    #region === UNITY CALLBACKS ===
    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        _collider = GetComponent<Collider>();
    }
    #endregion

    #region === INIT & STATE ===
    public void CharInit(VFXManager vfxM, UIManager uiM, InventoryManager invM, PartyManager partyM)
    {
        vfxManager = vfxM;
        uiManager = uiM;
        invManager = invM;
        partyManager = partyM;

        inventoryItems = new Item[InventoryManager.MAXSLOT];
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
            SetState(CharState.Attack);
            Attack();
        }
    }

    protected void Attack()
    {
        transform.LookAt(curCharTarget.transform);
        anim.SetTrigger("Attack");

        AttackLogic();
    }

    protected void AttackUpdate()
    {
        if (curCharTarget == null) return;

        if (curCharTarget.CurHp <= 0)
        {
            SetState(CharState.Idle);
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
        Character target = curCharTarget.GetComponent<Character>();

        int totalAtkDmg = attackDamage + attackPower;

        if (target != null)
        {
            target.ReceiveDamage(totalAtkDmg);
        }
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

        invManager.SpawnDropInventory(inventoryItems, transform.position);

        StartCoroutine(DestroyObject());
    }

    public void ReceiveDamage(int dmg)
    {
        if (curHp <= 0 || state == CharState.Die) return;

        int damageAfter = dmg - defensePower;

        if (damageAfter < 0)
        {
            damageAfter = 0;
        }

        curHp -= dmg;

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

    #region === MAGIC LOGIC ===
    protected void MagicCastLogic(Magic magic)
    {
        Character target = curCharTarget.GetComponent<Character>();

        if (target != null)
        {
            target.ReceiveDamage(magic.Power);
        }
    }

    private IEnumerator ShootMagicCast(Magic curMagicCast)
    {
        if (curCharTarget == null || vfxManager == null)
            yield break;

        if (curCharTarget.CurHp <= 0)
        {
            SetState(CharState.Idle);
            yield break;
        }

        Vector3 spawnPosition = transform.position + Vector3.up;
        Vector3 targetPosition = GetTargetCenter(curCharTarget);

        vfxManager.ShootMagic(
            curMagicCast.ShootId,
            spawnPosition,
            targetPosition,
            curMagicCast.ShootTime
        );

        Debug.DrawLine(spawnPosition, targetPosition, Color.red, 2f);

        isMagicMode = false;
        SetState(CharState.Idle);

        if (uiManager != null)
        {
            uiManager.IsOnCurToggleMagic(false);
        }

        yield return new WaitForSeconds(curMagicCast.ShootTime);

        MagicCastLogic(curMagicCast);
    }

    private IEnumerator LoadMagicCast(Magic curMagicCast)
    {
        if (vfxManager != null)
        {
            vfxManager.LoadMagic(
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
        transform.LookAt(curCharTarget.transform);
        anim.SetTrigger("MagicAttack");

        StartCoroutine(LoadMagicCast(curMagicCast));
    }

    protected void WalkToMagicCastUpadate()
    {
        if (curCharTarget == null || curMagicCast == null)
        {
            SetState(CharState.Idle);
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
        weaponObj = Instantiate(invManager.ItemPrefabs[item.PrefabID], weaponHand);

        weaponObj.transform.localPosition = new Vector3(6f, 2f, 0f);
        weaponObj.transform.Rotate(0f, 90f, 270f, Space.Self);

        attackPower += item.Power;
        mainWeapon = item;
    }

    public void UnEquipWeapon()
    {
        if (mainWeapon != null)
        {
            defensePower -= mainWeapon.Power;
            mainWeapon = null;
            Destroy(weaponObj);
        }
    }

    public void EquipShield(Item item)
    {
        shieldObj = Instantiate(invManager.ItemPrefabs[item.PrefabID], shieldHand);

        shieldObj.transform.localPosition = new Vector3(-8.5f, -4f, -3f);
        shieldObj.transform.Rotate(-90f, 0f, 180f, Space.Self);

        defensePower += item.Power;
        shield = item;
    }

    public void UnEquipShield()
    {
        if (shield != null)
        {
            defensePower -= shield.Power;
            shield = null;
            Destroy(shieldObj);
        }
    }
    #endregion

    #region === NPC INTERACTION ===
    public void ToTalkToNPC(Character npc)
    {
        if (curHp <= 0 || state == CharState.Die) return;

        curCharTarget = npc;

        navAgent.SetDestination(npc.transform.position);
        navAgent.isStopped = false;

        SetState(CharState.WalkToNPC);
    }
    #endregion
}