using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSliding : MonoBehaviour
{
    [Header("References")]
    private PlayerStats playerStats;
    public Transform orientation;
    public Transform playerObj;
    public Rigidbody rb;
    public PlayerFPSController pfpsc;
    public PlayerCam cam;

    [Header("Sliding")]
    private Vector3 moveDirection;
    public float maxSlideTime;
    public float slideForce;
    public float slideSpeedBoost;
    private float slideTimer;
    public float slideYScale;
    private float startYScale = 1;
    public float slideCooldown;

    [Header("Input")]
    public KeyCode crouchKey = KeyCode.LeftControl;
    private float verticalInput;
    private float horizontalInput;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pfpsc = GetComponent<PlayerFPSController>();
        slideTimer = maxSlideTime;
        playerStats = GetComponent<PlayerStats>();
    }


    // If SprintMode + Moving, then slide
    // If SprintMode + Not Moving, then crouch

    // Update is called once per frame
    void Update()
    {
        if (!playerStats.canMove)
            return;

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Slide when crouching while moving and sprinting OR while crouching on a slope
        if (pfpsc.canSlide && ((Input.GetKeyDown(crouchKey) && (horizontalInput != 0 || verticalInput != 0) && pfpsc.sprintMode && pfpsc.isGrounded) || (Input.GetKeyDown(crouchKey) && pfpsc.OnSlope())))
            StartSlide();

        if (Input.GetKeyUp(crouchKey) && pfpsc.isSliding)
            EndSlide();
    }

    private void FixedUpdate()
    {
        if (pfpsc.isSliding)
            SlidingMovement();
    }

    private void StartSlide()
    {  
        transform.localScale = new Vector3(transform.localScale.x, slideYScale, transform.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Force);
        
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        pfpsc.isSliding = true;

        pfpsc.canSlide = false;
        cam.ChangeFOV(50);
    }

    private void SlidingMovement()
    {
        // Normal Slide
        if (!pfpsc.OnSlope())
        {
            rb.AddForce(moveDirection.normalized * slideForce, ForceMode.Impulse);
            slideTimer -= Time.deltaTime;
        }
        // Slope
        else
        {
            rb.AddForce(pfpsc.GetSlopeMoveDown() * slideForce, ForceMode.Impulse);
            pfpsc.moveSpeed += slideSpeedBoost;    // Increase speed the longer you slide
        }  

        if (slideTimer <= 0)
            EndSlide();
    }

    private void EndSlide()
    {
        pfpsc.isSliding = false;
        slideTimer = maxSlideTime;

        // Only go back up with no crouching
        if(!Input.GetKey(crouchKey))
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);

        // Ensure that player's desired speed returns after sliding
        if (pfpsc.state == PlayerFPSController.MovementStates.walking)
            pfpsc.desiredMoveSpeed = pfpsc.walkSpeed;
        else if (pfpsc.state == PlayerFPSController.MovementStates.crouching)
            pfpsc.desiredMoveSpeed = pfpsc.crouchSpeed;
        else if (pfpsc.state == PlayerFPSController.MovementStates.sprinting)
            pfpsc.desiredMoveSpeed = pfpsc.sprintSpeed;

        Invoke(nameof(ResetSlide), slideCooldown);
        cam.ChangeFOV(60);
    }

    private void ResetSlide()
    {
        pfpsc.canSlide = true;
    }
}
