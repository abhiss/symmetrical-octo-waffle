using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//

public class AttackState : State
{
    private GameObject player;
    private Vector3 previousPosition;
    [SerializeField] private float attackRange;
    [SerializeField] private State chase;


    void Start()
    {
        player = GameObject.Find("TopDownCharacter");
    
    }

    // UnityEngine.AI.NavMeshAgent navAgent
    public override State StateFunction(UnityEngine.AI.NavMeshAgent navAgent)
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        if( distance <= 1 + attackRange) //player radius + enemy radius + attack range
        {
            //turn towards player
            //Do attack stuff
            //turn towards player
            navAgent.updateRotation = false;
            rotate();
            return this;
        }
        navAgent.updateRotation = true; //turn rotation back on
        return chase;
    }

    public void rotate( )
    {
        
        Vector3 direction = (player.transform.position - this.transform.parent.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        this.transform.parent.rotation = Quaternion.Slerp(this.transform.rotation, lookRotation, Time.deltaTime * 1000);
        Debug.Log(this.transform.parent.rotation);
    }
}
