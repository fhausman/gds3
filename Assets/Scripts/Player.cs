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

    public PlayerMoving(Player p)
    {
        player = p;
    }

    public override void onInit(params object[] args)
    {
    }

    public override void onUpdate(float deltaTime)
    {
        if(Keyboard.current.spaceKey.wasPressedThisFrame && player.CanSwitch)
        {
            player.Flip();
        }
        if(Keyboard.current.leftCtrlKey.wasPressedThisFrame)
        {
            player.StateMachine.ChangeState(PlayerState.Dashing);
        }
    }

    public override void onFixedUpdate(float deltaTime)
    {
        var horizontalInput = Vector3.Dot(player.transform.right, Vector3.right) * Input.GetAxis("Horizontal");
        var verticalInput = Vector3.Dot(player.transform.right, Vector3.up) * Input.GetAxis("Vertical");
        var inputDirectionModifier = horizontalInput + verticalInput;

        if(Mathf.Abs(inputDirectionModifier) > 0.0f + Mathf.Epsilon)
        {
            player.FacingDirection = Mathf.Sign(inputDirectionModifier);
        }

        var horizontalMove = player.transform.right * inputDirectionModifier * player.Speed * deltaTime;
        player.Move(horizontalMove, deltaTime);
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
    #endregion

    #region Outside References
    [SerializeField]
    private Transform _parent = null;
    #endregion

    #region Private Fields
    private Vector3 _gravityVelocity = Vector3.zero;
    #endregion

    #region Properties
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
        StateMachine.AddState(PlayerState.Moving, new PlayerMoving(this));
        StateMachine.AddState(PlayerState.Dashing, new PlayerDashing(this));
        StateMachine.ChangeState(PlayerState.Moving);
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