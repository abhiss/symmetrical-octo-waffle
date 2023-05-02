using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicPan : MonoBehaviour
{
    public GameObject lookAtTarget;
    public float panSpeed = 1.0f;
    private float bounce = 0.0f;

    void Update()
    {
        //bounce = Mathf.MoveTowards(bounce, Mathf.Sin(Time.time), Time.deltaTime * panSpeed);
        transform.LookAt(lookAtTarget.transform.position);
        //transform.RotateAround(lookAtTarget.transform.position, new Vector3(bounce, 0, 0), Time.deltaTime * panSpeed);
    }
}
