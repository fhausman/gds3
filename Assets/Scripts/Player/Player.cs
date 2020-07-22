using System.Collections;
using System.Linq;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

#region Player States
public enum PlayerState
{
    Moving,
    Dashing,
    Attacking,
    ReceivedDamage,
    FailedToFlip
}

public enum AttackType
{
    High,
    Low
}

public class PlayerMoving : BaseState
{
    private Player player = null;
    private float speedModifier = 1.0f;

    public PlayerMoving(Player p)
    {
        player = p;
    }

    public override void onInit(params object[] args)
    {
        player.Controls.Player.GravitySwitch.performed += GravitySwitch;
        player.Controls.Player.Dash.performed += Dash;
        player.Controls.Player.AttackHigh.performed += player.AttackHigh;
        player.Controls.Player.AttackLow.performed += player.AttackLow;
        //player.Controls.Player.Block.performed += Block;
        //player.Controls.Player.Block.canceled += LeavingBlock;

        player.Weapon.SetIdle();
    }

    public override void onExit()
    {
        player.Controls.Player.GravitySwitch.performed -= GravitySwitch;
        player.Controls.Player.Dash.performed -= Dash;
        player.Controls.Player.AttackHigh.performed -= player.AttackHigh;
        player.Controls.Player.AttackLow.performed -= player.AttackLow;
        //player.Controls.Player.Block.performed -= Block;
        //player.Controls.Player.Block.canceled -= LeavingBlock;

        //player.Weapon.SetIdle();
        //player.Blocking = false;
        speedModifier = 1.0f;
    }

    public override void onUpdate(float deltaTime)
    {
        //if (player.Blocking)
        //{
        //    speedModifier = player.BlockSpeedModifier;
        //    if (player.Aim >= 0.0f)
        //    {
        //        player.Weapon.SetUpper();
        //    }
        //    else
        //    {
        //        player.Weapon.SetBottom();
        //    }
        //}
        //else
        //{
        //    player.Weapon.SetIdle();
        //    speedModifier = 1.0f;
        //}
    }

    public override void onFixedUpdate(float deltaTime)
    {
        var horizontalInput = Vector3.Dot(player.transform.right, Vector3.right) * player.Controls.Player.HorizontalMovement.ReadValue<float>();
        var verticalInput = Vector3.Dot(player.transform.right, Vector3.up) * player.Controls.Player.VerticalMovement.ReadValue<float>();
        var inputDirectionModifier = Mathf.Clamp(horizontalInput + verticalInput, -1.0f, 1.0f);

        if(Mathf.Abs(inputDirectionModifier) > 0.0f + Mathf.Epsilon)
        {
            player.FacingDirection = Mathf.Sign(inputDirectionModifier);
        }

        var horizontalMove = player.transform.right * inputDirectionModifier * player.Speed * speedModifier * deltaTime;
        player.Move(horizontalMove, deltaTime);
    }

    private void GravitySwitch(InputAction.CallbackContext ctx)
    {
        var isTouchingGround = player.IsTouchingGround;
        if (isTouchingGround && player.CanSwitch)
        {
            player.Flip();
        }
        else if (isTouchingGround)
        {
            player.StateMachine.ChangeState(PlayerState.FailedToFlip);
        }
    }

    public void Dash(InputAction.CallbackContext ctx)
    {
        if (player.CanDash)
        {
            player.StateMachine.ChangeState(PlayerState.Dashing);
            player.DashCooldownElapsed = 0.0f;
        }
    }

    //private void Block(InputAction.CallbackContext ctx)
    //{
    //    player.Blocking = true;

    //    var zoneSize = player.Aim >= 0.0f ? player.UpperBlockZoneSize : player.BottomBlockZoneSize;
    //    var projectiles =
    //        Physics.OverlapBox(
    //            player.transform.position + new Vector3(Mathf.Sign(player.transform.right.x) * player.FacingDirection * (zoneSize.x + (player.SweetSpotWidth) / 2),
    //                                                    Mathf.Sign(player.Aim) * zoneSize.y / 2),
    //            new Vector3(player.SweetSpotWidth / 2, zoneSize.y / 2, 0.5f),
    //            player.transform.rotation,
    //            LayerMask.GetMask("Projectiles")
    //        );

    //    foreach (var proj in projectiles)
    //    {
    //        proj.SendMessage("Reflect");
    //    }
    //}

    //private void LeavingBlock(InputAction.CallbackContext ctx)
    //{
    //    if (player.Blocking)
    //    {
    //        player.Blocking = false;
    //    }
    //}
}

public class PlayerDashing : BaseState
{
    private Player player = null;
    private float dashTimeElapsed = 0.0f;

    public PlayerDashing(Player p)
    {
        player = p;
    }

    public override void onInit(params object[] args)
    {
        player.Controls.Player.AttackHigh.performed += player.AttackHigh;
        player.Controls.Player.AttackLow.performed += player.AttackLow;
        //player.Controls.Player.Block.performed += Block;

        dashTimeElapsed = 0.0f;
    }

    public override void onExit()
    {
        player.Controls.Player.AttackHigh.performed -= player.AttackHigh;
        player.Controls.Player.AttackLow.performed -= player.AttackLow;
        //player.Controls.Player.Block.performed -= Block;

        player.DashCooldownElapsed = 0.0f;
    }

    public override void onFixedUpdate(float deltaTime)
    {
        if(dashTimeElapsed > player.DashTime)
        {
            player.StateMachine.ChangeState(PlayerState.Moving);
        }

        var horizontalMove =
            player.FacingDirection * player.transform.right * (player.DashSpeed - (dashTimeElapsed / player.DashTime) * player.DashSpeed) * deltaTime;
        player.Move(horizontalMove, deltaTime);

        dashTimeElapsed += deltaTime;
    }

    //private void Block(InputAction.CallbackContext ctx)
    //{
    //    player.Blocking = true;
    //    player.StateMachine.ChangeState(PlayerState.Moving);
    //}
}

public class PlayerAttacking : BaseState
{
    private Player player = null;
    private Animator animator = null;
    private float attackTimeElapsed = 0.0f;
    private float attackDuration = 0.0f;

    public PlayerAttacking(Player p)
    {
        player = p;
        animator = player.Weapon.GetComponent<Animator>();
        attackDuration = animator.runtimeAnimatorController.animationClips.First(a => a.name == "Attack").length;
    }

    public override void onInit(params object[] args)
    {
        player.OnAttackStartNotify.Invoke();

        player.Controls.Player.GravitySwitch.performed += GravitySwitch;
        player.Controls.Player.Dash.performed += Dash;

        attackTimeElapsed = 0.0f;

        var attackType = (AttackType) args[0];
        if(attackType == AttackType.High)
        {
            player.Weapon.GetComponent<Animator>().Play("Attack");
        }
        else if (attackType == AttackType.Low)
        {
            player.Weapon.GetComponent<Animator>().Play("Attack_Low");
        }

        player.Weapon.EnableCollision();

        //Collider[] hitObjects = GetHitObjects(player.Aim >= 0.0f ? player.UpperBlockZoneSize : player.BottomBlockZoneSize);
        //foreach (var obj in hitObjects)
        //{
        //    obj.SendMessage("ReceivedDamage");
        //}

        //if (player.Aim >= 0.0f)
        //{
        //    player.Weapon.SetUpper();
        //}
        //else
        //{
        //    player.Weapon.SetBottom();
        //}
    }

    public override void onExit()
    {
        player.Controls.Player.GravitySwitch.performed -= GravitySwitch;
        player.Controls.Player.Dash.performed -= Dash;

        player.Weapon.DisableCollision();
        player.Weapon.SetIdle();
    }

    public override void onUpdate(float deltaTime)
    {
        if (attackTimeElapsed > attackDuration)
        {
            player.StateMachine.ChangeState(PlayerState.Moving);
        }

        var horizontalMove =
            player.FacingDirection * player.transform.right * (player.Speed + 1.0f - (attackTimeElapsed / player.AttackDuration) * player.Speed) * deltaTime;
        player.Move(horizontalMove, deltaTime);

        attackTimeElapsed += deltaTime;
    }

    //private Collider[] GetHitObjects(Vector2 zoneSize)
    //{
    //    return Physics.OverlapBox(
    //                player.transform.position + new Vector3(player.FacingDirection * zoneSize.x / 2, Mathf.Sign(player.Aim) * zoneSize.y / 2),
    //                new Vector3(zoneSize.x / 2, zoneSize.y / 2, 0.5f),
    //                player.transform.rotation,
    //                LayerMask.GetMask("Enemies")
    //                );
    //}

    private void GravitySwitch(InputAction.CallbackContext ctx)
    {
        if (player.CanSwitch)
        {
            player.Flip();
        }
    }

    public void Dash(InputAction.CallbackContext ctx)
    {
        if (player.CanDash)
        {
            player.StateMachine.ChangeState(PlayerState.Dashing);
            player.DashCooldownElapsed = 0.0f;
        }
    }
}

public class PlayerReceivedDamage : BaseState
{
    private Player player = null;
    private float damageCooldownDuration = 0.1f;
    private float damageCooldownElapsed = 0.0f;
    private float impactDirection = 1.0f;

    public PlayerReceivedDamage(Player p)
    {
        player = p;
    }

    public override void onInit(params object[] args)
    {
        damageCooldownElapsed = 0.0f;
        impactDirection = (float) args[0];

        player.DecreaseHealth(1);
    }

    public override void onUpdate(float deltaTime)
    {
        if (damageCooldownElapsed > damageCooldownDuration)
        {
            player.StateMachine.ChangeState(PlayerState.Moving);
        }

        var horizontalMove =
            impactDirection * Vector3.right * (player.Speed + 1.0f - (damageCooldownElapsed / damageCooldownDuration) * player.Speed) * deltaTime;
        player.Move(horizontalMove, deltaTime);

        damageCooldownElapsed += deltaTime;
    }
}

public class PlayerFailedToFlip : BaseState
{
    private Player player = null;

    public PlayerFailedToFlip(Player p)
    {
        player = p;
    }

    public override void onInit(params object[] args)
    {
        player.Animator.Play("SwitchFailed");
    }

    public override void onExit()
    {
        player.Animator.Rebind();
    }
}
#endregion

public class Player : MonoBehaviour
{
    #region Settings
    public PlayerSettings settings;
    public int Health { get => settings.health; }
    public float Speed { get => settings.speed; }
    public float GravitySpeed { get => settings.gravitySpeed; }
    //public float BlockSpeedModifier { get => settings.blockSpeedModifier; }
    public float DashSpeed { get => settings.dashSpeed; }
    public float DashTime { get => settings.dashTime; }
    public float DashCooldown { get => settings.dashCooldown; }
    public float AttackDuration { get => settings.attackDuration; }
    //public Vector2 UpperBlockZoneSize { get => settings.upperBlockZoneSize; }
    //public Vector2 BottomBlockZoneSize { get => settings.bottomBlockZoneSize; }
    //public float SweetSpotWidth { get => settings.sweetSpotWidth; }
    public float GravitySwitchHeight { get => settings.gravitySwitchHeight; }
    public float GravitySwitchCooldown { get => settings.gravitySwitchCooldown; }
    #endregion

    #region Outside References
    [SerializeField]
    private Transform _parent = null;
    public Transform Parent { get => _parent; }

    [SerializeField]
    private Weapon _weapon = null;
    public Weapon Weapon { get => _weapon; }

    #endregion

    #region Private Fields 
    private Vector2 _aim = Vector2.zero;
    private Interactable _heldObject = null;
    private int _currentHealth = 0;
    private int _attacksCounter = 0;
    #endregion

    #region Properties
    public MainControls Controls { get; private set; } = null;
    public StateMachine<PlayerState> StateMachine { get; private set; } = new StateMachine<PlayerState>();
    public Animator Animator { get; private set; } = null;
    private Vector3 GravityVelocity { get; set; } = Vector3.zero;
    //public bool Blocking { get; set; } = false;
    public bool WeaponEquipped { get; set; } = true;
    public bool CanDash { get => DashCooldownElapsed > DashCooldown; }
    public float FacingDirection { get; set; } = 1.0f;
    public float DashCooldownElapsed { get; set; } = 100.0f;
    public float GravitySwitchCooldownElapsed { get; set; } = 100.0f;
    public bool IsHoldingObject { get => _heldObject != null; }
    public int CurrentHealth { get => _currentHealth; }
    public UnityEvent OnAttackStartNotify { get; set; } = new UnityEvent();

    public bool IsTouchingGround
    {
        get
        {
            RaycastHit hit;
            return Physics.Raycast(transform.position, -transform.up, out hit, 1.0f, 1 << 8);
        }
    }
    public bool CanSwitch
    {
        get
        {
            RaycastHit hit;
            return Physics.Raycast(Parent.position, Parent.up, out hit, GravitySwitchHeight, 1 << 8) && (GravitySwitchCooldownElapsed > GravitySwitchCooldown);
        }
    }

    public bool IsAttacking
    {
        get => StateMachine.CurrentState == PlayerState.Attacking;
    }

    //public float Aim { get => _aim.y * transform.up.y; }

    //public Bounds UpperBlockAreaBounds
    //{
    //    get =>
    //        new Bounds(
    //            transform.position + new Vector3(
    //                Mathf.Sign(transform.right.x) * FacingDirection * UpperBlockZoneSize.x / 2,
    //                Mathf.Sign(Aim) * UpperBlockZoneSize.y / 2
    //                ),
    //            new Vector3(UpperBlockZoneSize.x, UpperBlockZoneSize.y, 0.5f));
    //}

    //public Bounds BottomBlockAreaBounds
    //{
    //    get =>
    //          new Bounds(
    //              transform.position + new Vector3(
    //                  Mathf.Sign(transform.right.x) * FacingDirection * BottomBlockZoneSize.x / 2,
    //                  Mathf.Sign(Aim) * BottomBlockZoneSize.y / 2
    //                  ),
    //              new Vector3(BottomBlockZoneSize.x, BottomBlockZoneSize.y, 0.5f));
    //}
    #endregion

    public void Move(Vector3 dir, float deltaTime)
    {
        RaycastHit hit;
        //vertical movement
        if (Physics.Raycast(transform.position, -transform.up, out hit, 1.0f, 1 << 8))
        {
            if (hit.collider.CompareTag("Elevator"))
            {
                _parent.transform.parent = hit.collider.transform;
            }
            else
            {
                _parent.transform.parent = null;
            }

            //Debug.DrawRay(hit.point, hit.normal, Color.red);
            _parent.position = hit.point;
            _parent.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            GravityVelocity = Vector3.zero;
        }
        else
        {
            //TODO: change state to falling
            GravityVelocity += -_parent.up * GravitySpeed * deltaTime;
            _parent.position += GravityVelocity * deltaTime;
        }

        if (Mathf.Abs(dir.x) > 0.0f + Mathf.Epsilon)
        {
            if (!Physics.Raycast(transform.position, dir, out hit, 0.22f, 1 << 8))
            {
                _parent.position += dir;
            }

            _parent.transform.localScale = new Vector3(
                Mathf.Sign(FacingDirection) * Mathf.Abs(_parent.transform.localScale.x), _parent.transform.localScale.y, _parent.transform.localScale.z
            );
        }
    }

    public void Flip()
    {
        GravitySwitchCooldownElapsed = 0.0f;

        _parent.Rotate(0, 0, 180);
        _parent.position -= _parent.up;
        _parent.transform.localScale = new Vector3(
            -_parent.transform.localScale.x, _parent.transform.localScale.y, _parent.transform.localScale.z
        ); ;
    }

    public void DecreaseHealth(int healthAmount)
    {
        _currentHealth -= healthAmount;
        if (_currentHealth <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void AttackHigh(InputAction.CallbackContext ctx)
    {
        if (WeaponEquipped && _attacksCounter < 3)
        {
            _attacksCounter++;
            StopCoroutine("ResetAttacksCounter");
            StartCoroutine("ResetAttacksCounter");
            StateMachine.ChangeState(PlayerState.Attacking, AttackType.High);
        }
    }

    public void AttackLow(InputAction.CallbackContext ctx)
    {
        if (WeaponEquipped)
        {
            StateMachine.ChangeState(PlayerState.Attacking, AttackType.Low);
        }
    }

    private IEnumerator ResetAttacksCounter()
    {
        yield return new WaitForSeconds(0.5f);
        _attacksCounter = 0;
    }

    #region Mono behaviour methods
    void Start()
    {
        Controls = new MainControls();
        Animator = GetComponent<Animator>();

        StateMachine.AddState(PlayerState.Moving, new PlayerMoving(this));
        StateMachine.AddState(PlayerState.Dashing, new PlayerDashing(this));
        StateMachine.AddState(PlayerState.Attacking, new PlayerAttacking(this));
        StateMachine.AddState(PlayerState.ReceivedDamage, new PlayerReceivedDamage(this));
        StateMachine.AddState(PlayerState.FailedToFlip, new PlayerFailedToFlip(this));
        StateMachine.ChangeState(PlayerState.Moving);

        _currentHealth = Health;

        Controls.Enable();
    }

    void Update()
    {
        //_aim = Controls.Player.Aim.ReadValue<Vector2>();

        CooldownUpdate();
        StateMachine.OnUpdate(Time.deltaTime);

        if (Controls.Player.Interact.triggered)
        {
            //todo: this is temporary garbage, requires refactoring
            if (_heldObject == null)
            {
                var objects = Physics.OverlapSphere(transform.position, 0.5f, LayerMask.GetMask("Lens"));
                if (objects.Length > 0)
                {
                    _heldObject = objects[0].gameObject.GetComponent<Interactable>();
                    _heldObject.OnInteractionStart();
                }

                foreach(var obj in Physics.OverlapSphere(transform.position, 0.5f, LayerMask.GetMask("Terminal")))
                {
                    obj.gameObject.SendMessage("Activate");
                }
            }
            else
            {
                _heldObject.OnInteractionEnd();
                _heldObject.transform.position = new Vector3(transform.position.x, transform.position.y, _heldObject.transform.position.z);
                _heldObject = null;
            }
        }
    }

    void FixedUpdate()
    {
        StateMachine.OnFixedUpdate(Time.fixedDeltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.CompareTag("Projectile") && StateMachine.CurrentState != PlayerState.Dashing)
        {
            //if(Blocking)
            //{
            //    if (collision.collider.bounds.Intersects(Aim >= 0.0f ? UpperBlockAreaBounds : BottomBlockAreaBounds))
            //    {
            //        collision.collider.SendMessage("Destroy");
            //        return;
            //    }
            //}

            StateMachine.ChangeState(PlayerState.ReceivedDamage,
                collision.gameObject.GetComponent<Projectile>().Dir.x);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("EnemyWeapon") && StateMachine.CurrentState != PlayerState.Dashing)
        {
            StateMachine.ChangeState(PlayerState.ReceivedDamage,
                other.transform.position.x > transform.position.x ? -1.0f : 1.0f);
        }
    }

    void CooldownUpdate()
    {
        if(DashCooldownElapsed < DashCooldown + Mathf.Epsilon)
        {
            DashCooldownElapsed += Time.deltaTime;
        }

        if(GravitySwitchCooldownElapsed < GravitySwitchCooldown + Mathf.Epsilon)
        {
            GravitySwitchCooldownElapsed += Time.deltaTime;
        }
    }

    void OnLaserHit()
    {
        DecreaseHealth(100);
    }

    void OnSwitchFailEnd()
    {
        StateMachine.ChangeState(PlayerState.Moving);
    }

#if !UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Aim > 0.0f + Mathf.Epsilon ?
            Color.green : Color.red;
        DrawZonesDebug(UpperBlockZoneSize, 1.0f);

        Gizmos.color = Aim < 0.0f - Mathf.Epsilon ?
            Color.green : Color.red;
        DrawZonesDebug(BottomBlockZoneSize, -1.0f);
    }

    private void DrawZonesDebug(Vector2 size, float dir)
    {
        //block box
        var position = new Vector3(size.x / 2, dir * size.y / 2);
        var boxSize = new Vector3(size.x, size.y, 0.5f);
        Gizmos.DrawWireCube(position, boxSize);
        //sweetspot
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            position + new Vector3((boxSize.x + SweetSpotWidth) / 2, 0.0f),
            new Vector3(SweetSpotWidth, boxSize.y, boxSize.z)
            );
    }
#endif
#endregion
}