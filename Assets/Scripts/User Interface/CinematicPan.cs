using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicPan : MonoBehaviour
{
    [Header("Properties")]
    public float panSpeed = 1.0f;
    public GameObject[] lookObjects;
    private Vector3 currentPoint;
    private int index = 0;

    void Awake()
    {
        // Look at main menu
        currentPoint = lookObjects[index].transform.position;
    }

    void LateUpdate()
    {
        SmoothLookAt();
    }

    private void SmoothLookAt()
    {
        Vector3 lookDirection = currentPoint - transform.position;
        lookDirection.Normalize();
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lookDirection), panSpeed * Time.deltaTime);
    }

    public int GetIndex()
    {
        return index;
    }

    public void MoveToPoint(int pointIndex)
    {
        if (pointIndex >= 0 && pointIndex < lookObjects.Length)
        {
            index = pointIndex;
            currentPoint = lookObjects[index].transform.position;
        }
    }
}
