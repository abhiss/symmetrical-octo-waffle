using UnityEngine;
using Unity.Netcode;
public class RemoveDupCursor : NetworkBehaviour
{
    private void Start()
    {
        if (!IsOwner) Destroy(gameObject);
    }
}
