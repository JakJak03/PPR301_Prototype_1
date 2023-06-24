using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    // Reference: @DawnosaurDev at youtube.com/c/DawnosaurStudios

    private Rigidbody2D playerRb;

    public PlayerData movementData;
    public Walk walkAnim;

    #region Variables
    public bool canMove;
    public bool IsFacingRight { get; private set; }
    public bool IsJumping { get; private set; }
    public bool IsWallJumping { get; private set; }
    public bool IsSliding { get; private set; }
    // Jump
    private bool playerJumpCut, playerFallingFromJump;
    private int previousWallJumpDirection; // Wall Jump int is used as force direction -1 left. 0 none, 1 right
    // Timers
    private float timeSinceGrounded, timeSinceOnWall, timeSinceOnWallRight, timeSinceOnWallLeft, wallJumpStartTime, LastPressedJumpTime;
    // Input
    private Vector2 playerMoveInput;
    // Inspector Transforms
    [Header("Checks")]
    [SerializeField] private Transform _groundCheckPoint;
    //Size of groundCheck depends on the size of your character generally you want them slightly smaller than width (for ground) and height (for the wall check)
    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);
    [Space(5)]
    [SerializeField] private Transform _frontWallCheckPoint;
    [SerializeField] private Transform _backWallCheckPoint;
    [SerializeField] private Vector2 _wallCheckSize = new Vector2(0.5f, 1f);

    [Header("Layers & Tags")]
    [SerializeField] private LayerMask _groundLayer;
    #endregion

    //enum PreviousWallJumpDirection
    //{
    //    Left,
    //    Right,
    //    None
    //}
    //enum CurrentAction
    //{
    //    Jumping,
    //    WallJumping,
    //    Sliding,
    //    Idle
    //}
    //PreviousWallJumpDirection previousWallJumpDirection;
    //CurrentAction currentAction;

    [Serializable]
    public class Walk
    {
        public float frameInterval;
        public Sprite[] sprites; // The sprites for each frame of the attack
        public Sprite defaultSprite;
    }

    private void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();

        canMove = true;

        SetGravityScale(movementData.gravityScale);

        IsFacingRight = true;

    }
    bool isWalking;
    private void Update()
    {
        if(!isWalking)
        {
            StartCoroutine(WalkAnim());
        }


        #region TIMERS
        timeSinceGrounded -= Time.deltaTime;
        timeSinceOnWall -= Time.deltaTime;
        timeSinceOnWallRight -= Time.deltaTime;
        timeSinceOnWallLeft -= Time.deltaTime;

        LastPressedJumpTime -= Time.deltaTime;
        #endregion

        #region INPUT HANDLER
        playerMoveInput.x = Input.GetAxisRaw("Horizontal");
        playerMoveInput.y = Input.GetAxisRaw("Vertical");

        if (playerMoveInput.x != 0)
            CheckDirectionToFace(playerMoveInput.x > 0);

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.J))
        {
            OnJumpInputDown();
        }

        if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.C) || Input.GetKeyUp(KeyCode.J))
        {
            OnJumpInputUp();
        }
        #endregion

        #region COLLISION CHECKS
        if (!IsJumping)
        {
            // Ground Check
            if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer) && !IsJumping) // checks if set box overlaps with ground
            {
                timeSinceGrounded = movementData.coyoteTime; // if so sets the lastGrounded to coyoteTime
            }

            // Right Wall Check
            if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)
                    || (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)) && !IsWallJumping)
                timeSinceOnWallRight = movementData.coyoteTime;

            // Right Wall Check
            if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)
                || (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)) && !IsWallJumping)
                timeSinceOnWallLeft = movementData.coyoteTime;

            // Two checks needed for both left and right walls since whenever the play turns the wall checkPoints swap sides
            timeSinceOnWall = Mathf.Max(timeSinceOnWallLeft, timeSinceOnWallRight);
        }
        #endregion

        #region JUMP CHECKS
        if (IsJumping && playerRb.velocity.y < 0)
        {
            IsJumping = false;

            if (!IsWallJumping)
                playerFallingFromJump = true;
        }

        if (IsWallJumping && Time.time - wallJumpStartTime > movementData.wallJumpTime)
        {
            IsWallJumping = false;
        }

        if (timeSinceGrounded > 0 && !IsJumping && !IsWallJumping)
        {
            playerJumpCut = false;

            if (!IsJumping)
                playerFallingFromJump = false;
        }

        //Jump
        if (CanJump() && LastPressedJumpTime > 0)
        {
            IsJumping = true;
            IsWallJumping = false;
            playerJumpCut = false;
            playerFallingFromJump = false;
            Jump();
        }
        //WALL JUMP
        else if (CanWallJump() && LastPressedJumpTime > 0)
        {
            IsWallJumping = true;
            IsJumping = false;
            playerJumpCut = false;
            playerFallingFromJump = false;
            wallJumpStartTime = Time.time;
            previousWallJumpDirection = (timeSinceOnWallRight > 0) ? -1 : 1;

            WallJump(previousWallJumpDirection);
        }
        #endregion

        #region SLIDE CHECKS
        if (CanSlide() && ((timeSinceOnWallLeft > 0 && playerMoveInput.x < 0) || (timeSinceOnWallRight > 0 && playerMoveInput.x > 0)))
            IsSliding = true;
        else
            IsSliding = false;
        #endregion

        #region GRAVITY
        //Higher gravity if we've released the jump input or are falling
        if (IsSliding)
        {
            SetGravityScale(0);
        }
        else if (playerRb.velocity.y < 0 && playerMoveInput.y < 0)
        {
            //Much higher gravity if holding down
            SetGravityScale(movementData.gravityScale * movementData.fastFallGravityMult);
            //Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
            playerRb.velocity = new Vector2(playerRb.velocity.x, Mathf.Max(playerRb.velocity.y, -movementData.maxFastFallSpeed));
        }
        else if (playerJumpCut)
        {
            //Higher gravity if jump button released
            SetGravityScale(movementData.gravityScale * movementData.jumpCutGravityMult);
            playerRb.velocity = new Vector2(playerRb.velocity.x, Mathf.Max(playerRb.velocity.y, -movementData.maxFallSpeed));
        }
        else if ((IsJumping || IsWallJumping || playerFallingFromJump) && Mathf.Abs(playerRb.velocity.y) < movementData.jumpHangTimeThreshold)
        {
            SetGravityScale(movementData.gravityScale * movementData.jumpHangGravityMult);
        }
        else if (playerRb.velocity.y < 0)
        {
            //Higher gravity if falling
            SetGravityScale(movementData.gravityScale * movementData.fallGravityMult);
            //Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
            playerRb.velocity = new Vector2(playerRb.velocity.x, Mathf.Max(playerRb.velocity.y, -movementData.maxFallSpeed));
        }
        else
        {
            //Default gravity if standing on a platform or moving upwards
            SetGravityScale(movementData.gravityScale);
        }
        #endregion
    }

    private IEnumerator WalkAnim()
    {
        while(canMove && playerRb.velocity.magnitude >= 0.01f)
        {
            isWalking = true;
            for (int i = 0; i < walkAnim.sprites.Length; i++)
            {
                if (canMove && playerRb.velocity.magnitude >= 0.01f)
                {
                    GetComponent<SpriteRenderer>().sprite = walkAnim.sprites[i];

                    yield return new WaitForSeconds(walkAnim.frameInterval);
                }    
            }
            if(canMove)
                GetComponent<SpriteRenderer>().sprite = walkAnim.defaultSprite;
            isWalking = false;
        }
    }

    private void FixedUpdate()
    {
        if (!canMove)
            return;

        // Handle Run
        if (IsWallJumping)
            Run(movementData.wallJumpRunLerp);
        else
            Run(1);

        // Handle Slide
        if (IsSliding)
            Slide();

        // playerRb.velocity = new Vector3(playerSpeed * Input.GetAxis("Horizontal"), playerRb.velocity.y, 0);
    }
    #region INPUT CALLBACKS
    // Methods which whandle input detected in Update()
    public void OnJumpInputDown()
    {
        LastPressedJumpTime = movementData.jumpInputBufferTime;
    }

    public void OnJumpInputUp()
    {
        if (CanJumpCut() || CanWallJumpCut())
            playerJumpCut = true;
    }
    #endregion

    #region GENERAL METHODS
    public void SetGravityScale(float scale)
    {
        playerRb.gravityScale = scale;
    }
    #endregion

    // MOVEMENT METHODS
    #region RUN METHODS
    private void Run(float lerpAmount)
    {
        // Calculate the direction we want to move in and our desired velocity
        float targetSpeed = playerMoveInput.x * movementData.runMaxSpeed;
        // We can reduce are control using Lerp() this smooths changes to are direction and speed
        targetSpeed = Mathf.Lerp(playerRb.velocity.x, targetSpeed, lerpAmount);

        #region Calculate AccelRate
        float accelRate;

        // Gets an acceleration value based on if we are accelerating (includes turning) 
        // or trying to decelerate (stop). As well as applying a multiplier if we're air borne.
        if (timeSinceGrounded > 0)
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? movementData.runAccelAmount : movementData.runDeccelAmount;
        else
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? movementData.runAccelAmount * movementData.accelInAir : movementData.runDeccelAmount * movementData.deccelInAir;
        #endregion

        #region Add Bonus Jump Apex Acceleration
        // Increase are acceleration and maxSpeed when at the apex of their jump, makes the jump feel a bit more bouncy, responsive and natural
        if ((IsJumping || IsWallJumping || playerFallingFromJump) && Mathf.Abs(playerRb.velocity.y) < movementData.jumpHangTimeThreshold)
        {
            accelRate *= movementData.jumpHangAccelerationMult;
            targetSpeed *= movementData.jumpHangMaxSpeedMult;
        }
        #endregion

        #region Conserve Momentum
        // We won't slow the player down if they are moving in their desired direction but at a greater speed than their maxSpeed
        if (movementData.doConserveMomentum && Mathf.Abs(playerRb.velocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(playerRb.velocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && timeSinceGrounded < 0)
        {
            //Prevent any deceleration from happening, or in other words conserve are current momentum
            //You could experiment with allowing for the player to slightly increae their speed whilst in this "state"
            accelRate = 0;
        }
        #endregion

        // Calculate difference between current velocity and desired velocity
        float speedDif = targetSpeed - playerRb.velocity.x;
        // Calculate force along x-axis to apply to thr player

        float movement = speedDif * accelRate;

        // Convert this to a vector and apply to rigidbody
        playerRb.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    private void Turn()
    {
        if (!canMove)
            return;

        // stores scale and flips the player along the x axis, 
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        IsFacingRight = !IsFacingRight;
    }
    #endregion

    #region JUMP METHODS
    private void Jump()
    {
        // Ensures we can't call Jump multiple times from one press
        LastPressedJumpTime = 0;
        timeSinceGrounded = 0;

        #region Perform Jump
        // We increase the force applied if we are falling
        // This means we'll always feel like we jump the same amount 
        // (setting the player's Y velocity to 0 beforehand will likely work the same, but I find this more elegant :D)
        float force = movementData.jumpForce;
        if (playerRb.velocity.y < 0)
            force -= playerRb.velocity.y;

        playerRb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        #endregion
    }

    private void WallJump(int dir)
    {
        // Ensures we can't call Wall Jump multiple times from one press
        LastPressedJumpTime = 0;
        timeSinceGrounded = 0;
        timeSinceOnWallRight = 0;
        timeSinceOnWallLeft = 0;

        #region Perform Wall Jump
        Vector2 force = new Vector2(movementData.wallJumpForce.x, movementData.wallJumpForce.y);
        force.x *= dir; // apply force in opposite direction of wall

        if (Mathf.Sign(playerRb.velocity.x) != Mathf.Sign(force.x))
            force.x -= playerRb.velocity.x;

        if (playerRb.velocity.y < 0) // checks whether player is falling, if so we subtract the velocity.y (counteracting force of gravity). This ensures the player always reaches our desired jump force or greater
            force.y -= playerRb.velocity.y;

        // Unlike in the run we want to use the Impulse mode.
        // The default mode will apply are force instantly ignoring masss
        playerRb.AddForce(force, ForceMode2D.Impulse);
        #endregion
    }
    #endregion

    #region OTHER MOVEMENT METHODS
    private void Slide()
    {
        // Works the same as the Run but only in the y-axis
        // THis seems to work fine, buit maybe you'll find a better way to implement a slide into this system
        float speedDif = movementData.slideSpeed - playerRb.velocity.y;
        float movement = speedDif * movementData.slideAccel;
        // So, we clamp the movement here to prevent any over corrections (these aren't noticeable in the Run)
        // The force applied can't be greater than the (negative) speedDifference * by how many times a second FixedUpdate() is called. For more info research how force are applied to rigidbodies.
        movement = Mathf.Clamp(movement, -Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime), Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));

        playerRb.AddForce(movement * Vector2.up);
    }
    #endregion

    #region CHECK METHODS
    public void CheckDirectionToFace(bool isMovingRight)
    {
        if (isMovingRight != IsFacingRight)
            Turn();
    }

    private bool CanJump()
    {
        return timeSinceGrounded > 0 && !IsJumping;
    }

    private bool CanWallJump()
    {
        return LastPressedJumpTime > 0 && timeSinceOnWall > 0 && timeSinceGrounded <= 0 && (!IsWallJumping ||
             (timeSinceOnWallRight > 0 && previousWallJumpDirection == 1) || (timeSinceOnWallLeft > 0 && previousWallJumpDirection == -1));
    }

    private bool CanJumpCut()
    {
        return IsJumping && playerRb.velocity.y > 0;
    }

    private bool CanWallJumpCut()
    {
        return IsWallJumping && playerRb.velocity.y > 0;
    }

    public bool CanSlide()
    {
        if (timeSinceOnWall > 0 && !IsJumping && !IsWallJumping && timeSinceGrounded <= 0)
            return true;
        else
            return false;
    }
    #endregion


    #region EDITOR METHODS
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(_frontWallCheckPoint.position, _wallCheckSize);
        Gizmos.DrawWireCube(_backWallCheckPoint.position, _wallCheckSize);
    }
    #endregion
}
