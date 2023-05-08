using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IdleState : State
{
    // Start is called before the first frame update
    //private GameObject player;
    private List<GameObject> player = new List<GameObject>();
    private Vector3 wonderPosition;
    [SerializeField] private float detectionDistance;
    [SerializeField] private State chase;
    [SerializeField] private float wonderingRadius;
    //[SerializeField] private State attack;

    void Start()
    {
        //player = GameObject.Find("TopDownCharacter");
        wonderPosition = this.transform.position;
    }


    public override State StateFunction(UnityEngine.AI.NavMeshAgent agent)
    {

        if (detectPlayer() && Vector3.Distance(player[0].transform.position, this.transform.position) <= detectionDistance ){
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

    public bool detectPlayer()
    {
        player.Clear();
        RaycastHit[] hit = new RaycastHit[4];
        LayerMask mask = LayerMask.GetMask("Player");
        int numHit = Physics.SphereCastNonAlloc(this.transform.position, detectionDistance, Vector3.up, hit, 0, mask, QueryTriggerInteraction.UseGlobal);
        for( int i = 0; i < numHit; i++){
            player.Add(hit[i].transform.gameObject);
        }
        if (numHit == 0)
        {
            return false;
        }
        return true;
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
