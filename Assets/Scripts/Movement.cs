using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
public class Movement : MonoBehaviour
{
    [Header("Movement Stats")]
    [SerializeField] float moveSpeed;
    [SerializeField] float jumpPower;
    [SerializeField] float fallMultiplier;
    [SerializeField] float lowJumpMultiplier;

    [Header("Dash Stats")]
    [SerializeField] float dashPower;
    [SerializeField] float dashLength;
    [SerializeField] float dashCD;

    [Header("Wall Movement")]
    [SerializeField] float WallFriction;
    [SerializeField] float ClimbingSpeed;
    private bool isWallSliding;
    bool DisableGrab = false;


    //WALL JUMPING
    [SerializeField] bool isWallJumping;
    private float wallJumpingDirection;
    [SerializeField] float wallJumpingTime;
    private float wallJumpingCounter;
    [SerializeField] float wallJumpingDuration;
    [SerializeField] Vector2 wallJumpingPower;

    [Header("GameObjects")]
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Animator animator;
    [SerializeField] Transform GroundCheck;
    [SerializeField] Transform LeftWC;
    [SerializeField] Transform RightWC;
    [SerializeField] BoxCollider2D boxCollider;
    [SerializeField] SpriteRenderer SR;

    [Header("Layers")]
    public LayerMask Groundlayer;

    //  FLOATS
    private float horizontal;
    private float vertical;
    private float xRaw;
    private float yRaw;
    private float OriginalGravity;
    private float side = 1;

    //  BOOLS
    private bool canDash = true;
    private bool canMove = true;


    //STATES
    private bool isFacingRight = true;
    private bool isDashing;
    private bool wallGrabbed;
    private bool IsJumping;
    private bool IsFalling;
    private bool Grounded = false;
    private bool IsClimbing = true;
    private bool WallDetected;


    private void Awake()
    {
        OriginalGravity = rb.gravityScale; //Get the gravity value so when we tweak it at different points in the script we can return it back to normal
    }


    void Update()
    {
        handleMovement();
    }

    void handleMovement()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxis("Vertical");
        xRaw = Input.GetAxisRaw("Horizontal");
        yRaw = Input.GetAxisRaw("Vertical");
        Vector2 dir = new Vector2(horizontal, vertical);
        if (horizontal > 0)
        {
            side = 1;
        }
        if (horizontal < 0)
        {
            side = -1;
        }

        if (isDashing)
        {
            return; //Makes it so that the program cant go past this line of code until the dash is over, tldr disables movement.
        }

        HandleWalk();
        HandleFlip();
        HandleWallJump();
        HandleJump();
        if (WallDetect())
        {
            WallDetected = true;
            animator.SetBool("WallDetected", WallDetected);
        }
        else
        {
            WallDetected = false;
            animator.SetBool("WallDetected", WallDetected);
        }

        if (WallDetect() && !groundCheck() && rb.velocity.y < 0 && !wallGrabbed)
        {
            WallSlide();
        }
        else
        {
            animator.SetBool("WallSliding", isWallSliding);
            SR.flipX = false;
            isWallSliding = false;
        }

        if(WallDetect() && Input.GetAxis("Right Trigger") == 1)
        {
            wallGrab();
        }
        else
        {
            rb.gravityScale = OriginalGravity;
            wallGrabbed = false;
            animator.SetBool("WallGrabbed", wallGrabbed);
            IsClimbing = false;
            animator.SetBool("IsClimbing", IsClimbing);
            SR.flipX = false;
        }

        if (Input.GetButtonDown("Dash") && canDash)
        {
            StartCoroutine(Handledash());
        }
    }

    void HandleWalk()
    {
        if (!canMove)
        {
            return;
        }

        if (wallGrabbed)
        {
            return;
        }

        if (isWallJumping)
        {
            return;
        }

        rb.velocity = new Vector2(horizontal * moveSpeed, rb.velocity.y);
        animator.SetFloat("Speed", Mathf.Abs(horizontal * moveSpeed));
    }

    void HandleJump()
    {
        
        if (rb.velocity.y < 0) //If you are no longer jumping increases gravity so that the fall is faster, gives a better feel
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump")) //If you release the jump button before you reach max height, increases gravity so that u can either shorthop or jump
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump") && groundCheck()) //Checks wheter you are touching the ground before it allows you to jump
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpPower);
            Grounded = false;
            animator.SetBool("Grounded", Grounded);
        }
        if (rb.velocity.y > 0f && !WallDetect())
        {
            IsJumping = true;
            animator.SetBool("IsJumping", IsJumping);
        }
        if (rb.velocity.y <= 0f)
        {
            IsJumping = false;
            animator.SetBool("IsJumping", IsJumping);
        }
        if (rb.velocity.y < 0f && !WallDetect())
        {
            IsFalling = true;
            animator.SetBool("IsFalling", true);
        }
        if (rb.velocity.y >= 0f)
        {
            IsFalling = false;
            animator.SetBool("IsFalling", false); //cEHCKING FOER WAKATIME
        }

    }

    private bool groundCheck()
    {
        return Physics2D.OverlapCircle(GroundCheck.position, 0.2f, Groundlayer); //Creates a circle at the coordinates of the groundcheck gameobject that will overlap with the ground
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (groundCheck() == true)
        {
            Grounded = true;
            IsFalling = false;
            animator.SetBool("Grounded", Grounded);
            animator.SetBool("IsFalling", IsFalling);
            canDash = true;
        }
    }

    void HandleWallJump()
    {
        if(isWallSliding) //Prepares the jump
        {
            isWallJumping = false;
            wallJumpingDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingTime;

            CancelInvoke("StopWallJumping");
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime; //Counting the cooldown
        }

        if (Input.GetButtonDown("Jump") && wallJumpingCounter > 0f)
        {
            StartCoroutine("disableGrab");
            isWallJumping = true;
            rb.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
            wallJumpingCounter = 0f;

            if(transform.localScale.x != wallJumpingDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1;
                transform.localScale = localScale;
            }
            Invoke("StopWallJumping", wallJumpingTime);
        }

    }

    private void StopWallJumping()
    {
        isWallJumping = false;
    }

    void wallGrab()
    {
        wallGrabbed = true;
        animator.SetBool("WallGrabbed", wallGrabbed);
        if (rb.velocity.y < 0|| rb.velocity.y > 0)
        {
            IsClimbing = true;
            animator.SetBool("IsClimbing", IsClimbing);
        }
        else
        {
            IsClimbing = false;
            animator.SetBool("IsClimbing", IsClimbing);
        }
        
        if (wallGrabbed)
        {
            SR.flipX = true;
        }

        if(DisableGrab == false)
        {
            rb.velocity = new Vector2(0f, vertical * moveSpeed * ClimbingSpeed);
            rb.gravityScale = 0f;
        }
        //if (horizontal == 1 && !isFacingRight)
        //{
        //    HandleFlip();
        //}
        //if (horizontal == -1 && isFacingRight)
        //{
        //    HandleFlip();
        //}
    }

    void WallSlide()
    {
        SR.flipX = true;
        isWallSliding = true;
        animator.SetBool("WallSliding", isWallSliding);
        rb.velocity = new Vector2(rb.velocity.x, -WallFriction);
        
    }

    private bool WallDetect()
    {
        return Physics2D.OverlapCircle((Vector2)LeftWC.position, 0.4f, Groundlayer) || Physics2D.OverlapCircle((Vector2)RightWC.position, 0.4f, Groundlayer);
    }

    IEnumerator Handledash()
    {
        canDash = false;
        isDashing = true;
        rb.gravityScale = 0f;
        CameraShake.instance.ShakeCamera(2f, 0.2f); //Requiere un script extra
        rb.velocity = new Vector2(xRaw, yRaw).normalized * (dashPower - 10f);
        if(xRaw == 0 && yRaw == 0)
        {
            rb.velocity = new Vector2(transform.localScale.x * dashPower, 0f);
        }
        yield return new WaitForSeconds(dashLength);
        isDashing = false;
        rb.gravityScale = OriginalGravity;
        yield return new WaitForSeconds(dashCD);
        if(groundCheck())
        {
            canDash = true;
        }
    }

    void HandleFlip()
    {
        if (horizontal == -1 && isFacingRight && !isWallJumping && !wallGrabbed || horizontal == 1 && !isFacingRight && !isWallJumping && !wallGrabbed)
        {
            Vector3 localScale = transform.localScale;
            isFacingRight = !isFacingRight;
            localScale.x *= -1;
            transform.localScale = localScale;
        }
    }

    IEnumerator DisableMovement(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }

    IEnumerator disableGrab()
    {
        DisableGrab = true;
        yield return new WaitForSeconds(wallJumpingDuration);
        DisableGrab = false;
    }
}