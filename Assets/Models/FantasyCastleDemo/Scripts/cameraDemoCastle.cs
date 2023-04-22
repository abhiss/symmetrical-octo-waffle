using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraDemoCastle : MonoBehaviour
{
    public float speed;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float H = Input.GetAxis("Horizontal");
        float V = Input.GetAxis("Vertical");

        float X = Input.GetAxis("Mouse X");

        transform.eulerAngles += Vector3.up * X * 100f * Time.deltaTime;

        transform.position += transform.TransformDirection(Vector3.ClampMagnitude(new Vector3(H, 0, V), 1f)) * speed * Time.deltaTime;
    }
}
