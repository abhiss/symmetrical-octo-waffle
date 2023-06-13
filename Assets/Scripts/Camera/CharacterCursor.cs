using Unity.Netcode;
using UnityEngine;

public class CharacterCursor : NetworkBehaviour
{
    public Transform Player;

    private void Start()
    {
        Cursor.visible = false;
    }

    private void LookAt2D(Vector3 lookAtPosition)
    {
        Vector3 dir = lookAtPosition - transform.position;
        float angle = Mathf.Atan2(dir.y,dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void Update()
    {
        if (Player == null)
            return;

        transform.position = Input.mousePosition;
        Vector3 playerPos2D = Camera.main.WorldToScreenPoint(Player.position);
        LookAt2D(playerPos2D);
    }
}
