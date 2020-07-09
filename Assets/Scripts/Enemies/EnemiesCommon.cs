using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum EnemyStates
{
    Idle,
    Shooting,
    Patroling,
    InFight,
    Dashing,
    Dead,
    Damaged
}

public class EnemyPatroling : BaseState
{
    EnemyBase enemy;
    GameObject playerRef;
    List<Vector3> patrolingPoints = new List<Vector3>();
    int currentPoint = 0;

    public EnemyPatroling(EnemyBase e)
    {
        enemy = e;
        patrolingPoints = e.PatrolingPoints.Select(t => t.position).ToList();
    }

    public override void onFixedUpdate(float deltaTime)
    {
        enemy.Logic.MoveTowards(patrolingPoints[currentPoint], enemy.Speed * deltaTime);
    }

    public override void onUpdate(float deltaTime)
    {
        var target = patrolingPoints[currentPoint];
        var current = enemy.transform.position;
        if (Mathf.Abs(target.x - current.x) < Mathf.Epsilon)
        {
            currentPoint = (currentPoint + 1) % patrolingPoints.Count;
        }

        var resolution = 20;
        for(int i = 0; i <= resolution; i++)
        {
            var dir = Quaternion.Euler(0.0f, 0.0f, i * (180 / resolution)) * enemy.transform.right;
            if (enemy.Logic.LookForPlayer(dir, enemy.Range, out playerRef))
            {
                enemy.StateMachine.ChangeState(EnemyStates.InFight, playerRef);
            }
        }
    }
}

public class EnemyDamaged : BaseState
{
    EnemyBase enemy;
    Rigidbody rigidbody;
    float angle = 20.0f;
    float damagedDelay = 1.0f;
    float currentDelay = 0.0f;

    public EnemyDamaged(EnemyBase e)
    {
        enemy = e;
        rigidbody = enemy.GetComponent<Rigidbody>();
        currentDelay = 0.0f;
    }

    public override void onInit(params object[] args)
    {
        rigidbody.velocity = Vector3.zero;
        rigidbody.AddForce((Quaternion.Euler(0.0f, 0.0f, angle) * enemy.transform.right) * 3, ForceMode.Impulse);
        enemy.Health -= 1;
    }

    public override void onUpdate(float deltaTime)
    {
        if(currentDelay >= damagedDelay)
        {
            if(enemy.Health <= 0)
            {
                enemy.StateMachine.ChangeState(EnemyStates.Dead);
            }

            enemy.StateMachine.ChangeState(EnemyStates.InFight);
        }

        currentDelay += deltaTime;
    }
}

public class EnemyDead : BaseState
{
    EnemyBase enemy;

    public EnemyDead(EnemyBase e)
    {
        enemy = e;
    }

    public override void onInit(params object[] args)
    {
        Object.Destroy(enemy.gameObject);
    }
}

public class EnemyDashing : BaseState
{
    EnemyBase enemy;
    Rigidbody rigidbody;

    public EnemyDashing(EnemyBase e)
    {
        enemy = e;
        rigidbody = enemy.GetComponent<Rigidbody>();
    }

    public override void onInit(params object[] args)
    {
        var dir = (float) args[0];
        rigidbody.AddForce(enemy.transform.right * dir * enemy.DashForce, ForceMode.Impulse);
    }

    public override void onUpdate(float deltaTime)
    {
        if(rigidbody.velocity.magnitude <= 0.0f + Mathf.Epsilon)
        {
            enemy.StateMachine.ChangeState(EnemyStates.InFight);
        }
    }
}

public class EnemyCommonLogic
{
    private Transform _transform;

    public EnemyCommonLogic(Transform t)
    {
        _transform = t;
    }

    public bool LookForPlayer(Vector3 dir, float range, out GameObject player)
    {
        RaycastHit hit;
        if(Physics.Raycast(_transform.position, dir, out hit, range, LayerMask.GetMask("Player")))
        {
            player = hit.collider.gameObject;
            return true;
        }
        else
        {
            player = null;
            return false;
        }

    }

    public void MoveTowards(Vector3 target, float speed)
    {
        _transform.position = Vector3.MoveTowards(_transform.position,
            new Vector3(target.x, _transform.position.y, _transform.position.z), speed);
    }
}

public class EnemyBase : MonoBehaviour
{
    #region Settings
    [SerializeField]
    private EnemySettings settings = null;
    public float Speed { get => settings.speed; }
    public float ShootRate { get => settings.shootRate; }
    public float Range { get => settings.range; }
    public float SafeDistance { get => settings.safeDistance; }
    public float DashForce { get => settings.dashForce; }
    #endregion

    [SerializeField]
    private Projectile projectile = null;
    public Projectile Projectile { get => projectile; }

    [SerializeField]
    private SpriteRenderer spriteRenderer = null;
    public SpriteRenderer SpriteRenderer { get => spriteRenderer; }

    [SerializeField]
    private Transform[] patrolingPoints = null;
    public Transform[] PatrolingPoints { get => patrolingPoints; }

    public StateMachine<EnemyStates> StateMachine { get; private set; } = new StateMachine<EnemyStates>();
    public EnemyCommonLogic Logic { get; private set; }
    public GameObject PlayerRef { get; set; } = null;
    public float FacingDirection { get; set; } = -1.0f;
    public float TimeBetweenShots { get => 1 / ShootRate; }
    public int Health { get; set; } = 0;

    protected void Start()
    {
        StateMachine.AddState(EnemyStates.Dead, new EnemyDead(this));
        StateMachine.AddState(EnemyStates.Damaged, new EnemyDamaged(this));

        Logic = new EnemyCommonLogic(transform);
        Health = settings.health;
    }

    protected void Update()
    {
        StateMachine.OnUpdate(Time.deltaTime);
    }

    protected void FixedUpdate()
    {
        StateMachine.OnFixedUpdate(Time.deltaTime);
    }

    protected void OnCollisionEnter(Collision collision)
    {
        var obj = collision.collider.gameObject;
        if (obj.CompareTag("Projectile"))
        {
            if (obj.GetComponent<Projectile>().IsReflected)
            {
                ReceivedDamage();
            }
        }
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Weapon"))
        {
            ReceivedDamage();
        }
    }

    private void ReceivedDamage()
    {
        StateMachine.ChangeState(EnemyStates.Damaged);
    }
}
