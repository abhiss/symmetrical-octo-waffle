using UnityEngine;

public class DisplayResolution : MonoBehaviour
{
    void Update()
    {
        Debug.Log("Screen Resolution: " + Screen.width + "x" + Screen.height);
    }
}
