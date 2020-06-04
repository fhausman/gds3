using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticEnemyShooting : BaseState
{
    EnemyBase enemy;
    private float timeElapsed = 0.0f;

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

public class StaticEnemy : EnemyBase
{
    private void Start()
    {
        base.Start();

        StateMachine.AddState(EnemyStates.Idle, new EnemyIdle(this));
        StateMachine.AddState(EnemyStates.Shooting, new StaticEnemyShooting(this));
        StateMachine.AddState(EnemyStates.Dead, new EnemyDead(this));
        StateMachine.ChangeState(EnemyStates.Idle);
    }
}
