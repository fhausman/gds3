using System.Collections;
using System.Linq;
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
    FailedToFlip,
    Flipping
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
    }

    public override void onExit()
    {
        player.Controls.Player.GravitySwitch.performed -= GravitySwitch;
        player.Controls.Player.Dash.performed -= Dash;
        player.Controls.Player.AttackHigh.performed -= player.AttackHigh;
        player.Controls.Player.AttackLow.performed -= player.AttackLow;

        speedModifier = 1.0f;
    }

    public override void onUpdate(float deltaTime)
    {
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
        if (player.IsTouchingGround)
        {
            player.StateMachine.ChangeState(PlayerState.Flipping);
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

public class PlayerFlipping : BaseState
{
    private Player player = null;

    private Quaternion _upRotation = Quaternion.Euler(Vector3.zero);
    private Quaternion _downRotation = Quaternion.Euler(-180.0f, 0.0f, 0.0f);

    private Quaternion? _start = null;
    private Quaternion? _target = null;

    private Vector3 _dir;

    public PlayerFlipping(Player p)
    {
        player = p;
    }

    public override void onInit(params object[] args)
    {
        player.Controls.Player.GravitySwitch.performed += CancelSwitch;
        _dir = player.Parent.transform.up;

        if (_dir.y > 0.0f)
        {
            _start = _upRotation;
            _target = _downRotation;
        }
        else
        {
            _start = _downRotation;
            _target = _upRotation;
        }

        player.Parent.rotation = _target.Value;
        player.Parent.position += 2 * _dir;

        player.Animator.Play("Falling Idle");
    }

    public override void onExit()
    {
        player.Controls.Player.GravitySwitch.performed -= CancelSwitch;
        player.Animator.Play("Falling To Landing");
    }

    public override void onUpdate(float deltaTime)
    {
        RaycastHit hit;
        if(player.HitsGround(out hit))
        {
            player.StateMachine.ChangeState(PlayerState.Moving);
        }
    }

    public override void onFixedUpdate(float deltaTime)
    {
        var input = player.Controls.Player.HorizontalMovement.ReadValue<float>();
        player.Parent.position += _dir * player.GravitySpeed * deltaTime;

        var horizontalMove = player.transform.right * input * player.SwitchSpeed * deltaTime;
        player.Move(horizontalMove, deltaTime);
    }

    private void CancelSwitch(InputAction.CallbackContext ctx)
    {
        player.Controls.Player.GravitySwitch.performed -= CancelSwitch;
        player.Parent.rotation = _start.Value;
        _dir = -_dir;
        player.Parent.position += 2 * _dir;
    }
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

        dashTimeElapsed = 0.0f;
    }

    public override void onExit()
    {
        player.Controls.Player.AttackHigh.performed -= player.AttackHigh;
        player.Controls.Player.AttackLow.performed -= player.AttackLow;

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

        player.ProjectileShield.enabled = true;

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
    }

    public override void onExit()
    {
        player.Controls.Player.GravitySwitch.performed -= GravitySwitch;
        player.Controls.Player.Dash.performed -= Dash;

        player.Weapon.DisableCollision();
        player.Weapon.SetIdle();

        player.ProjectileShield.enabled = false;
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
    }

    public override void onExit()
    {
    }
}
#endregion

public class Player : MonoBehaviour
{
    #region Settings
    public PlayerSettings settings;
    public int Health { get => settings.health; }
    public float Speed { get => settings.speed; }
    public float SwitchSpeed { get => settings.switchSpeed; }
    public float GravitySpeed { get => settings.gravitySpeed; }
    public float DashSpeed { get => settings.dashSpeed; }
    public float DashTime { get => settings.dashTime; }
    public float DashCooldown { get => settings.dashCooldown; }
    public float AttackDuration { get => settings.attackDuration; }
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

    [SerializeField]
    private Collider _projectileShield = null;
    public Collider ProjectileShield { get => _projectileShield; }

    [SerializeField]
    private Animator _animator = null;
    public Animator Animator { get => _animator; }

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
    private Vector3 GravityVelocity { get; set; } = Vector3.zero;
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
            if (StateMachine.CurrentState != PlayerState.Flipping)
            {
                //TODO: change state to falling
                GravityVelocity += -_parent.up * GravitySpeed * deltaTime;
                _parent.position += GravityVelocity * deltaTime;
            }
        }

        if (Mathf.Abs(dir.x) > 0.0f + Mathf.Epsilon)
        {
            Animator.SetTrigger("Running");

            if (!Physics.Raycast(transform.position, dir, out hit, 0.22f, 1 << 8))
            {
                _parent.position += dir;
            }

            _parent.transform.localScale = new Vector3(
                Mathf.Sign(FacingDirection) * Mathf.Abs(_parent.transform.localScale.x), _parent.transform.localScale.y, _parent.transform.localScale.z
            );
        }
        else
        {
            Animator.SetTrigger("Idle");
        }
    }

    public bool HitsGround(out RaycastHit hit)
    {
        return Physics.Raycast(transform.position, -transform.up, out hit, 1.0f, 1 << 8);
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

        StateMachine.AddState(PlayerState.Moving, new PlayerMoving(this));
        StateMachine.AddState(PlayerState.Dashing, new PlayerDashing(this));
        StateMachine.AddState(PlayerState.Attacking, new PlayerAttacking(this));
        StateMachine.AddState(PlayerState.ReceivedDamage, new PlayerReceivedDamage(this));
        StateMachine.AddState(PlayerState.FailedToFlip, new PlayerFailedToFlip(this));
        StateMachine.AddState(PlayerState.Flipping, new PlayerFlipping(this));
        StateMachine.ChangeState(PlayerState.Moving);

        _currentHealth = Health;

        Controls.Enable();
    }

    void Update()
    {
        CooldownUpdate();
        StateMachine.OnUpdate(Time.deltaTime);

        if (Controls.Player.Interact.triggered)
        {
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
            StateMachine.ChangeState(PlayerState.ReceivedDamage,
                collision.gameObject.GetComponent<Projectile>().Dir.x);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("EnemyWeapon") && StateMachine.CurrentState != PlayerState.Dashing && StateMachine.CurrentState != PlayerState.ReceivedDamage)
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
#endregion
}