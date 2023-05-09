using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : State
{
    // Start is called before the first frame update
    //[SerializeField] public UnityEngine.AI.NavMeshAgent navAgent;


    private List<GameObject> player = new List<GameObject>();
    //private GameObject self;
    private Vector3 previousPosition;
    [SerializeField] private float detectionDistance;
    [SerializeField] private float attackRange;
    [SerializeField] private State attack;


    void Start()
    {
        //player = GameObject.Find("TopDownCharacter");

    }

    public override string GetStateAsString()
    {
        return "Chase";
    }

    // Update is called once per frame
    public override State StateFunction(UnityEngine.AI.NavMeshAgent navAgent)
    {
        if(detectPlayer())
        {
            float distance = Vector3.Distance(transform.position, player[0].transform.position);
            if( distance <= 1 + attackRange) //player radius + enemy radius + attack range
            {
                navAgent.SetDestination(previousPosition);
                return attack;
            }

            previousPosition = this.transform.position;
            navAgent.SetDestination(player[0].transform.position);
            return this;
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
}
