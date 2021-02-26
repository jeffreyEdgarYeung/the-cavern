﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player Status")]
    [SerializeField] [Range(0, 50)] float maxSpeed = 10f;

    [Header("Jump Parameters")]
    [SerializeField] float jumpHeight = 1f;
    [SerializeField] bool isGrounded;
    [SerializeField] bool isJumping;
    [SerializeField] bool jumpKeyHeld;
    [SerializeField] Vector2 counterJumpForce;
    [SerializeField] private LayerMask platformLayerMask;

    // Player State
    float jumpForce;

    Rigidbody2D rigidBody;
    CapsuleCollider2D capsuleCollider;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        jumpForce = Mathf.Sqrt(2 * Physics2D.gravity.magnitude * jumpHeight);
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        HandleSpriteDirection();
        isGrounded = IsGrounded();
        isJumping = !isGrounded;
    }

    void FixedUpdate()
    {
        HandlePartialJump();
    }

    private void Move()
    {
        Run();
        HandleJump();
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

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            jumpKeyHeld = true;

            if (isGrounded)
            {
                isJumping = true;
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

        Debug.Log(raycastHit.collider);
        return raycastHit.collider != null;
    }
    
   
}
