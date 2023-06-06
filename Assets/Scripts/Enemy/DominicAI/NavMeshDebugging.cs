using UnityEngine;
using UnityEngine.AI;

public class NavMeshDebugging : MonoBehaviour
{
    [Header("Properties")]
    public bool showVelocity;
    public bool showDesiredVelocity;
    public bool showPath;
    private NavMeshAgent _agent;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    void OnDrawGizmos()
    {
        if(!_agent)
        {
            return;
        }

        if (showVelocity)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + _agent.velocity);
        }

        if (showDesiredVelocity)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + _agent.desiredVelocity);
        }

        if (showPath)
        {
            Gizmos.color = Color.black;
            NavMeshPath agentPath = _agent.path;
            Vector3 prevCorner = transform.position;
            foreach (Vector3 corner in agentPath.corners)
            {
                Gizmos.DrawLine(prevCorner, corner);
                Gizmos.DrawSphere(corner, 0.1f);
                prevCorner = corner;
            }
        }
    }
}
