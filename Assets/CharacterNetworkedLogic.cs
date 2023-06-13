using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterNetworkedLogic : NetworkBehaviour
{
    // Start is called before the first frame update
    public GameObject playercanvas;

    void Start()
    {
        if (!IsOwner) Destroy(playercanvas);
    }

    // // Update is called once per frame
    // void Update()
    // {
        
    // }
}
