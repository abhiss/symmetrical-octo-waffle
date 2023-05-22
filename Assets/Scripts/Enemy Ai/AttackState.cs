using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//

public class AttackState : State
{
    private List<GameObject> player = new List<GameObject>();
    private Vector3 previousPosition;
    [SerializeField] private float detectionDistance;
    [SerializeField] private float attackRange;
    [SerializeField] private State chase;


    void Start()
    {
        //player = GameObject.Find("TopDownCharacter");
    }

    // UnityEngine.AI.NavMeshAgent navAgent
    public override State StateFunction(UnityEngine.AI.NavMeshAgent navAgent)
    {
        if (detectPlayer())
        {
            float distance = Vector3.Distance(transform.position, player[0].transform.position);
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

    public void rotate( )
    {
        Vector3 direction = new Vector3(player[0].transform.position.x, this.transform.parent.position.y, player[0].transform.position.z);
        this.transform.parent.LookAt(direction);
    }
}
