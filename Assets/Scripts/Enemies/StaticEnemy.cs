using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticEnemyIdle : BaseState
{
    EnemyBase enemy;

    public StaticEnemyIdle(EnemyBase e)
    {
        enemy = e;
    }

    public override void onUpdate(float deltaTime)
    {
        if (enemy.Logic.IsInRange && enemy.Logic.HasClearShot)
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
    EnemyBase enemy;
    private float timeElapsed = 0.0f;

    bool IsPlayerInRange { get => enemy.Logic.IsInRange; }

    public StaticEnemyShooting(EnemyBase e)
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
            if (IsPlayerInRange)
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
        projectile.Dir = enemy.transform.right * enemy.FacingDirection;
    }
}

public class StaticEnemy : EnemyBase
{
    private void Start()
    {
        base.Start();

        StateMachine.AddState(EnemyStates.Idle, new StaticEnemyIdle(this));
        StateMachine.AddState(EnemyStates.Shooting, new StaticEnemyShooting(this));
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
        SpriteRenderer.flipX = facingDir >= 0.0f;
    }
}
