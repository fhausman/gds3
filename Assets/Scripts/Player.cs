using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerState
{
    Moving,
}

public class PlayerMain : BaseState
{
    private Player player;
    private Vector3 gravityVelocity = Vector3.zero;
    private bool canSwitch = false;

    public PlayerMain(Player p)
    {
        player = p;
    }

    public override void onInit(params object[] args)
    {
    }

    public override void onUpdate(float deltaTime)
    {
        if(Keyboard.current.spaceKey.wasPressedThisFrame && canSwitch)
        {
            player.parent.Rotate(0, 0, 180);
            player.parent.position -= player.parent.up;
        }
    }

    public override void onFixedUpdate(float deltaTime)
    {
        var horizontalInput = Vector3.Dot(player.transform.right, Vector3.right) * Input.GetAxis("Horizontal");
        var verticalInput = Vector3.Dot(player.transform.right, Vector3.up) * Input.GetAxis("Vertical");

        var horizontalMove = player.transform.right * (horizontalInput + verticalInput) * player.Speed * deltaTime;
        player.parent.position += horizontalMove;

        RaycastHit hit;
        //vertical movement
        if (Physics.Raycast(player.transform.position, -player.transform.up, out hit, 0.52f, 1 << 8))
        {
            //Debug.DrawRay(hit.point, hit.normal, Color.red);
            player.parent.position = hit.point;
            player.parent.rotation = Quaternion.FromToRotation(player.transform.up, hit.normal) * player.transform.rotation;
            gravityVelocity = Vector3.zero;
            canSwitch = true;
        }
        else
        {
            gravityVelocity += -player.parent.up * player.GravitySpeed * deltaTime;
            player.parent.position += gravityVelocity * deltaTime;
            canSwitch = false;
        }
    }
}

public class Player : MonoBehaviour
{
    public PlayerSettings settings;

    public float Speed { get => settings.speed; }
    public float DashSpeed { get => settings.dashSpeed; }
    public float GravitySpeed { get => settings.gravitySpeed; }

    public Transform parent;

    public StateMachine<PlayerState> StateMachine { get; private set; } = new StateMachine<PlayerState>();

    void Start()
    {
        StateMachine.AddState(PlayerState.Moving, new PlayerMain(this));
        StateMachine.ChangeState(PlayerState.Moving);
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