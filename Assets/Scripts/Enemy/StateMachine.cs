using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class StateMachine : MonoBehaviour
{

    [SerializeField] private State CurrentState;
    private NavMeshAgent navAgent;

    void Start(){
        navAgent = GetComponent<NavMeshAgent>();
    }
    // Update is called once per frame
    void Update()
    {
        RunStateMachine();
    }

    private void RunStateMachine()
    {
        State NextState = CurrentState?.StateFunction(navAgent);
        if( NextState != null)
        {
            CurrentState = NextState;
        }
    }

}
