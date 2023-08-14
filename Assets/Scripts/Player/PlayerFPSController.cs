using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFPSController : MonoBehaviour
{
    [Header("Movement")]
    private PlayerStats playerStats;
    public MovementStates state;
    public MovementStates lastState;
    public enum MovementStates
    {
        walking,
        sprinting,
        air,
        crouching,
        sliding,
        slipping
    }
    private Rigidbody rb;
    public Transform orientation;
    private float verticalInput;
    private float horizontalInput;
    private Vector3 moveDirection;
    public float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public bool sprintMode;
    public float desiredMoveSpeed;
    public float lastDesiredMoveSpeed;

    [Header("Sliding")]
    public float slideSpeed;
    public bool isSliding;
    public bool keepMomentum;
    public bool canSlide = true;

    [Header("Slopes")]
    public float maxSlopeAngle = 50f;
    private RaycastHit slopeHit;

    [Header("Jump")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public bool canJump;
    public int maxJumps;
    public int availableJumps;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    public float startYScale;

    [Header("Ground")]
    public float playerHeight;
    public float groundDrag;
    public LayerMask groundMask;
    public bool isGrounded;

    [Header("Key Binds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        playerStats = GetComponent<PlayerStats>();
        sprintMode = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerStats.canMove)
        {
            horizontalInput = 0;
            verticalInput = 0;
            return;
        }
            
        ReadInputs();
        GroundCheck();
        LimitMaxSpeed();
        StateHandler();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    void ReadInputs()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyUp(sprintKey))
            sprintMode = !sprintMode;

        // Only allow crouching if not sprint OR sprinting but not moving
        // Also only crouch when not on slopes
        // Also only crouch when you cannot slide after the cooldown
        if (!OnSlope() && (!sprintMode || (sprintMode && horizontalInput == 0 && verticalInput == 0) || (sprintMode && !canSlide)))
        {
            if (Input.GetKeyDown(crouchKey))
            {
                transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                rb.AddForce(Vector3.down * 5f, ForceMode.Force);
            }  
        }

        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }

        if (Input.GetKey(jumpKey) && canJump)
        {
            // Don't jump if no more are available
            if (availableJumps <= 0)
                return;

            availableJumps--;
            canJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }        
    }

    private void StateHandler()
    {
        // Sliding
        if (isSliding)
        {
            state = MovementStates.sliding;

            if (OnSlope() && rb.velocity.y < 0.1f)
                desiredMoveSpeed = slideSpeed;
            else
                desiredMoveSpeed = sprintSpeed;
        }
        // Crouching
        else if (Input.GetKey(crouchKey))
        {
            state = MovementStates.crouching;
            if(isGrounded)
                desiredMoveSpeed = crouchSpeed;
        }
        // Sprint
        else if (isGrounded && sprintMode)
        {
            state = MovementStates.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }
        // Walk
        else if (isGrounded && !sprintMode)
        {
            state = MovementStates.walking;
            desiredMoveSpeed = walkSpeed;
        }
        // Air
        else
        {
            state = MovementStates.air;
        }

        // Keep momentum after sliding
        if (lastState == MovementStates.sliding)
            keepMomentum = true;

        // Desired Move Speed has changed
        if (desiredMoveSpeed != lastDesiredMoveSpeed)
        {
            if (keepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
                moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastState = state;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        float speedFactor = 12f;

        while(time < difference)
        {
            // Break out if we moved from Momentum State -> Non-Momentum State 1 -> Non-Momentum State 2
            // EX: Slide -> Crouch -> Sprint
            if (lastState != MovementStates.sliding && lastState != state)
                break;

            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time/difference);
            time += Time.deltaTime * speedFactor;
            yield return null;
        }

        moveSpeed = desiredMoveSpeed;

        keepMomentum = false;
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Adjust for slope movement
        if (OnSlope() && canJump)
        {
            if (!OnSteepSlope())
                rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        // Normal ground movement  
        else if(isGrounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        // Air movement
        else
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        rb.useGravity = (!OnSlope() || OnSteepSlope());
    }

    private void Jump()
    {
        // Reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        canJump = true;
    }

    private void GroundCheck() {
        //isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight / 2 + 0.2f, groundMask);
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - 0.55f, transform.position.z);
        isGrounded = Physics.CheckSphere(spherePosition, 0.5f, groundMask, QueryTriggerInteraction.Ignore);

        if (isGrounded && (state == MovementStates.walking || state == MovementStates.sprinting || state == MovementStates.crouching))
        {
            if(canJump)
                availableJumps = maxJumps;
            rb.drag = groundDrag;
        }
        else
        {
            // In case player runs off a ledge and doesn't jump
            if (canJump && availableJumps == maxJumps)
                availableJumps--;

            rb.drag = 0;
        }
            
    }

    private void LimitMaxSpeed()
    {
        // Slope Speed
        if (OnSlope() && canJump)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }
        // Regular Speed
        else
        {
            Vector3 flatVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if (flatVelocity.magnitude >= moveSpeed)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
            }
        }   
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight/2 + 1f))
        {
            //Debug.Log(slopeHit.normal);
            if (slopeHit.normal != Vector3.up)
                return true;
        }
        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    public Vector3 GetSlopeMoveDown()
    {
        Vector3 left = Vector3.Cross(slopeHit.normal, Vector3.up);
        return Vector3.Cross(slopeHit.normal, left);
    }

    private bool OnSteepSlope()
    {
        //Debug.Log(Vector3.Angle(transform.up, slopeHit.normal));
        return Vector3.Angle(transform.up, slopeHit.normal) > maxSlopeAngle;
    }
}
