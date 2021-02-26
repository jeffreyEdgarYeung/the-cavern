using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player Status")]
    [SerializeField] [Range(0, 50)] float maxSpeed = 10f;

    Rigidbody2D rigidBody;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        HandleSpriteDirection();
    }
    private void Move()
    {
        Run();
    }

    private void Run()
    {
        var deltaX = Input.GetAxis("Horizontal") * maxSpeed;
        rigidBody.velocity = new Vector2(deltaX, rigidBody.velocity.y);
        
    }

    private void HandleSpriteDirection()
    {
        bool running = Mathf.Abs(rigidBody.velocity.x) > Mathf.Epsilon;

        if (running)
        {
            GetComponent<Animator>().SetBool("isRunning", true);
            Vector3 newScale = transform.localScale;
            newScale.x = Mathf.Sign(rigidBody.velocity.x);
            transform.localScale = newScale;
        }
        else
        {
            GetComponent<Animator>().SetBool("isRunning", false);
        }
    }
}
