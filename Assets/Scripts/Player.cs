using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerState
{
    Running,
}

public class PlayerBase : BaseState
{
    protected Player player;
    protected Vector3 movementVector = new Vector3();
    protected Vector3 gravityDirection;

    public PlayerBase(Player p)
    {
        player = p;
    }

    public override void onInit(params object[] args)
    {
        gravityDirection = -player.transform.up;
    }

    public override void onUpdate(float deltaTime)
    {
        if(Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            player.parent.Rotate(0, 180, 180);
            player.Velocity = Vector3.zero;
        }

        //gravityDirection = -player.transform.up;
        //RaycastHit hit;
        //if (Physics.Raycast(player.transform.position, -player.transform.up, out hit, 3.0f, 1 << 8))
        //{
        //    var angle = Vector3.Angle(player.transform.up, hit.normal);
        //    if(angle > 0.0f)
        //    {
        //        player.transform.RotateAround(hit.point, Vector3.forward, angle);
        //    }
        //}
    }

    public override void onFixedUpdate(float deltaTime)
    {
        RaycastHit hit;

        //vertical movement
        if (Physics.Raycast(player.transform.position, -player.transform.up, out hit, 0.52f, 1 << 8))
        {
            //Debug.DrawRay(player.transform.position, -player.transform.up * 1.0f, Color.yellow);
            Debug.DrawRay(hit.point, hit.normal, Color.red);
            player.parent.position = hit.point;
            player.parent.rotation = Quaternion.FromToRotation(player.transform.up, hit.normal) * player.transform.rotation;
        }
        else
        {
            player.parent.position -= player.parent.up * 0.5f;
        }

        //horizontal movement
        var horizontalMove = player.transform.right * Input.GetAxis("Horizontal") * 0.2f;
        if (Physics.Raycast(player.transform.position, player.transform.right, out hit, 0.22f, 1 << 8))
        {
            horizontalMove.x = Mathf.Clamp(horizontalMove.x, -1.0f, 0.0f);
            player.parent.position = Vector3.right * (hit.point.x - 0.22f);// - player.parent.position;
            //Debug.Log(string.Format("hit: {0} player: {0}", hit.point, player.transform.position));
        }
        else if (Physics.Raycast(player.transform.position, -player.transform.right, out hit, 0.22f, 1 << 8))
        {
            horizontalMove.x = Mathf.Clamp(horizontalMove.x, 0.0f, 1.0f);
            player.parent.position = hit.point;// + player.parent.position;
        }

        player.parent.position += horizontalMove;
    }
}

public class Player : MonoBehaviour
{
    [SerializeField]
    private float _speed;
    public float Speed { get => _speed; }

    [SerializeField]
    private float _dashSpeed;
    public float DashSpeed { get => _dashSpeed; }

    [SerializeField]
    private float _fallSpeed;
    public float FallSpeed { get => _fallSpeed; }

    [SerializeField]
    private float _jumpForce;
    public float JumpForce { get => _jumpForce; }

    [SerializeField]
    private float _jumpTime;
    public float JumpTime { get => _jumpTime; }

    [SerializeField]
    private float _coyoteTime;
    public float CoyoteTime { get => _coyoteTime; }

    [SerializeField]
    private bool _doubleJump = false;
    public bool DoubleJump { get => _doubleJump; }

    public Vector3 Velocity { get; set; }
    public bool JumpEnabled { get; set; }
    public Transform parent;

    public StateMachine<PlayerState> StateMachine { get; private set; } = new StateMachine<PlayerState>();
    public Rigidbody Rigidbody { get; private set; }

    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        StateMachine.AddState(PlayerState.Running, new PlayerBase(this));
        StateMachine.ChangeState(PlayerState.Running);
    }

    void Update()
    {
        StateMachine.OnUpdate(Time.deltaTime);
    }

    private void FixedUpdate()
    {
        StateMachine.OnFixedUpdate(Time.fixedDeltaTime);
    }
}