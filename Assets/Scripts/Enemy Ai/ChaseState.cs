using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : State
{
    // Start is called before the first frame update
    //[SerializeField] public UnityEngine.AI.NavMeshAgent navAgent;


    private GameObject player;
    //private GameObject self;
    private Vector3 previousPosition;
    [SerializeField] private float attackRange;
    [SerializeField] private State attack;


    void Start()
    {
        player = GameObject.Find("TopDownCharacter");
    
    }

    // Update is called once per frame
    public override State StateFunction(UnityEngine.AI.NavMeshAgent navAgent)
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        if( distance <= 1 + attackRange) //player radius + enemy radius + attack range
        {
            navAgent.SetDestination(previousPosition);
            return attack;
        }
        
        previousPosition = this.transform.position;
        navAgent.SetDestination(player.transform.position);
        return this;
    }
}
