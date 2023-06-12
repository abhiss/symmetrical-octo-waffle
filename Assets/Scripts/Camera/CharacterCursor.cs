using UnityEngine;

public class CharacterCursor : MonoBehaviour
{
    public Transform Player;
    private RectTransform _rectTransform;

    private void Start()
    {
        Cursor.visible = false;
        _rectTransform = GetComponent<RectTransform>();
    }

    private void LookAt2D(Vector3 lookAtPosition)
    {
        Vector3 dir = lookAtPosition - transform.position;
        float angle = Mathf.Atan2(dir.y,dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void Update()
    {
        transform.position = Input.mousePosition;
        if (Player == null)
            return;

        Vector3 playerPos2D = Camera.main.WorldToScreenPoint(Player.position);
        LookAt2D(playerPos2D);
    }
}
