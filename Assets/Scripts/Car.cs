using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class Car : MonoBehaviour
{
    // Start is called before the first frame update
    Vector2 rawInput;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float paddingLeft = 5f;
    [SerializeField] float paddingRight = 5f;
    [SerializeField] float paddingTop = 5f;
    [SerializeField] float paddingBottom = 5f;

    Vector2 minBounds;
    Vector2 maxBounds;
    void Start()
    {
        InitBounds();
    }
    // Update is called once per frame
    void Update()
    {
        Move();
    }

    void InitBounds()
    {
        Camera mainCamera = Camera.main;
        minBounds = mainCamera.ViewportToWorldPoint(new Vector2(0,0f));
        maxBounds = mainCamera.ViewportToWorldPoint(new Vector2(1,1f));
    }
    void Move()
    {
        Vector2 delta = rawInput * moveSpeed * Time.deltaTime;
        Vector2 newPos = new Vector2();
        newPos.x = Mathf.Clamp(transform.position.x + delta.x, minBounds.x+paddingLeft, maxBounds.x-paddingRight);
        newPos.y = Mathf.Clamp(transform.position.y + delta.y, minBounds.y+paddingBottom, maxBounds.y-paddingTop);
        transform.position = newPos;
    }
    void OnMove(InputValue value)
    {
        rawInput = value.Get<Vector2>();
        Debug.Log(rawInput);
    }
}
