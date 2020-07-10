using System.Collections;
using UnityEngine;

public class MeleeEnemyInFight : BaseState
{
    EnemyBase enemy;

    float DistanceToPlayer
    {
        get => Vector3.Distance(enemy.transform.position, enemy.PlayerRef.transform.position);
    }

    public MeleeEnemyInFight(EnemyBase e)
    {
        enemy = e;
    }

    public override void onInit(params object[] args)
    {
    }

    public override void onUpdate(float deltaTime)
    {
        if(enemy.PlayerRef.IsAttacking && DistanceToPlayer < enemy.SafeDistance)
        {
            enemy.StateMachine.ChangeState(EnemyStates.Dashing, enemy.Logic.IsPlayerOnLeft ? -1.0f : 1.0f);
        }
    }

    public override void onFixedUpdate(float deltaTime)
    {
        enemy.Logic.MoveTowards(enemy.PlayerRef.transform.position, enemy.Speed * deltaTime);
        
        if (DistanceToPlayer < enemy.SafeDistance)
        {
            var dir = enemy.PlayerRef.transform.position.x > enemy.transform.position.x ? -1.0f : 1.0f;
            enemy.transform.position += enemy.transform.right * dir * enemy.Speed * deltaTime;

            RaycastHit hit;
            if (Physics.Raycast(enemy.transform.position, enemy.transform.right * dir, out hit, 0.1f, LayerMask.GetMask("Ground")))
            {
                enemy.StateMachine.ChangeState(EnemyStates.Dashing, -dir);
            }
        }
    }
}

public class MeleeEnemy : EnemyBase
{
    // Start is called before the first frame update
    void Start()
    {
        base.Start();

        StateMachine.AddState(EnemyStates.Patroling, new EnemyPatroling(this));
        StateMachine.AddState(EnemyStates.Dashing, new EnemyDashing(this));
        StateMachine.AddState(EnemyStates.InFight, new MeleeEnemyInFight(this));
        StateMachine.ChangeState(EnemyStates.Patroling);
    }
}
