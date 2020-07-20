using System.Collections;
using System.Linq;
using UnityEngine;

public class MeleeEnemyInFight : BaseState
{
    MeleeEnemy enemy;

    float DistanceToPlayer
    {
        get => Vector3.Distance(enemy.transform.position, enemy.PlayerRef.transform.position);
    }

    public MeleeEnemyInFight(MeleeEnemy e)
    {
        enemy = e;
    }

    public override void onInit(params object[] args)
    {
        enemy.PlayerRef.OnAttackStartNotify.RemoveListener(Dodge);
        enemy.PlayerRef.OnAttackStartNotify.AddListener(Dodge);
    }

    public override void onExit()
    {
        enemy.PlayerRef.OnAttackStartNotify.RemoveListener(Dodge);
    }

    public override void onUpdate(float deltaTime)
    {
        if(DistanceToPlayer <= enemy.SafeDistance * 1.1)
        {
            enemy.StateMachine.ChangeState(EnemyStates.Attacking);
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

    private void Dodge()
    {
        if (DistanceToPlayer < enemy.SafeDistance)
        {
            enemy.StateMachine.ChangeState(EnemyStates.Dashing, 3 * (enemy.Logic.IsPlayerOnLeft ? -1.0f : 1.0f));
        }
    }
}

public class MeleeEnemyAttack : BaseState
{
    MeleeEnemy enemy;
    float duration = 0.0f;
    float elapsed = 0.0f;

    public MeleeEnemyAttack(MeleeEnemy e)
    {
        enemy = e;
        duration = enemy.Animator.runtimeAnimatorController.animationClips.First(a => a.name == "Attack").length;
    }

    public override void onInit(params object[] args)
    {
        enemy.Animator.Play("Attack");
        elapsed = 0.0f;
    }

    public override void onUpdate(float deltaTime)
    {
        if(elapsed > duration)
        {
            enemy.StateMachine.ChangeState(EnemyStates.InFight);
        }

        elapsed += deltaTime;
    }

    public override void onFixedUpdate(float deltaTime)
    {
    }
}

//public class MeleeEnemyInFight : BaseState
//{
//    EnemyBase enemy;

//    float DistanceToPlayer
//    {
//        get => Vector3.Distance(enemy.transform.position, enemy.PlayerRef.transform.position);
//    }

//    public MeleeEnemyInFight(EnemyBase e)
//    {
//        enemy = e;
//    }

//    public override void onInit(params object[] args)
//    {
//    }

//    public override void onUpdate(float deltaTime)
//    {
//        if (enemy.PlayerRef.IsAttacking && DistanceToPlayer < enemy.SafeDistance)
//        {
//            enemy.StateMachine.ChangeState(EnemyStates.Dashing, enemy.Logic.IsPlayerOnLeft ? -1.0f : 1.0f);
//        }
//    }

//    public override void onFixedUpdate(float deltaTime)
//    {
//        enemy.Logic.MoveTowards(enemy.PlayerRef.transform.position, enemy.Speed * deltaTime);

//        if (DistanceToPlayer < enemy.SafeDistance)
//        {
//            var dir = enemy.PlayerRef.transform.position.x > enemy.transform.position.x ? -1.0f : 1.0f;
//            enemy.transform.position += enemy.transform.right * dir * enemy.Speed * deltaTime;

//            RaycastHit hit;
//            if (Physics.Raycast(enemy.transform.position, enemy.transform.right * dir, out hit, 0.1f, LayerMask.GetMask("Ground")))
//            {
//                enemy.StateMachine.ChangeState(EnemyStates.Dashing, -dir);
//            }
//        }
//    }
//}

public class MeleeEnemy : EnemyBase
{
    public Animator Animator { get; private set; } = null;
    public float AttackCooldown { get; private set; } = 2.0f;

    void Start()
    {
        base.Start();

        Animator = GetComponentInChildren<Animator>();

        StateMachine.AddState(EnemyStates.Patroling, new EnemyPatroling(this));
        StateMachine.AddState(EnemyStates.Dashing, new EnemyDashing(this));
        StateMachine.AddState(EnemyStates.InFight, new MeleeEnemyInFight(this));
        StateMachine.AddState(EnemyStates.Attacking, new MeleeEnemyAttack(this));
        StateMachine.ChangeState(EnemyStates.Patroling);
    }

    public void OnAttackEnd()
    {
        StateMachine.ChangeState(EnemyStates.InFight);
    }
}
