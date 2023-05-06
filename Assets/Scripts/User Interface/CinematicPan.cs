using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicPan : MonoBehaviour
{
    [Header("Properties")]
    public float panSpeed = 1.0f;
    public GameObject[] lookObjects;
    private Vector3 _currentPoint;
    private int index = 0;
    // TEMP
    private AudioSource _audioSource;
    void Awake()
    {
        // Look at main menu
        _currentPoint = lookObjects[index].transform.position;
        _audioSource = GetComponent<AudioSource>();
    }

    void LateUpdate()
    {
        // TOOD: TEMP
        if (Input.GetButtonDown("Fire1"))
        {
            _audioSource.Play();
            index++;
            if (index >= lookObjects.Length)
            {
                index = 0;
            }
            _currentPoint = lookObjects[index].transform.position;
        }
        // END OF TEMP
        SmoothLookAt();
    }

    private void SmoothLookAt()
    {
        Vector3 lookDirection = _currentPoint - transform.position;
        lookDirection.Normalize();
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lookDirection), panSpeed * Time.deltaTime);
    }
}
