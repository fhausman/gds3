using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StaticEnemyStates
{
    Idle,
    Shooting,
    Dead
}

public class StaticEnemyIdle : BaseState
{
    EnemyBase enemy;

    bool IsEnemyOnLeft { get => enemy.Logic.LookForPlayer(-enemy.transform.right, enemy.Range); }
    bool IsEnemyOnRight { get => enemy.Logic.LookForPlayer(enemy.transform.right, enemy.Range); }

    public StaticEnemyIdle(EnemyBase e)
    {
        enemy = e;
    }

    public override void onUpdate(float deltaTime)
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

    private void StartShooting(float facingDir)
    {
        enemy.FacingDirection = facingDir;
        enemy.StateMachine.ChangeState(StaticEnemyStates.Shooting);
        enemy.SpriteRenderer.flipX = facingDir >= 0.0f;

    }
}

public class StaticEnemyShooting : BaseState
{
    EnemyBase enemy;
    private float timeElapsed = 0.0f;

    bool IsPlayerInRange { get => enemy.Logic.LookForPlayer(enemy.FacingDirection * enemy.transform.right, enemy.Range); }

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
                enemy.StateMachine.ChangeState(StaticEnemyStates.Idle);
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

public class StaticEnemy : EnemyBase
{
    private void Start()
    {
        base.Start();

        StateMachine.AddState(StaticEnemyStates.Idle, new StaticEnemyIdle(this));
        StateMachine.AddState(StaticEnemyStates.Shooting, new StaticEnemyShooting(this));
        StateMachine.AddState(StaticEnemyStates.Dead, new EnemyDead(this));
        StateMachine.ChangeState(StaticEnemyStates.Idle);
    }
}
