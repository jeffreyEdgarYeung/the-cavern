using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player Parameters")]
    [SerializeField] [Range(0, 50)] float maxSpeed = 10f;
    [SerializeField] float knockbackForce;

    [Header("Jump Parameters")]
    [SerializeField] float jumpHeight = 1f;
    [SerializeField] Vector2 counterJumpForce;
    [SerializeField] private LayerMask platformLayerMask;
    [SerializeField] float jumpForce;

    [Header("Player State")]
    [SerializeField] bool isGrounded;
    [SerializeField] bool isJumping;
    [SerializeField] bool jumpKeyHeld;
    [SerializeField] bool isFalling;
    [SerializeField] bool isAgainstWall;

    [SerializeField] GameObject sword;
    [SerializeField] GameObject slash;

    Rigidbody2D rigidBody;
    CapsuleCollider2D capsuleCollider;
    Animator animator;

    string[] attackTriggers = new string[] { "attackTrigger1", "attackTrigger2" };

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        animator = GetComponent<Animator>();
        jumpForce = Mathf.Sqrt(2 * Physics2D.gravity.magnitude * jumpHeight);
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        HandleSpriteDirection();
        isGrounded = IsGrounded();
        isJumping = !isGrounded;
        isAgainstWall = IsAgainstWall();
    }

    void FixedUpdate()
    {
        HandlePartialJump();
    }

    private void Move()
    {
        Run();
        Jump();
        Fall();
        Attack();
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
            animator.SetBool("isRunning", true);
            Vector3 newScale = transform.localScale;
            newScale.x = Mathf.Sign(rigidBody.velocity.x);
            transform.localScale = newScale;
        }
        else
        {
            animator.SetBool("isRunning", false);
        }
    }

    private void Jump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            jumpKeyHeld = true;

            if (isGrounded)
            {
                isJumping = true;
                animator.SetTrigger("jumpTrigger");
                rigidBody.AddForce(Vector2.up * jumpForce * rigidBody.mass, ForceMode2D.Impulse);
            }
        }
        else if (Input.GetButtonUp("Jump"))
        {
            jumpKeyHeld = false;
        }
    }

    private void HandlePartialJump()
    {
        if (isJumping)
        {
            if(!jumpKeyHeld && Vector2.Dot(rigidBody.velocity, Vector2.up) > 0)
            {
                rigidBody.AddForce(counterJumpForce * rigidBody.mass);
            }
        }
    }

    private bool IsGrounded()
    {
        float extraHeightText = 0.1f;
        RaycastHit2D raycastHit = Physics2D.BoxCast(capsuleCollider.bounds.center, capsuleCollider.bounds.size, 0f, Vector2.down, extraHeightText, platformLayerMask);

        Color rayColor;
        if (raycastHit.collider != null)
        {
            rayColor = Color.green;
        }
        else
        {
            rayColor = Color.red;
        }
        Debug.DrawRay(capsuleCollider.bounds.center + new Vector3(capsuleCollider.bounds.extents.x, 0), Vector2.down * (capsuleCollider.bounds.extents.y + extraHeightText), rayColor);
        Debug.DrawRay(capsuleCollider.bounds.center - new Vector3(capsuleCollider.bounds.extents.x, 0), Vector2.down * (capsuleCollider.bounds.extents.y + extraHeightText), rayColor);
        Debug.DrawRay(capsuleCollider.bounds.center - new Vector3(capsuleCollider.bounds.extents.x, capsuleCollider.bounds.extents.y + extraHeightText), Vector2.right * (capsuleCollider.bounds.extents.x * 2f), rayColor);

        return raycastHit.collider != null;
    }

    private void Fall()
    {
        animator.SetBool("isFalling", rigidBody.velocity.y < -0.5f);
        animator.SetBool("isAgainstWall", isAgainstWall);
    }

    private bool IsAgainstWall()
    {
        float extraHeightText = 0.1f;
        RaycastHit2D raycastLeft = Physics2D.BoxCast(capsuleCollider.bounds.center, capsuleCollider.bounds.size, 0f, Vector2.left, extraHeightText, platformLayerMask);
        RaycastHit2D raycastRight = Physics2D.BoxCast(capsuleCollider.bounds.center, capsuleCollider.bounds.size, 0f, Vector2.right, extraHeightText, platformLayerMask);
        return (raycastLeft.collider != null && raycastRight.collider != null);
    }

    private void Attack()
    {
        if 
        (
        
            !animator.GetCurrentAnimatorStateInfo(0).IsName("Attack 1") && 
            !animator.GetCurrentAnimatorStateInfo(0).IsName("Attack 2") &&
            Input.GetButtonDown("Fire1")
        )
        {
            animator.SetTrigger(attackTriggers[Random.Range(0,attackTriggers.Length)]);
        }
    }

    public void Slash()
    {
        GameObject s = Instantiate(slash, sword.transform.position, Quaternion.identity);
        s.transform.parent = sword.transform;
    }

    public void Knockback()
    {
        Debug.Log("knockback");
        Vector2 direction = (transform.localScale.x == 1) ? Vector2.left : Vector2.right;
        rigidBody.AddForce(Vector2.left * rigidBody.mass * knockbackForce);
    }
    
}
