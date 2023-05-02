using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicPan : MonoBehaviour
{
    public GameObject lookAtTarget;
    public float panSpeed = 1.0f;

    void Update()
    {
        transform.LookAt(lookAtTarget.transform.position);
    }
}
