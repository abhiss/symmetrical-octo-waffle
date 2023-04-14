using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pleasework : MonoBehaviour
{
    public float speed = 1;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Vertical") * speed;
        float y = Input.GetAxis("Horizontal") * speed;
        transform.position += new Vector3(-y, 0, -x);
    }
}
