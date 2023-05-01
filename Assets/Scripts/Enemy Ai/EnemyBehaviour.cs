using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class EnemyBehavior : MonoBehaviour
{
    private NavMeshAgent navAgent;
    private GameObject player;
    private Vector3 previousPosition;
    [SerializeField] private float detectionRange;
    [SerializeField] private float attackRange;

    // Start is called before the first frame update
    private void Awake()
    {
        player = GameObject.Find("TopDownCharacter");
        navAgent = GetComponent<NavMeshAgent>(); 
    }

    // Update is called once per frame
    private void Update()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        if( distance <= 1 + attackRange) //player radius + enemy radius + attack range
        {
            navAgent.SetDestination(previousPosition);
            return;
        }
        previousPosition = this.transform.position;
        navAgent.SetDestination(player.transform.position);
        
    }
}
