using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IdleState : State
{
    // Start is called before the first frame update
    private GameObject player;
    private Vector3 wonderPosition;
    [SerializeField] private float detectionDistance;
    [SerializeField] private State chase;
    [SerializeField] private float wonderingRadius;
    //[SerializeField] private State attack;

    void Start()
    {
        player = GameObject.Find("TopDownCharacter");
        wonderPosition = this.transform.position;
    }


    public override State StateFunction(UnityEngine.AI.NavMeshAgent agent)
    {
        if (Vector3.Distance(player.transform.position, this.transform.position) <= detectionDistance ){
            return chase;
        }
        else
        {
           if(agent.remainingDistance <= agent.stoppingDistance) 
            {
                Vector3 point;
                if (RandomPoint(this.transform.position, wonderingRadius, out point))
                {
                    agent.SetDestination(point);
                }
            }
        }
        return this;
    }

    
    private bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        Vector3 randDirection = Random.insideUnitSphere * wonderingRadius;
        NavMeshHit hit;
        randDirection += this.transform.position;
        if( NavMesh.SamplePosition( randDirection, out hit, wonderingRadius, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }
        
        result = Vector3.zero;
        return false;

    }
}
