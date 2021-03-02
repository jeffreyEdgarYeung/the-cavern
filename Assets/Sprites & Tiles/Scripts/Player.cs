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
    [SerializeField] bool isGroundedLastFrame;
    [SerializeField] bool isJumping;
    [SerializeField] bool jumpKeyHeld;
    [SerializeField] bool isFalling;
    [SerializeField] bool isAgainstWall;

    [Header("Attack Parameters")]
    [SerializeField] GameObject sword;
    [SerializeField] GameObject slash;

    [Header("SFX")]
    [SerializeField] AudioClip jumpSFX;
    [SerializeField] [Range(0,1f)] float jumpVolume;
    [SerializeField] AudioClip landingSFX;
    [SerializeField] [Range(0, 1f)] float landingVolume;


    // Cached refs
    Rigidbody2D rigidBody;
    CapsuleCollider2D capsuleCollider;
    Animator animator;
    AudioSource audioSource;

    string[] attackTriggers = new string[] { "attackTrigger1", "attackTrigger2" };

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        animator = GetComponent<Animator>();
        jumpForce = Mathf.Sqrt(2 * Physics2D.gravity.magnitude * jumpHeight);
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = IsGrounded();
        isJumping = !isGrounded;
        isAgainstWall = IsAgainstWall();
        Move();
        HandleSpriteDirection();

        isGroundedLastFrame = isGrounded; 

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
        HandleRunSFX();
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
                PlaySFX(jumpSFX, jumpVolume);
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
        RaycastHit2D raycastHit = Physics2D.BoxCast
        (
            capsuleCollider.bounds.center - new Vector3(0f, 0.1f, 0),   // Slightly lowered from center - new Vector3(0f, 0.1f, 0)
            capsuleCollider.bounds.size - new Vector3(0.1f, 0.1f, 0),  // - new Vector3(0.1f, 0.1f, 0)
            0f, 
            Vector2.down, 
            extraHeightText, 
            platformLayerMask);

        Color rayColor;
        if (raycastHit.collider != null)
        {
            rayColor = Color.green;
        }
        else
        {
            rayColor = Color.red;
        }
        
        Debug.DrawRay(capsuleCollider.bounds.center + new Vector3(capsuleCollider.bounds.extents.x, 0), Vector2.down * (capsuleCollider.bounds.extents.y ), rayColor);
        Debug.DrawRay(capsuleCollider.bounds.center - new Vector3(capsuleCollider.bounds.extents.x, 0), Vector2.down * (capsuleCollider.bounds.extents.y ), rayColor);
        Debug.DrawRay(capsuleCollider.bounds.center - new Vector3(capsuleCollider.bounds.extents.x, capsuleCollider.bounds.extents.y), Vector2.right * (capsuleCollider.bounds.extents.x * 2f), rayColor);
        
        return raycastHit.collider != null;
    }

    private void Fall()
    {
        isFalling = rigidBody.velocity.y < -0.5f;
        animator.SetBool("isFalling", isFalling);
        animator.SetBool("isAgainstWall", isAgainstWall);
        HandleLandingSFX();
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
        rigidBody.AddForce(direction * rigidBody.mass * knockbackForce);
    }

    private void PlaySFX(AudioClip clip, float volume) { AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, volume); }

    private void HandleRunSFX()
    {
        if (!isGrounded) { GetComponent<AudioSource>().Stop(); }

        if (!(Mathf.Abs(rigidBody.velocity.x) > Mathf.Epsilon && isGrounded))
        {
            audioSource.Stop();
            return;
        }

        if (!audioSource.isPlaying){ audioSource.Play(); }
    }

    private void HandleLandingSFX()
    {
        if (!isGroundedLastFrame && isGrounded)
        {
            PlaySFX(landingSFX, landingVolume);
        }
    }
}
