using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

public class DoubleBlastEnemyInFight : BaseState
{
    EnemyBase enemy;
    GameObject playerRef;
    float elapsedTimeBetweenShots = 0.0f;

    Vector3 DistanceToPlayer 
    {
        get => new Vector3(playerRef.transform.position.x - enemy.transform.position.x,
                            playerRef.transform.position.y - enemy.transform.position.y);
    }

    public DoubleBlastEnemyInFight(EnemyBase e)
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
        if(elapsedTimeBetweenShots >= enemy.TimeBetweenShots)
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
        if(DistanceToPlayer.magnitude > enemy.Range)
        {
            enemy.Logic.MoveTowards(playerRef.transform.position, enemy.Speed * deltaTime);
        }
        else if(DistanceToPlayer.magnitude < enemy.SafeDistance)
        {
            var dir = playerRef.transform.position.x > enemy.transform.position.x ? -1.0f : 1.0f;
            enemy.transform.position += enemy.transform.right * dir * enemy.Speed * deltaTime;

            RaycastHit hit;
            if(Physics.Raycast(enemy.transform.position, enemy.transform.right * dir, out hit, 0.1f, LayerMask.GetMask("Ground")))
            {
                enemy.StateMachine.ChangeState(EnemyStates.Dashing, -dir);
            }
        }
    }

    IEnumerator Shoot()
    {
        CreateProjectile(5f);

        yield return new WaitForSeconds(0.5f);

        CreateProjectile(-5f);
    }

    void CreateProjectile(float offset)
    {
        var proj = Object.Instantiate(enemy.Projectile);
        proj.transform.position = enemy.transform.position;
        proj.Dir = Quaternion.Euler(0.0f, 0.0f, offset) * DistanceToPlayer.normalized;
    }
}

public class DoubleBlastEnemy : EnemyBase
{
    private void Start()
    {
        base.Start();

        StateMachine.AddState(EnemyStates.Patroling, new EnemyPatroling(this));
        StateMachine.AddState(EnemyStates.Dashing, new EnemyDashing(this));
        StateMachine.AddState(EnemyStates.InFight, new DoubleBlastEnemyInFight(this));
        StateMachine.ChangeState(EnemyStates.Patroling);
    }
}
