using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float moveSpeed = 2f;
    
    private Vector3 nextPosition;
    private Vector3 previousPosition;
    private Rigidbody2D platformRb;
    public Vector2 PlatformDelta { get; private set; }


    void Start()
    {
        nextPosition = pointB.position;
        previousPosition = transform.position;
        platformRb = GetComponent<Rigidbody2D>();
        if (platformRb != null)
        {
            platformRb.bodyType = RigidbodyType2D.Kinematic; 
            platformRb.gravityScale = 0;
        }
    }

    void FixedUpdate()
    {
        previousPosition = transform.position;
        
       Vector3 newPosition = Vector3.MoveTowards(
            transform.position,
            nextPosition,
            moveSpeed * Time.fixedDeltaTime
        );

        platformRb.MovePosition(newPosition);

        PlatformDelta = (Vector2)(newPosition - previousPosition);

        if (Vector3.Distance(transform.position, nextPosition) < 0.05f)
        {
            nextPosition = nextPosition == pointA.position ? pointB.position : pointA.position;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerPlatformingMovement player = collision.gameObject.GetComponent<PlayerPlatformingMovement>();
            if (player != null)
            {
                player.AttachToMovingPlatform(this);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerPlatformingMovement player = collision.gameObject.GetComponent<PlayerPlatformingMovement>();
            if (player != null)
            {
                player.DetachFromMovingPlatform();
            }
        }
    }
}