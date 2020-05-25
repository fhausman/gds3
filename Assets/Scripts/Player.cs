using System;
using UnityEngine;
using UnityEngine.InputSystem;

#region Player States
public enum PlayerState
{
    Moving,
    Dashing
}

public class PlayerMoving : BaseState
{
    private Player player = null;
    private float dashCooldownElapsed = 0.0f;

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
    }

    public override void onUpdate(float deltaTime)
    {
        dashCooldownElapsed += deltaTime;
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

        var horizontalMove = player.transform.right * inputDirectionModifier * player.Speed * deltaTime;
        player.Move(horizontalMove, deltaTime);
    }

    private void GravitySwitch(InputAction.CallbackContext ctx)
    {
        if(player.CanSwitch)
        {
            player.Flip();
        }
    }

    private void Dash(InputAction.CallbackContext ctx)
    {
        if (dashCooldownElapsed >= player.DashCooldown)
        {
            player.StateMachine.ChangeState(PlayerState.Dashing);
            dashCooldownElapsed = 0.0f;
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
        dashTimeElapsed = 0.0f;
    }

    public override void onFixedUpdate(float deltaTime)
    {
        if(dashTimeElapsed <= player.DashTime)
        {
            var horizontalMove =
                player.FacingDirection * player.transform.right * player.DashSpeed * deltaTime;
            player.Move(horizontalMove, deltaTime);
        }
        else
        {
            player.StateMachine.ChangeState(PlayerState.Moving);
        }

        dashTimeElapsed += deltaTime;
    }
}
#endregion

public class Player : MonoBehaviour
{
    #region Settings
    public PlayerSettings settings;
    public float Speed { get => settings.speed; }
    public float GravitySpeed { get => settings.gravitySpeed; }
    public float DashSpeed { get => settings.dashSpeed; }
    public float DashTime { get => settings.dashTime; }
    public float DashCooldown { get => settings.dashCooldown; }
    #endregion

    #region Outside References
    [SerializeField]
    private Transform _parent = null;
    #endregion

    #region Private Fields
    private Vector3 _gravityVelocity = Vector3.zero;
    #endregion

    #region Properties
    public MainControls Controls { get; private set; } = null;
    public StateMachine<PlayerState> StateMachine { get; private set; } = new StateMachine<PlayerState>();
    public bool CanSwitch { get; private set; } = false;
    public float FacingDirection { get; set; } = 1.0f;
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
            _gravityVelocity = Vector3.zero;
            CanSwitch = true;
        }
        else
        {
            //TODO: change state to falling
            _gravityVelocity += -_parent.up * GravitySpeed * deltaTime;
            _parent.position += _gravityVelocity * deltaTime;
            CanSwitch = false;
        }

        if (Mathf.Abs(dir.x) > 0.0f + Mathf.Epsilon)
        {
            if (!Physics.Raycast(transform.position, dir, out hit, 0.22f, 1 << 8))
            {
                _parent.position += dir;
            }
        }
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
        StateMachine.ChangeState(PlayerState.Moving);

        Controls.Enable();
    }

    void Update()
    {
        StateMachine.OnUpdate(Time.deltaTime);
    }

    void FixedUpdate()
    {
        StateMachine.OnFixedUpdate(Time.fixedDeltaTime);
    }
    #endregion
}