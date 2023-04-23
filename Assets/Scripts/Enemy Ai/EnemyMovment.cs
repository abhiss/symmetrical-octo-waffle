using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class EnemyMovment : MonoBehaviour
{
    private NavMeshAgent navAgent;
    private GameObject player;

    // Start is called before the first frame update
    private void Start()
    {
        player = GameObject.Find("TopDownCharacter");
        navAgent = GetComponent<NavMeshAgent>(); 
        
    }

    // Update is called once per frame
    private void Update()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        // this makes enemy kind of lag behind before update to player's position, so you can juke them
        if(distance > 2){
            navAgent.SetDestination(player.transform.position);
        }
        
    }
}
