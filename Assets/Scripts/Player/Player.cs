using System.Collections;
using System.Linq;
using TMPro;
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
    private float coyoteTimeElapsed = 0.0f;

    public PlayerMoving(Player p)
    {
        player = p;
    }

    public override void onInit(params object[] args)
    {
        player.Controls.Player.GravitySwitch.performed += GravitySwitch;
        //player.Controls.Player.Dash.performed += Dash;

        coyoteTimeElapsed = 0.0f;
    }

    public override void onExit()
    {
        player.Controls.Player.GravitySwitch.performed -= GravitySwitch;
        //player.Controls.Player.Dash.performed -= Dash;
        //player.Controls.Player.AttackHigh.performed -= player.AttackHigh;
        //player.Controls.Player.AttackLow.performed -= player.AttackLow;

        speedModifier = 1.0f;
    }

    public override void onUpdate(float deltaTime)
    {
        if(player.IsTouchingGround)
        {
            coyoteTimeElapsed = 0.0f;
            speedModifier = 1.0f;

            if (player.Animator.GetCurrentAnimatorStateInfo(0).IsName("Falling Idle"))
            {
                player.Animator.Play("Falling To Landing");
            }
        }
        else
        {
            coyoteTimeElapsed += deltaTime;
            speedModifier = 1.0f; // 0.8f;

            if (!player.Animator.GetCurrentAnimatorStateInfo(0).IsName("Falling Idle"))
            {
                player.Animator.Play("Falling Idle");
            }
        }
    }

    public override void onFixedUpdate(float deltaTime)
    {
        var inputDirectionModifier = player.InputModifier;

        if(Mathf.Abs(inputDirectionModifier) > 0.01f)
        {
            player.FacingDirection = Mathf.Sign(inputDirectionModifier);
        }

        var horizontalMove = player.transform.right * inputDirectionModifier * player.Speed * speedModifier * deltaTime;
        player.Move(horizontalMove, deltaTime);
    }

    private void GravitySwitch(InputAction.CallbackContext ctx)
    {
       // if (player.IsTouchingGround || (!player.IsTouchingGround && coyoteTimeElapsed < player.CoyoteTime))
       // {
            player.StateMachine.ChangeState(PlayerState.Flipping);
            player.GravityVelocity = Vector3.zero;
        // }
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
        player.Parent.transform.parent = null;

        _dir = player.Parent.transform.up;
        _start = player.Parent.rotation;
        _target = _start * Quaternion.Euler(180.0f, 0.0f, 0.0f);

        player.Parent.rotation = _target.Value;
        player.Parent.position += 2 * _dir;

        player.Animator.Play("Falling Idle");
        player.PlayFootstep();
    }

    public override void onExit()
    {
        player.Controls.Player.GravitySwitch.performed -= CancelSwitch;
        player.Animator.Play("Falling To Landing");
        //player.PlayFootstep();
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
        var inputDirectionModifier = player.InputModifier;
        if (Mathf.Abs(inputDirectionModifier) > 0.0f + Mathf.Epsilon)
        {
            player.FacingDirection = Mathf.Sign(inputDirectionModifier);
        }

        var horizontalMove = player.transform.right * inputDirectionModifier * player.SwitchSpeed * deltaTime;
        player.Move(horizontalMove, deltaTime);
    }

    private void CancelSwitch(InputAction.CallbackContext ctx)
    {
        player.Controls.Player.GravitySwitch.performed -= CancelSwitch;
        player.Parent.rotation = _start.Value;
        _dir = -_dir;
        player.Parent.position += 2 * _dir;
        player.GravityVelocity = Vector3.zero;

        //player.PlayLand();
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
        //damageCooldownElapsed = 0.0f;
        //impactDirection = (float) args[0];

        //player.DecreaseHealth(1000);
        player.PlayDeath();
        player.Animator.Play("Being Electrocuted");
        player.StartCoroutine(FadeOut());
        player.Collider.enabled = false;
        damageCooldownElapsed = 0.0f;
    }

    public override void onUpdate(float deltaTime)
    {
        damageCooldownElapsed += deltaTime;
        player.Renderer.material.SetFloat("_Prog", damageCooldownElapsed);
        player.Joints.enabled = false;

        if (player.Lens.activeSelf)
        {
            player.Lens.GetComponent<Rigidbody>().isKinematic = false;
            player.Lens.transform.parent = null;
        }

        if(damageCooldownElapsed > 2.0f)
        {
            player.Respawn();
        }
        //if (damageCooldownElapsed > damageCooldownDuration)
        //{
        //    player.StateMachine.ChangeState(PlayerState.Moving);
        //}

        //var horizontalMove =
        //    impactDirection * Vector3.right * (player.Speed + 1.0f - (damageCooldownElapsed / damageCooldownDuration) * player.Speed) * deltaTime;
        //player.Move(horizontalMove, deltaTime);

        //damageCooldownElapsed += deltaTime;
    }

    private IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(0.5f);
        var fade = GameObject.Find("Fade");
        if (fade)
            fade.BroadcastMessage("FadeOut");
    }

    public override void onExit()
    {
        player.Renderer.material.SetFloat("_Prog", 0.0f);
        player.Joints.enabled = true;
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
    public float CoyoteTime { get => settings.coyoteTime; }
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
    private Collider _collider = null;
    public Collider Collider { get => _collider; }

    [SerializeField]
    private Animator _animator = null;
    public Animator Animator { get => _animator; }

    [SerializeField]
    private Renderer _renderer = null;
    public Renderer Renderer { get => _renderer; }

    [SerializeField]
    private Renderer _joints = null;
    public Renderer Joints { get => _joints; }

    [SerializeField]
    private GameObject _lensOnTheBack = null;
    public GameObject Lens { get => _lensOnTheBack; }

    [SerializeField]
    private AudioSource _audio = null;

    [SerializeField]
    private AudioClip[] _footsteps = null;

    [SerializeField]
    private AudioClip _land = null;

    [SerializeField]
    private AudioClip _death = null;

    [SerializeField]
    private AudioClip[] _takeBox = null;

    #endregion

    #region Private Fields 
    private Vector2 _aim = Vector2.zero;
    private Interactable _heldObject = null;
    private int _currentHealth = 0;
    private int _attacksCounter = 0;
    private GameObject _checkpoint = null;

    private Vector3 _pickLensPosition = Vector3.zero;
    private Vector3 _originalLensPosition = Vector3.zero;
    private Quaternion _originalLensRotation;
    private Transform _originalLensParent = null;
    private bool _canDie = true;
    private float _footstepDelay = 0.0f;
    private bool _wasInAir = false;
    private Rigidbody _rb = null;
    #endregion

    #region Properties
    public MainControls Controls { get; private set; } = null;
    public PlayerInput PlayerInput { get; private set; } = null;
    public StateMachine<PlayerState> StateMachine { get; private set; } = new StateMachine<PlayerState>();
    public Vector3 GravityVelocity { get; set; } = Vector3.zero;
    public bool WeaponEquipped { get; set; } = true;
    public bool CanDash { get => DashCooldownElapsed > DashCooldown; }
    public float FacingDirection { get; set; } = 1.0f;
    public float DashCooldownElapsed { get; set; } = 100.0f;
    public float GravitySwitchCooldownElapsed { get; set; } = 100.0f;
    public bool IsHoldingObject { get => _heldObject != null; }
    public int CurrentHealth { get => _currentHealth; }
    public UnityEvent OnAttackStartNotify { get; set; } = new UnityEvent();
    public string CurrentControls { get; private set; }

    public bool IsTouchingGround
    {
        get
        {
            RaycastHit hit;
            return Physics.Raycast(transform.position, -transform.up, out hit, 1.0f, LayerMask.GetMask("Ground"));
        }
    }

    public float InputModifier
    {
        get
        {
            var horizontalInput = Vector3.Dot(_parent.transform.right, Vector3.right) * Controls.Player.HorizontalMovement.ReadValue<float>();
            var verticalInput = Vector3.Dot(_parent.transform.right, Vector3.up) * Controls.Player.VerticalMovement.ReadValue<float>();
            return Mathf.Clamp(horizontalInput + verticalInput, -1.0f, 1.0f);
        }
    }
    #endregion

    public void Move(Vector3 dir, float deltaTime)
    {
        RaycastHit hit;
        //vertical movement
        if (Physics.Raycast(transform.position, -transform.up, out hit, 1.0f, LayerMask.GetMask("Ground")))
        {
            if (hit.collider.CompareTag("Elevator"))
            {
                _parent.transform.parent = hit.collider.transform;
                _rb.isKinematic = false;
                _rb.constraints &= ~RigidbodyConstraints.FreezePositionY;
            }
            else
            {
                OnLeftElevator();
            }

            //Debug.DrawRay(hit.point, hit.normal, Color.red);
            _parent.position = hit.point;
            var normalQuat = Quaternion.LookRotation(_parent.forward, hit.normal);
            _parent.rotation = Quaternion.RotateTowards(_parent.rotation, normalQuat, 5.0f);
            //_parent.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            GravityVelocity = Vector3.zero;

            if(_wasInAir)
            {
                PlayLand();
            }

            _wasInAir = false;
        }
        else
        {
            if(GravityVelocity == Vector3.zero)
            {
                GravityVelocity += -_parent.up * GravitySpeed * 0.5f;
            }

            GravityVelocity += -_parent.up * GravitySpeed * deltaTime;
            _parent.position += GravityVelocity * deltaTime;
            OnLeftElevator();

            _wasInAir = true;
        }

        if (dir.magnitude > 0.01f)
        {
            Animator.SetTrigger("Running");

            if (!Physics.Raycast(transform.position, dir, out hit, 0.5f, LayerMask.GetMask("Ground")))
            {
                _parent.position += dir;
            }

            _parent.transform.localScale = new Vector3(
                Mathf.Sign(FacingDirection) * Mathf.Abs(_parent.transform.localScale.x), _parent.transform.localScale.y, _parent.transform.localScale.z
            );

            if (IsTouchingGround)
            {
                if (_footstepDelay > 0.3f)
                {
                    PlayFootstep();
                }
            }
        }
        else
        {
            Animator.SetTrigger("Idle");
        }
    }

    private void OnLeftElevator()
    {
        _parent.transform.parent = null;
        _rb.isKinematic = true;
        _rb.constraints |= RigidbodyConstraints.FreezePositionY;
    }

    public void PlayFootstep()
    {
        _footstepDelay = 0.0f;
        _audio.pitch = 1.0f;
        _audio.PlayOneShot(_footsteps[Random.Range(0, _footsteps.Length)]);
    }

    public void PlayLand()
    {
        _audio.pitch = Random.Range(0.95f, 1.05f);
        _audio.PlayOneShot(_land);
    }

    public void PlayDeath()
    {
        _audio.pitch = Random.Range(0.95f, 1.05f);
        _audio.PlayOneShot(_death);
    }

    public bool HitsGround(out RaycastHit hit)
    {
        return Physics.Raycast(transform.position, -transform.up, out hit, 1.0f, LayerMask.GetMask("Ground"));
    }

    public void Respawn()
    {
        if (!_checkpoint)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            Animator.Play("Idle");
            var pos = _checkpoint.transform.position;
            pos.z = Parent.position.z;
            Parent.position = pos;
            Parent.rotation = Quaternion.Euler(Vector3.zero);
            StateMachine.ChangeState(PlayerState.Moving);
            Collider.enabled = true;

            if (_lensOnTheBack.activeSelf)
            {
                _lensOnTheBack.transform.parent = _originalLensParent;
                _lensOnTheBack.GetComponent<Rigidbody>().isKinematic = true;
                _lensOnTheBack.transform.localPosition = _originalLensPosition;
                _lensOnTheBack.transform.localRotation = _originalLensRotation;
                _lensOnTheBack.SetActive(false);

                _heldObject.transform.position = _pickLensPosition;
                _heldObject.gameObject.SetActive(true);
                _heldObject = null;
            }
        }

        StartCoroutine(DeathCooldown());
    }

    private IEnumerator DeathCooldown()
    {
        _canDie = false;

        yield return new WaitForSeconds(0.3f);
        var fade = GameObject.Find("Fade");
        if(fade)
            fade.BroadcastMessage("FadeIn");

        _canDie = true;
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
        _rb = GetComponent<Rigidbody>();
        Controls = new MainControls();

        StateMachine.AddState(PlayerState.Moving, new PlayerMoving(this));
        StateMachine.AddState(PlayerState.Dashing, new PlayerDashing(this));
        StateMachine.AddState(PlayerState.ReceivedDamage, new PlayerReceivedDamage(this));
        StateMachine.AddState(PlayerState.FailedToFlip, new PlayerFailedToFlip(this));
        StateMachine.AddState(PlayerState.Flipping, new PlayerFlipping(this));
        StateMachine.ChangeState(PlayerState.Moving);

        _currentHealth = Health;

        _originalLensParent = _lensOnTheBack.transform.parent;
        _originalLensPosition = _lensOnTheBack.transform.localPosition;
        _originalLensRotation = _lensOnTheBack.transform.localRotation;

        Controls.Enable();

        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            PlayerInput = playerInput;
            PlayerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
            PlayerInput.onControlsChanged += (ctx) => CurrentControls = ctx.currentControlScheme;
            CurrentControls = PlayerInput.currentControlScheme;
        }
    }

    void Update()
    {
        CooldownUpdate();
        StateMachine.OnUpdate(Time.deltaTime);

        if (Controls.Player.Interact.triggered && IsTouchingGround)
        {
            if (_heldObject == null)
            {
                var objects = Physics.OverlapSphere(Parent.position + Vector3.forward*2.0f, 1f, LayerMask.GetMask("Lens"));
                foreach(var obj in objects)
                {
                    _heldObject = obj.GetComponent<Interactable>();
                    if (_heldObject != null)
                    {
                        _heldObject.OnInteractionStart();
                        _pickLensPosition = _heldObject.transform.position;

                        var meshRenderer = _heldObject.GetComponentInChildren<MeshRenderer>();
                        _lensOnTheBack.SetActive(true);
                        _lensOnTheBack.GetComponentInChildren<MeshRenderer>().materials = meshRenderer.materials;

                        _audio.PlayOneShot(_takeBox[0], 0.5f);
                        break;
                    }
                }
            }
            else
            {
                _heldObject.OnInteractionEnd();
                _heldObject.transform.position = GetLensPosition();
                _heldObject = null;
                _lensOnTheBack.SetActive(false);
                _audio.PlayOneShot(_takeBox[1], 0.5f);
            }
        }

        _footstepDelay += Time.deltaTime;
    }

    Vector3 GetLensPosition()
    {
        var verDot = Vector3.Dot(Parent.up, Vector3.up);
        var horDot = Vector3.Dot(Parent.up, Vector3.right);

        Debug.Log(string.Format("ver: {0} hor: {1}", verDot, horDot));
        if (Mathf.Abs(verDot) >= 0.5f)
        {
            return new Vector3(Mathf.Round(Parent.position.x * 2.0f) / 2.0f, Parent.position.y + Parent.up.y * 0.4f, _heldObject.transform.position.z);
        }
        else if (Mathf.Abs(horDot) >= 0.5f)
        {
            return new Vector3(Parent.position.x + Parent.up.x * 0.4f, Mathf.Round(Parent.position.y * 2.0f) / 2.0f, _heldObject.transform.position.z);
        }
        else
        {
            return new Vector3(Mathf.Round(Parent.position.x * 2.0f) / 2.0f, Mathf.Round(Parent.position.y * 2.0f) / 2.0f, _heldObject.transform.position.z);
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
            StateMachine.ChangeState(PlayerState.ReceivedDamage);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //if(other.CompareTag("EnemyWeapon") && StateMachine.CurrentState != PlayerState.Dashing && StateMachine.CurrentState != PlayerState.ReceivedDamage)
        //{
        //    StateMachine.ChangeState(PlayerState.ReceivedDamage);
        //}
        if(other.CompareTag("Checkpoint"))
        {
            _checkpoint = other.gameObject;
            _checkpoint.BroadcastMessage("OnCheckPointEnter");
        }
        else if (other.CompareTag("SpaceDeath"))
        {
            Controls.Player.Disable();
            StartCoroutine(SpaceDeath());
        }
    }

    IEnumerator SpaceDeath()
    {
        var fade = GameObject.Find("Fade");
        if (fade)
            fade.BroadcastMessage("FadeOut");

        yield return new WaitForSeconds(0.5f);

        Respawn();
        Controls.Player.Enable();
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
        if(_canDie)
            StateMachine.ChangeState(PlayerState.ReceivedDamage);
    }

    void OnSwitchFailEnd()
    {
        StateMachine.ChangeState(PlayerState.Moving);
    }
#endregion
}