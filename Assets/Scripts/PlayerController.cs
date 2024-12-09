using UnityEngine;
using System.Collections;

public enum PlayerDirection
{
    left, right
}

public enum PlayerState
{
    idle, walking, jumping, dead
}

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D body;
    private PlayerDirection currentDirection = PlayerDirection.right;
    public PlayerState currentState = PlayerState.idle;
    public PlayerState previousState = PlayerState.idle;

    [Header("Horizontal")]
    public float maxSpeed = 5f;
    public float accelerationTime = 0.25f;
    public float decelerationTime = 0.15f;
    public float dashLength = 10f;

    [Header("Vertical")]
    public float apexHeight = 3f;
    public float apexTime = 0.5f;

    [Header("Ground Checking")]
    public float groundCheckOffset = 0.5f;
    public Vector2 groundCheckSize = new(0.4f, 0.1f);
    public LayerMask groundCheckMask;

    private float accelerationRate;
    private float decelerationRate;

    private float gravity;
    private float initialJumpSpeed;

    private bool isGrounded = false;
    public bool isDead = false;

    private Vector2 velocity;

    bool dashStarted = false;

    public BoxCollider2D playerCollider;
    float playerGameSpeed = 1;
    bool BulletTimeStarted = false;
    public GameObject Trail;

    public void Start()
    {
        body.gravityScale = 0;

        accelerationRate = maxSpeed / accelerationTime;
        decelerationRate = maxSpeed / decelerationTime;

        gravity = -2 * apexHeight / (apexTime * apexTime);
        initialJumpSpeed = 2 * apexHeight / apexTime;
    }

    public void Update()
    {
        if (Input.GetKey("p"))
        {
            Instantiate(Trail, transform.position, Quaternion.identity);
        }

        previousState = currentState;

        CheckForGround();

        Vector2 playerInput = new Vector2();
        playerInput.x = Input.GetAxisRaw("Horizontal");

        if (isDead)
        {
            currentState = PlayerState.dead;
        }

        switch(currentState)
        {
            case PlayerState.dead:
                // do nothing - we ded.
                break;
            case PlayerState.idle:
                if (!isGrounded) currentState = PlayerState.jumping;
                else if (velocity.x != 0) currentState = PlayerState.walking;
                break;
            case PlayerState.walking:
                if (!isGrounded) currentState = PlayerState.jumping;
                else if (velocity.x == 0) currentState = PlayerState.idle;
                break;
            case PlayerState.jumping:
                if (isGrounded)
                {
                    if (velocity.x != 0) currentState = PlayerState.walking;
                    else currentState = PlayerState.idle;
                }
                break;
        }

        MovementUpdate(playerInput);
        JumpUpdate();

        if (!isGrounded)
            velocity.y += gravity * Time.deltaTime / playerGameSpeed;
        else
            velocity.y = 0;

        body.velocity = velocity;

        if (Input.GetKeyDown("f") && dashStarted == false)
        {
            StartCoroutine("Dash");
        }

        if (Input.GetKeyDown("q") && BulletTimeStarted == false)
        {
            StartCoroutine("BulletTime");
        }
    }

    IEnumerator BulletTime() //Changing a variable for a moment that changes the movement of the x velocity and gravity
    {
        BulletTimeStarted = true;
        playerGameSpeed = 2;
        yield return new WaitForSeconds(3);
        playerGameSpeed = 1;
        BulletTimeStarted = false;
        StopAllCoroutines();
        yield return null;
    }
    IEnumerator Dash()
    {
        dashStarted = true;
        if (currentDirection == PlayerDirection.left) //Speeds the player forward in the direction they are facing
        {
            velocity.x -= dashLength * playerGameSpeed;

        }
        else
        {
            velocity.x += dashLength * playerGameSpeed;
        }
        yield return new WaitForSeconds(1);

        dashStarted = false;
        StopAllCoroutines();
        yield return null;
    }

    private void MovementUpdate(Vector2 playerInput)
    {
        if (playerInput.x < 0)
            currentDirection = PlayerDirection.left;
        else if (playerInput.x > 0)
            currentDirection = PlayerDirection.right;

        if (playerInput.x != 0)
        {
            if (!dashStarted)//Turns of this movement if currently dashing
            {
                velocity.x += accelerationRate * playerInput.x * Time.deltaTime * playerGameSpeed;
                velocity.x = Mathf.Clamp(velocity.x, -maxSpeed * playerGameSpeed, maxSpeed * playerGameSpeed);
            }
            
        }
        else
        {
            if (velocity.x > 0)
            {
                velocity.x -= decelerationRate * Time.deltaTime;
                velocity.x = Mathf.Max(velocity.x, 0);
            }
            else if (velocity.x < 0)
            {
                velocity.x += decelerationRate * Time.deltaTime;
                velocity.x = Mathf.Min(velocity.x, 0);
            }
        }

        if (Input.GetAxisRaw("Horizontal") != 0)//Changes the collider size to it can collide in the wall when moving.
        {
            playerCollider.size = new Vector2(0.6f, 1.0f);
        }
        else
        {
            playerCollider.size = new Vector2(0.7f, 1.0f);
        }
    }

    private void JumpUpdate()
    {
        if (isGrounded && Input.GetButton("Jump"))
        {
            velocity.y = initialJumpSpeed;
            isGrounded = false;
        }
    }

    private void CheckForGround()
    {
        isGrounded = Physics2D.OverlapBox(transform.position + Vector3.down * groundCheckOffset, groundCheckSize, 0, groundCheckMask);
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position + Vector3.down * groundCheckOffset, groundCheckSize);
    }

    public bool IsWalking()
    {
        return velocity.x != 0;
    }
    public bool IsGrounded()
    {
        return isGrounded;
    }

    public PlayerDirection GetFacingDirection()
    {
        return currentDirection;
    }
}
