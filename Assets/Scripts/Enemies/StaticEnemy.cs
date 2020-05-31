using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyStates
{
    Idle,
    Shooting,
    Dead
}

public class EnemyIdle : BaseState
{
    EnemyBase enemy;

    public EnemyIdle(EnemyBase e)
    {
        enemy = e;
    }

    public override void onUpdate(float deltaTime)
    {
        RaycastHit hit;
        if (Physics.Raycast(enemy.transform.position, enemy.transform.right, out hit, enemy.Range, LayerMask.GetMask("Player")))
        {
            StartShooting(1.0f);
        }
        else if (Physics.Raycast(enemy.transform.position, -enemy.transform.right, out hit, enemy.Range, LayerMask.GetMask("Player")))
        {
            StartShooting(-1.0f);
        }
    }

    private void StartShooting(float facingDir)
    {
        enemy.FacingDirection = facingDir;
        enemy.StateMachine.ChangeState(EnemyStates.Shooting);
        enemy.SpriteRenderer.flipX = facingDir >= 0.0f;
        
    }
}

public class EnemyShooting : BaseState
{
    EnemyBase enemy;
    private float timeElapsed = 0.0f;

    public EnemyShooting(EnemyBase e)
    {
        enemy = e;
    }

    public override void onInit(params object[] args)
    {
        timeElapsed = 0.0f;
        Shoot();
    }

    public override void onUpdate(float deltaTime)
    {
        if (timeElapsed >= enemy.TimeBetweenShots)
        {
            RaycastHit hit;
            if (Physics.Raycast(enemy.transform.position, enemy.FacingDirection * enemy.transform.right, out hit, enemy.Range, LayerMask.GetMask("Player")))
            {
                Shoot();
                timeElapsed = 0.0f;
            }
            else
            {
                enemy.StateMachine.ChangeState(EnemyStates.Idle);
            }
        }

        timeElapsed += deltaTime;
    }

    private void Shoot()
    {
        var projectile = Object.Instantiate(enemy.Projectile);
        projectile.transform.position = enemy.transform.position;
        projectile.Dir = enemy.transform.right * enemy.FacingDirection + enemy.transform.up;
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

public class EnemyBase : MonoBehaviour
{
    #region Settings
    [SerializeField]
    private EnemySettings settings;
    public float ShootRate { get => settings.shootRate; }
    public float Range { get => settings.range; }
    #endregion

    [SerializeField]
    private Projectile projectile;
    public Projectile Projectile { get => projectile; }

    [SerializeField]
    private SpriteRenderer spriteRenderer;
    public SpriteRenderer SpriteRenderer { get => spriteRenderer; }

    public StateMachine<EnemyStates> StateMachine { get; private set; } = new StateMachine<EnemyStates>();
    public float FacingDirection { get; set; } = -1.0f;
    public float TimeBetweenShots { get => 1 / ShootRate; }
    public int Health { get; set; } = 0;

    protected void Start()
    {
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
        if(collision.collider.CompareTag("Projectile"))
        {
            Health--;

            if (Health <= 0)
                StateMachine.ChangeState(EnemyStates.Dead);
        }
    }
}

public class StaticEnemy : EnemyBase
{
    private void Start()
    {
        base.Start();

        StateMachine.AddState(EnemyStates.Idle, new EnemyIdle(this));
        StateMachine.AddState(EnemyStates.Shooting, new EnemyShooting(this));
        StateMachine.AddState(EnemyStates.Dead, new EnemyDead(this));
        StateMachine.ChangeState(EnemyStates.Idle);
    }
}
