using System.Collections;
using UnityEngine;

public class MeleeEnemyInFight : BaseState
{
    EnemyBase enemy;
    GameObject playerRef;
    float elapsedTimeBetweenShots = 0.0f;

    Vector3 DistanceToPlayer
    {
        get => new Vector3(playerRef.transform.position.x - enemy.transform.position.x,
                            playerRef.transform.position.y - enemy.transform.position.y);
    }

    public MeleeEnemyInFight(EnemyBase e)
    {
        enemy = e;
    }

    public override void onInit(params object[] args)
    {
        if (args.Length > 0)
        {
            playerRef = (GameObject)args[0];
        }

        elapsedTimeBetweenShots = enemy.TimeBetweenShots + Mathf.Epsilon;
    }

    public override void onUpdate(float deltaTime)
    {
        if (elapsedTimeBetweenShots >= enemy.TimeBetweenShots)
        {
            enemy.StartCoroutine(Shoot());
            elapsedTimeBetweenShots = 0.0f;
        }
        else
        {
            elapsedTimeBetweenShots += deltaTime;
        }
    }

    public override void onFixedUpdate(float deltaTime)
    {
        if (DistanceToPlayer.magnitude > enemy.Range)
        {
            enemy.Logic.MoveTowards(playerRef.transform.position, enemy.Speed * deltaTime);
        }
        else if (DistanceToPlayer.magnitude < enemy.SafeDistance)
        {
            var dir = playerRef.transform.position.x > enemy.transform.position.x ? -1.0f : 1.0f;
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
        StateMachine.ChangeState(EnemyStates.Patroling);
    }
}
