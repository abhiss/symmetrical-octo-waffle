using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    
    [SerializeField] private State CurrentState;
    private UnityEngine.AI.NavMeshAgent navAgent;
    
    void Start(){
        navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
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
