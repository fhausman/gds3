using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // Update is called once per frame
    void Update()
    {
        
    }
}
