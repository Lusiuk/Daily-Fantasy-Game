using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerPlatformingMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    [Header("Movement")]
    public float moveSpeed = 5f;
    float horizontalMovement;

    [Header("Jumping")]
    public float jumpPower = 10f;
    public int jumpMax = 1;
    int jumpsRemaining;

    [Header("GroundCheck")]
    public Transform groundCheckPos;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    public LayerMask groundLayer;

    [Header("Gravity")]
    public float baseGravity = 2f;
    public float maxFallSpeed = 18f;
    public float fallSpeedMultiplier = 2f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        rb.linearVelocity = new Vector2(horizontalMovement * moveSpeed, rb.linearVelocityY);
        Gravity();
    }

    private void Gravity()
    {
        if(rb.linearVelocityY < 0)
        {
            rb.gravityScale = baseGravity * fallSpeedMultiplier;
            rb.linearVelocity = new Vector2(rb.linearVelocityX,Mathf.Max(-maxFallSpeed,rb.linearVelocityY));
        }
        else 
        {
            rb.gravityScale = baseGravity;
        }
        if (rb.linearVelocityY == 0)
        {
            GroundCheck();
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
    }

     public void Jump(InputAction.CallbackContext context)
    {
       if (context.performed && jumpsRemaining > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpPower*1.2f);
            jumpsRemaining--;
        }
        else if (context.canceled && jumpsRemaining > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpPower);
            jumpsRemaining--;
        }
    }

    private void GroundCheck() 
    {
        if (Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer))
        {
            jumpsRemaining = jumpMax;
        }     
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawCube(groundCheckPos.position, groundCheckSize);
    }
}
