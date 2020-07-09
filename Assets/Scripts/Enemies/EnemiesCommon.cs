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

        if (enemy.Logic.IsInRange && enemy.Logic.HasClearShot)
        {
            enemy.StateMachine.ChangeState(EnemyStates.InFight);
        }
    }
}

public class EnemyDamaged : BaseState
{
    EnemyBase enemy;
    Rigidbody rigidbody;
    float force = 3.0f;
    float angle = 20.0f;
    float damagedDelay = 1.0f;
    float currentDelay = 0.0f;

    public EnemyDamaged(EnemyBase e)
    {
        enemy = e;
        rigidbody = enemy.GetComponent<Rigidbody>();
        currentDelay = damagedDelay;
    }

    public override void onInit(params object[] args)
    {
        rigidbody.velocity = Vector3.zero;
        rigidbody.AddForce((Quaternion.Euler(0.0f, 0.0f, angle) * enemy.transform.right) * force, ForceMode.Impulse);
        enemy.Health -= 1;
    }

    public override void onUpdate(float deltaTime)
    {
        if(currentDelay >= damagedDelay)
        {
            if(enemy.Health <= 0)
            {
                enemy.StateMachine.ChangeState(EnemyStates.Dead);
                return;
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
    private EnemyBase _enemy;

    public EnemyCommonLogic(EnemyBase e)
    {
        _enemy = e;
    }

    //public bool LookForPlayer(Vector3 dir, float range, out GameObject player)
    //{
    //    RaycastHit hit;
    //    if(Physics.Raycast(_transform.position, dir, out hit, range, LayerMask.GetMask("Player")))
    //    {
    //        player = hit.collider.gameObject;
    //        return true;
    //    }
    //    else
    //    {
    //        player = null;
    //        return false;
    //    }
    //}

    public bool IsPlayerOnLeft
    {
        get => _enemy.transform.position.x <= _enemy.PlayerRef.transform.position.x;
    }

    public bool IsInRange
    {
        get => _enemy.Range > Vector3.Distance(_enemy.transform.position, _enemy.PlayerRef.transform.position);
    }

    public bool HasClearShot
    {
        get
        {
            return !Physics.Linecast(_enemy.transform.position, _enemy.PlayerRef.transform.position, LayerMask.GetMask("Ground"));
        }
    }

    public void MoveTowards(Vector3 target, float speed)
    {
        var _transform = _enemy.transform;
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
    public Player PlayerRef { get; set; } = null;
    public float FacingDirection { get; set; } = -1.0f;
    public float TimeBetweenShots { get => 1 / ShootRate; }
    public int Health { get; set; } = 0;

    protected void Start()
    {
        StateMachine.AddState(EnemyStates.Dead, new EnemyDead(this));
        StateMachine.AddState(EnemyStates.Damaged, new EnemyDamaged(this));

        Logic = new EnemyCommonLogic(this);
        Health = settings.health;
        PlayerRef = GameObject.Find("Player").GetComponentInChildren<Player>();
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
