using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticEnemyIdle : BaseState
{
    EnemyBase enemy;

    bool IsEnemyOnLeft { get => enemy.Logic.IsPlayerOnLeft; }
    bool IsEnemyOnRight { get => !enemy.Logic.IsPlayerOnLeft; }

    public StaticEnemyIdle(EnemyBase e)
    {
        enemy = e;
    }

    public override void onUpdate(float deltaTime)
    {
        if (enemy.Logic.HasClearShot)
        {
            if (IsEnemyOnRight)
            {
                StartShooting(1.0f);
            }
            else if (IsEnemyOnLeft)
            {
                StartShooting(-1.0f);
            }
        }
    }

    private void StartShooting(float facingDir)
    {
        enemy.FacingDirection = facingDir;
        enemy.StateMachine.ChangeState(EnemyStates.Shooting);
        enemy.SpriteRenderer.flipX = facingDir >= 0.0f;

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
}
