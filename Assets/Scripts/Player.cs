using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

#region Player States
public enum PlayerState
{
    Moving,
    Dashing,
    Attacking,
    ReceivedDamage
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
        player.Controls.Player.Attack.performed += Attack;
        player.Controls.Player.Block.performed += Block;
        player.Controls.Player.Block.canceled += LeavingBlock;
    }

    public override void onExit()
    {
        player.Controls.Player.GravitySwitch.performed -= GravitySwitch;
        player.Controls.Player.Dash.performed -= Dash;
        player.Controls.Player.Attack.performed -= Attack;
        player.Controls.Player.Block.performed -= Block;
        player.Controls.Player.Block.canceled -= LeavingBlock;

        player.Weapon.SetIdle();
        player.Blocking = false;
        speedModifier = 1.0f;
    }

    public override void onUpdate(float deltaTime)
    {
        if (player.Blocking)
        {
            speedModifier = player.BlockSpeedModifier;
            if (player.Aim.y >= 0.0f)
            {
                player.Weapon.SetUpper();
            }
            else
            {
                player.Weapon.SetBottom();
            }
        }
        else
        {
            player.Weapon.SetIdle();
            speedModifier = 1.0f;
        }
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
        if(player.CanSwitch)
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

    private void Attack(InputAction.CallbackContext ctx)
    {
        if (player.WeaponEquipped)
        {
            player.StateMachine.ChangeState(PlayerState.Attacking);
        }
    }

    private void Block(InputAction.CallbackContext ctx)
    {
        Debug.Log("BLOCKING");
        player.Blocking = true;

        var zoneSize = player.Aim.y >= 0.0f ? player.UpperBlockZoneSize : player.BottomBlockZoneSize;
        var projectiles =
            Physics.OverlapBox(
                player.transform.position + new Vector3(player.FacingDirection * (zoneSize.x + (player.SweetSpotWidth) / 2), Mathf.Sign(player.Aim.y) * zoneSize.y / 2),
                new Vector3(player.SweetSpotWidth / 2, zoneSize.y / 2, 0.5f),
                player.transform.rotation,
                LayerMask.GetMask("Projectiles")
            );

        foreach (var proj in projectiles)
        {
            proj.SendMessage("Reflect");
        }
    }

    private void LeavingBlock(InputAction.CallbackContext ctx)
    {
        if (player.Blocking)
        {
            Debug.Log("NOT BLOCKING");
            player.Blocking = false;
        }
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
        player.Controls.Player.Attack.performed += Attack;
        player.Controls.Player.Block.performed += Block;

        dashTimeElapsed = 0.0f;
    }

    public override void onExit()
    {
        player.Controls.Player.Attack.performed -= Attack;
        player.Controls.Player.Block.performed -= Block;

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

    private void Attack(InputAction.CallbackContext ctx)
    {
        if (player.WeaponEquipped)
        {
            player.StateMachine.ChangeState(PlayerState.Attacking);
        }
    }

    private void Block(InputAction.CallbackContext ctx)
    {
        player.Blocking = true;
        player.StateMachine.ChangeState(PlayerState.Moving);
    }
}

public class PlayerAttacking : BaseState
{
    private Player player = null;
    private float attackTimeElapsed = 0.0f;

    public PlayerAttacking(Player p)
    {
        player = p;
    }

    public override void onInit(params object[] args)
    {
        player.Controls.Player.GravitySwitch.performed += GravitySwitch;
        player.Controls.Player.Dash.performed += Dash;

        attackTimeElapsed = 0.0f;

        Collider[] hitObjects = GetHitObjects(player.Aim.y >= 0.0f ? player.UpperBlockZoneSize : player.BottomBlockZoneSize);
        foreach (var obj in hitObjects)
        {
            obj.SendMessage("ReceivedDamage");
        }

        if (player.Aim.y >= 0.0f)
        {
            player.Weapon.SetUpper();
        }
        else
        {
            player.Weapon.SetBottom();
        }
    }

    public override void onExit()
    {
        player.Controls.Player.GravitySwitch.performed -= GravitySwitch;
        player.Controls.Player.Dash.performed -= Dash;

        player.Weapon.SetIdle();
    }

    public override void onUpdate(float deltaTime)
    {
        if (attackTimeElapsed > player.AttackDuration)
        {
            player.StateMachine.ChangeState(PlayerState.Moving);
        }

        var horizontalMove =
            player.FacingDirection * player.transform.right * (player.Speed + 1.0f - (attackTimeElapsed / player.AttackDuration) * player.Speed) * deltaTime;
        player.Move(horizontalMove, deltaTime);

        attackTimeElapsed += deltaTime;
    }

    private Collider[] GetHitObjects(Vector2 zoneSize)
    {
        return Physics.OverlapBox(
                    player.transform.position + new Vector3(player.FacingDirection * zoneSize.x / 2, Mathf.Sign(player.Aim.y) * zoneSize.y / 2),
                    new Vector3(zoneSize.x / 2, zoneSize.y / 2, 0.5f),
                    player.transform.rotation,
                    LayerMask.NameToLayer("Enemies")
                    );
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
    }

    public override void onUpdate(float deltaTime)
    {
        if (damageCooldownElapsed > damageCooldownDuration)
        {
            player.StateMachine.ChangeState(PlayerState.Moving);
        }

        var horizontalMove =
            impactDirection * player.transform.right * (player.Speed + 1.0f - (damageCooldownElapsed / damageCooldownDuration) * player.Speed) * deltaTime;
        player.Move(horizontalMove, deltaTime);

        damageCooldownElapsed += deltaTime;
    }
}
#endregion

public class Player : MonoBehaviour
{
    #region Settings
    public PlayerSettings settings;
    public float Speed { get => settings.speed; }
    public float GravitySpeed { get => settings.gravitySpeed; }
    public float BlockSpeedModifier { get => settings.blockSpeedModifier; }
    public float DashSpeed { get => settings.dashSpeed; }
    public float DashTime { get => settings.dashTime; }
    public float DashCooldown { get => settings.dashCooldown; }
    public float AttackDuration { get => settings.attackDuration; }
    public Vector2 UpperBlockZoneSize { get => settings.upperBlockZoneSize; }
    public Vector2 BottomBlockZoneSize { get => settings.bottomBlockZoneSize; }
    public float SweetSpotWidth { get => settings.sweetSpotWidth; }
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
    #endregion

    #region Properties
    public MainControls Controls { get; private set; } = null;
    public StateMachine<PlayerState> StateMachine { get; private set; } = new StateMachine<PlayerState>();
    private Vector3 GravityVelocity { get; set; } = Vector3.zero;
    public Vector2 Aim { get; set; } = Vector2.zero;
    public bool Blocking { get; set; } = false;
    public bool CanSwitch { get; set; } = false;
    public bool WeaponEquipped { get; set; } = true;
    public bool CanDash { get => DashCooldownElapsed > DashCooldown; }
    public float FacingDirection { get; set; } = 1.0f;
    public float DashCooldownElapsed { get; set; } = 0.0f;

    public Bounds UpperBlockAreaBounds
    {
        get =>
            new Bounds(
                transform.position + new Vector3(FacingDirection * UpperBlockZoneSize.x / 2, Mathf.Sign(Aim.y) * UpperBlockZoneSize.y / 2),
                new Vector3(UpperBlockZoneSize.x, UpperBlockZoneSize.y, 0.5f));
    }
    public Bounds BottomBlockAreaBounds
    {
        get =>
              new Bounds(
                  transform.position + new Vector3(FacingDirection * BottomBlockZoneSize.x / 2, Mathf.Sign(Aim.y) * BottomBlockZoneSize.y / 2),
                  new Vector3(BottomBlockZoneSize.x, BottomBlockZoneSize.y, 0.5f));
    }
    #endregion

    public void Move(Vector3 dir, float deltaTime)
    {
        RaycastHit hit;
        //vertical movement
        if (Physics.Raycast(transform.position, -transform.up, out hit, 1.0f, 1 << 8))
        {
            //Debug.DrawRay(hit.point, hit.normal, Color.red);
            _parent.position = hit.point;
            _parent.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            GravityVelocity = Vector3.zero;
            CanSwitch = true;
        }
        else
        {
            //TODO: change state to falling
            GravityVelocity += -_parent.up * GravitySpeed * deltaTime;
            _parent.position += GravityVelocity * deltaTime;
            CanSwitch = false;
        }

        if (Mathf.Abs(dir.x) > 0.0f + Mathf.Epsilon)
        {
            if (!Physics.Raycast(transform.position, dir, out hit, 0.22f, 1 << 8))
            {
                _parent.position += dir;
            }
        }

        _parent.transform.localScale = new Vector3(
            Mathf.Sign(FacingDirection) * Mathf.Abs(_parent.transform.localScale.x), _parent.transform.localScale.y, _parent.transform.localScale.z
        );
    }

    public void Flip()
    {
        _parent.Rotate(0, 0, 180);
        _parent.position -= _parent.up;
    }

    #region Mono behaviour methods
    void Start()
    {
        Controls = new MainControls();

        StateMachine.AddState(PlayerState.Moving, new PlayerMoving(this));
        StateMachine.AddState(PlayerState.Dashing, new PlayerDashing(this));
        StateMachine.AddState(PlayerState.Attacking, new PlayerAttacking(this));
        StateMachine.AddState(PlayerState.ReceivedDamage, new PlayerReceivedDamage(this));
        StateMachine.ChangeState(PlayerState.Moving);

        Controls.Enable();
    }

    void Update()
    {
        Aim = Controls.Player.Aim.ReadValue<Vector2>();
        DashCooldownElapsed += Time.deltaTime;
        StateMachine.OnUpdate(Time.deltaTime);
    }

    void FixedUpdate()
    {
        StateMachine.OnFixedUpdate(Time.fixedDeltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.CompareTag("Projectile"))
        {
            if(Blocking)
            {
                if (collision.collider.bounds.Intersects(Aim.y >= 0.0f ? UpperBlockAreaBounds : BottomBlockAreaBounds))
                {
                    collision.collider.SendMessage("Destroy");
                    return;
                }
            }

            var impactDirection = Mathf.Sign(Vector3.Dot(transform.right, collision.collider.attachedRigidbody.velocity));
            StateMachine.ChangeState(PlayerState.ReceivedDamage, impactDirection);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Aim.y > 0.0f + Mathf.Epsilon ?
            Color.green : Color.red;
        DrawZonesDebug(UpperBlockZoneSize, 1.0f);

        Gizmos.color = Aim.y < 0.0f - Mathf.Epsilon ?
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