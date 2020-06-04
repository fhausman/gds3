using System.Linq;
using UnityEngine;

public class DoubleBlastEnemyChasing : BaseState
{
    EnemyBase enemy;
    GameObject playerRef;

    Vector3 DistanceToPlayer 
    {
        get => new Vector3(playerRef.transform.position.x - enemy.transform.position.x,
                            playerRef.transform.position.y - enemy.transform.position.y);
    }

    public DoubleBlastEnemyChasing(EnemyBase e)
    {
        enemy = e;
    }

    public override void onInit(params object[] args)
    {
        playerRef = (GameObject) args[0];
    }

    public override void onUpdate(float deltaTime)
    {
    }

    public override void onFixedUpdate(float deltaTime)
    {
        if(DistanceToPlayer.magnitude > enemy.Range)
        {
            enemy.Logic.MoveTowards(playerRef.transform.position, enemy.Speed * deltaTime);
        }
        else if(DistanceToPlayer.magnitude < enemy.SafeDistance)
        {
            var dir = playerRef.transform.position.x > enemy.transform.position.x ? -1.0f : 1.0f;
            enemy.transform.position += enemy.transform.right * dir * enemy.Speed * deltaTime;
        }
    }
}

public class DoubleBlastEnemy : EnemyBase
{
    private void Start()
    {
        base.Start();

        StateMachine.AddState(EnemyStates.Patroling, new EnemyPatroling(this));
        StateMachine.AddState(EnemyStates.Chasing, new DoubleBlastEnemyChasing(this));
        StateMachine.ChangeState(EnemyStates.Patroling);
    }
}
