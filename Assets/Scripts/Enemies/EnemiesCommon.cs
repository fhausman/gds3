using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

public class EnemyCommonLogic
{
    private Transform _transform;

    public EnemyCommonLogic(Transform t)
    {
        _transform = t;
    }

    public bool LookForPlayer(Vector3 dir, float range)
    {
        RaycastHit hit;
        return Physics.Raycast(_transform.position, dir, out hit, range, LayerMask.GetMask("Player"));
    }
}

public class EnemyBase : MonoBehaviour
{
    #region Settings
    [SerializeField]
    private EnemySettings settings = null;
    public float ShootRate { get => settings.shootRate; }
    public float Range { get => settings.range; }
    #endregion

    [SerializeField]
    private Projectile projectile = null;
    public Projectile Projectile { get => projectile; }

    [SerializeField]
    private SpriteRenderer spriteRenderer = null;
    public SpriteRenderer SpriteRenderer { get => spriteRenderer; }

    public StateMachine<StaticEnemyStates> StateMachine { get; private set; } = new StateMachine<StaticEnemyStates>();
    public EnemyCommonLogic Logic { get; private set; }
    public float FacingDirection { get; set; } = -1.0f;
    public float TimeBetweenShots { get => 1 / ShootRate; }
    public int Health { get; set; } = 0;

    protected void Start()
    {
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
        if (collision.collider.CompareTag("Projectile"))
        {
            Health--;

            if (Health <= 0)
                StateMachine.ChangeState(StaticEnemyStates.Dead);
        }
    }
}
