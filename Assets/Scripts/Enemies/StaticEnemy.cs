using UnityEngine;

public class StaticEnemyIdle : BaseState
{
    StaticEnemy enemy;

    public StaticEnemyIdle(StaticEnemy e)
    {
        enemy = e;
    }

    public override void onInit(params object[] args)
    {
        base.onInit(args);
        //enemy.Animator.Play("AnimRED_Idle");
    }

    public override void onUpdate(float deltaTime)
    {
        if (enemy.Logic.IsInRange)
        {
            StartShooting();
        }
    }

    private void StartShooting()
    {
        enemy.StateMachine.ChangeState(EnemyStates.Shooting);
    }
}

public class StaticEnemyShooting : BaseState
{
    StaticEnemy enemy;
    private float timeElapsed = 0.0f;

    bool IsPlayerInRange { get => enemy.Logic.IsInRange; }

    public StaticEnemyShooting(StaticEnemy e)
    {
        enemy = e;
    }

    public override void onInit(params object[] args)
    {
        timeElapsed = 100.0f;
        //enemy.Animator.Play("AnimRED_Attack");
    }

    public override void onUpdate(float deltaTime)
    {
        if (timeElapsed >= enemy.TimeBetweenShots)
        {
            if (IsPlayerInRange)
            {
                timeElapsed = 0.0f;
                //enemy.Animator.Play("AnimRED_Attack");
                enemy.Shoot();
            }
            else
            {
                enemy.StateMachine.ChangeState(EnemyStates.Idle);
            }
        }

        timeElapsed += deltaTime;
    }
}

public class StaticEnemyDamaged : BaseState
{
    StaticEnemy enemy;

    public StaticEnemyDamaged(StaticEnemy e)
    {
        enemy = e;
    }

    public override void onInit(params object[] args)
    {
        //enemy.Animator.Play("AnimRED_Damage");
        enemy.Health -= 1;

        if (enemy.Health <= 0)
        {
            enemy.StateMachine.ChangeState(EnemyStates.Dead);
        }
    }
}


public class StaticEnemyDead : BaseState
{
    StaticEnemy enemy;

    public StaticEnemyDead(StaticEnemy e)
    {
        enemy = e;
    }

    public override void onInit(params object[] args)
    {
        //enemy.Animator.Play("AnimRED_Crush");
    }
}


public class StaticEnemy : EnemyBase
{
    [SerializeField]
    private Transform projectileSpawnPoint = null;
    public Vector3 ProjectileSpawnPosition { get => projectileSpawnPoint.position; }

    public Animator Animator { get; private set; } = null;

    [SerializeField]
    private AudioSource _audio = null;

    [SerializeField]
    private AudioClip[] _shotSounds = null;

    private int _currentShot = 0;

    private void Start()
    {
        Animator = GetComponent<Animator>();
        StateMachine.AddState(EnemyStates.Idle, new StaticEnemyIdle(this));
        StateMachine.AddState(EnemyStates.Shooting, new StaticEnemyShooting(this));
        StateMachine.AddState(EnemyStates.Damaged, new StaticEnemyDamaged(this));
        StateMachine.AddState(EnemyStates.Dead, new StaticEnemyDead(this));

        base.Start();

        StateMachine.ChangeState(EnemyStates.Idle);
    }

    private void Update()
    {
        base.Update();
        SetDirection();
    }

    private void SetDirection()
    {
        var facingDir = IsEnemyOnLeft ? -1.0f : 1.0f;
        FacingDirection = facingDir;
        // SpriteRenderer.flipX = facingDir >= 0.0f;
    }

    public void Shoot()
    {
        var projectile = Object.Instantiate(Projectile);
        projectile.transform.position = ProjectileSpawnPosition;
        projectile.Dir = projectileSpawnPoint.forward;

        Animator.Play("Shoot");

        _audio.PlayOneShot(_shotSounds[_currentShot % 2]);
        ++_currentShot;
    }

    private void SetIdle()
    {
        //Animator.Play("AnimRED_Idle");
    }

    private void OnDamageEnd()
    {
        StateMachine.ChangeState(EnemyStates.Idle);
    }

    protected override void ReceivedDamage(float dir)
    {
        //var _currState = StateMachine.CurrentState;
        //if (_currState != EnemyStates.Damaged && _currState != EnemyStates.Dead)
        //{
        //    StateMachine.ChangeState(EnemyStates.Damaged, dir);
        //}
    }
}
