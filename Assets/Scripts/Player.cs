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
        }
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
            player.parent.position = Vector3.right * (hit.point.x - 0.22f);
        }
        else if (Physics.Raycast(player.transform.position, -player.transform.right, out hit, 0.22f, 1 << 8))
        {
            horizontalMove.x = Mathf.Clamp(horizontalMove.x, 0.0f, 1.0f);
            player.parent.position = hit.point;
        }

        player.parent.position += horizontalMove;
    }
}

public class Player : MonoBehaviour
{
    [SerializeField]
    private float _speed = 0.0f;
    public float Speed { get => _speed; }

    [SerializeField]
    private float _dashSpeed = 0.0f;
    public float DashSpeed { get => _dashSpeed; }

    [SerializeField]
    private float _gravitySpeed = 0.0f;
    public float GravitySpeed { get => _gravitySpeed; }

    public Transform parent;

    public StateMachine<PlayerState> StateMachine { get; private set; } = new StateMachine<PlayerState>();

    void Start()
    {
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