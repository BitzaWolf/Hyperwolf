/**
 * This is the main player!
 */
using UnityEngine;

public class Wolf : MonoBehaviour
{
    public enum State
    {
        GRUONDED,
        FALLING,
        DASHING,
        DYING,
        LEVEL_START,
        LEVEL_END,
        PAUSED
    }

    public enum FacingDir
    {
        UP_RIGHT,
        UP_LEFT,
        DOWN_RIGHT,
        DOWN_LEFT
    }

    public State currentState = State.GRUONDED;
    [Tooltip("How long the player is in the dashing state where they cannot act.")]
    public float dashLengthTime = 0.5f;
    [Tooltip("How long the death state plays before respawning.")]
    public float deathLengthTime = 0.3f;
    [Tooltip("How many seconds the player has after being detected off the ground, but will remain in the grounded state.")]
    public float groundedBuffer = 0.15f;
    [Tooltip("How many air charges the player has at their disposal.")]
    public int maximumAirCharges;
    [Tooltip("How many units/sec the player moves at the slowest speed.")]
    public float runSpeed0 = 10;
    public float runSpeed1 = 20;
    public float runSpeed2 = 30; // fastest speed
    [Tooltip("How many seconds the player must run at the given speed before upgrading to the next level.")]
    public float runUpgradeTime = 5;
    [Tooltip("How many units/sec^2 the player decelerates at.")]
    public float speedDecelerationRate = 5;
    [Tooltip("Force used to make the player jump.")]
    public float jumpForceAmount = 100;

    public FMODUnity.StudioEventEmitter
        fm_running,
        fm_air;

    private State previousState;
    private FacingDir facingDirection;

    private float
        aboutToLandCheckDistance,
        airTimer, // how long the player has been airborne, used for animations.
        dashTimer,
        deathTimer,
        groundBufferTimer,
        groundCheckDistance,
        speed,
        speedTierTimer;

    private bool
        aboutToLand, // Used to prevent animations from changing rapidly.
        attackPressed,
        backwardPressed,
        dashPressed,
        forwardPressed,
        jumpPressed,
        leftPressed,
        rightPressed,
        canTurn;

    [Header("Privates")]
    [SerializeField]
    private int
        currentAirCharges,
        currentRunTier;

    private Vector3
        vec_down,
        vec_forward,
        vec_offset,
        vec_turn,
        vec_pauseVelocity;

    private Animator anim;
    private Rigidbody rb;
    private BoxCollider coll;

    private void Start()
    {
        previousState = State.GRUONDED;
        facingDirection = FacingDir.UP_RIGHT;
        aboutToLandCheckDistance = 20;
        airTimer = 0;
        dashTimer = 0;
        deathTimer = 0;
        groundBufferTimer = 0;
        groundCheckDistance = 1;
        speed = 0;
        speedTierTimer = 0;
        aboutToLand = false;
        attackPressed = false;
        backwardPressed = false;
        dashPressed = false;
        forwardPressed = false;
        leftPressed = false;
        rightPressed = false;
        canTurn = false;
        currentAirCharges =  maximumAirCharges;
        currentRunTier = -1;
        vec_down = new Vector3(0, -1, 0);
        vec_forward = -transform.right;
        vec_turn = new Vector3(0, 90, 0);
        vec_offset = new Vector3(0, 1, 0);
        vec_pauseVelocity = new Vector3();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        coll = GetComponent<BoxCollider>();
    }

    private void Update()
    {
        switch (currentState)
        {
            case State.GRUONDED:    updateGrounded();   break;
            case State.FALLING:     updateFalling();    break;
            case State.DASHING:     updateDashing();    break;
            case State.DYING:       updateDying();      break;
            case State.LEVEL_START: updateLevelStart(); break;
            case State.LEVEL_END:   updateLevelEnd();   break;
            case State.PAUSED:      updatePaused();     break;
        }
    }

    private void updateGrounded()
    {
        checkInputs();

        if (forwardPressed && !backwardPressed)
        {
            speedTierTimer += Time.deltaTime;
            checkAndUpgradeSpeedTier();
            speed = getCurrentRunSpeedBasedOnTier();
            transform.Translate(vec_forward * speed * Time.deltaTime);
        }
        if (backwardPressed && !forwardPressed)
        {
            //
        }
        if (!forwardPressed && !backwardPressed) // neither forward no backward pressed
        {
            if (speed > 0.1f)
            {
                speed -= speedDecelerationRate * Time.deltaTime;
                if (speed <= 0.1f)
                    speed = 0;
                checkAndDowngradeSpeedTier();
                transform.Translate(vec_forward * speed * Time.deltaTime);
            }
        }
        if (jumpPressed)
        {
            rb.AddForce(transform.up * jumpForceAmount);
        }
        if (dashPressed)
        {
            transitionState(State.DASHING);
        }
        if (attackPressed)
        {
            // TODO activate hitbox and all.
        }
        if (leftPressed || rightPressed && canTurn)
        {
            //turn();
        }

        // set anim speed to current velocity.
        // TODO if vel near 0, switch to idle anim at 1 speed.
        checkForGroundAndTransition();
    }

    private void updateFalling()
    {
        airTimer += Time.deltaTime;
        if (airTimer > 0.5f)
        {
            checkAndSetAboutToLand();
            if (aboutToLand)
                anim.SetBool("InAir", false);
        }

        checkInputs();
        // slow down forward movement

        checkForGroundAndTransition();
    }

    private void updateDashing()
    {
        dashTimer -= Time.deltaTime;
        
        if (dashTimer <= 0)
        {
            checkForGroundAndTransition();
        }
    }

    private void updateDying()
    {
        deathTimer -= Time.deltaTime;
        if (deathTimer <= 0)
        {
            GameObject checkpoint = GameManager.i().lastCheckpoint;
            if (checkpoint == null)
            {
                GameObject spawn = GameManager.i().spawnPoint;
                turn(spawn.transform.position, spawn.GetComponent<SpawnPoint>().faceLeft);
                return;
            }

            turn(checkpoint.transform.position, checkpoint.GetComponent<Checkpoint>().faceLeft);
            transitionState(State.GRUONDED);
        }
    }

    private void updateLevelStart()
    {

    }

    private void updateLevelEnd()
    {

    }

    private void updatePaused()
    {

    }

    private void leaveState()
    {
        switch (currentState)
        {
            case State.GRUONDED:
                fm_running.Stop();
                break;
            case State.FALLING:
                fm_air.Stop();
                break;
            case State.PAUSED:
                rb.useGravity = true;
                rb.velocity = vec_pauseVelocity;
                coll.enabled = true;
                anim.speed = 1;
                break;
        }
    }

    private void transitionState(State nextState)
    {
        leaveState();
        switch (nextState)
        {
            case State.GRUONDED:
                groundBufferTimer = groundedBuffer;
                currentAirCharges = maximumAirCharges;
                Vector3 vel = rb.velocity;
                vel.y = 0;
                rb.velocity = vel;
                airTimer = 0;
                aboutToLand = false;
                fm_running.Play();
                break;
            case State.FALLING:
                fm_air.Play();
                break;
            case State.DYING:
                deathTimer = deathLengthTime;
                break;
            case State.PAUSED:
                rb.useGravity = false;
                vec_pauseVelocity = rb.velocity;
                rb.velocity.Set(0, 0, 0);
                coll.enabled = false;
                anim.speed = 0;
                break;
        }

        previousState = currentState;
        currentState = nextState;
    }

    /**
     * Check to see if the player has met the requirements to run at the next highest speed tier.
     * The requirements are to be running non-stop for some amount of time and for there to be a higher
     * tier to move at.
     */
    private void checkAndUpgradeSpeedTier()
    {
        // going from a standstill is always an instant upgrade.
        if (currentRunTier == -1)
            ++currentRunTier;

        // already at max tier
        if (currentRunTier == 2)
            return;

        if (speedTierTimer >= runUpgradeTime)
        {
            ++currentRunTier;
            speedTierTimer = 0;
        }
    }

    /**
     * Checks to see if the player's speed has dropped enough such that they're now at a slower speed tier.
     * This function does not modify the speedTierTimer, which is used to -upgrade- the player's speed.
     */
    private void checkAndDowngradeSpeedTier()
    {
        bool downgradeTier = false;
        switch (currentRunTier)
        {
            default:
            case -1: return;
            case 0: downgradeTier = speed < runSpeed0; break;
            case 1: downgradeTier = speed < runSpeed1; break;
            case 2: downgradeTier = speed < runSpeed2; break;
        }
        if (downgradeTier)
            --currentRunTier;
    }

    /**
     * Checks to see if the wolf is on ground and changes the wolf's state to grounded or 
     * falling appropriately.
     * This function should only be called once in each state-update function.
     */
    private void checkForGroundAndTransition()
    {
        bool onGround = Physics.Raycast(transform.position + vec_offset, vec_down, groundCheckDistance);
        bool groundBufferActive = groundBufferTimer >= 0;

        if (currentState == State.GRUONDED && !onGround)
        {
            if (groundBufferActive)
            {
                // Don't change to the falling state if the grounded buffer is active.
                groundBufferTimer -= Time.deltaTime;
            }
            else
                transitionState(State.FALLING);
        }
        else if (currentState == State.FALLING && onGround)
        {
            transitionState(State.GRUONDED);
        }

        // Although setting every update cycle, fixes edge cases where the player barely misses landing,
        // which sets inAir to false without ever actually landing, so we can't just set on the onground-offground
        // transition. TLDR: bad programming, but it works and other higher priorities.
        if (!aboutToLand && !groundBufferActive)
        {
            anim.SetBool("InAir", !onGround);
            //anim.ResetTrigger("JumpPressed");
        }
    }

    /**
     * Checks to see if the wolf is about to land (not if they're on ground), and sets the
     * result to the private member aboutToLand.
     */
    private void checkAndSetAboutToLand()
    {
        aboutToLand = Physics.Raycast(transform.position + vec_offset, Vector3.Normalize(vec_down * 10 + transform.forward), aboutToLandCheckDistance);
    }

    /**
     * Checks to see if any inputs have been pressed, setting private bool members to the appropriate state
     * if they are pressed or not.
     * This function does not perform any actions based on the inputs.
     */
    private void checkInputs()
    {
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");
        forwardPressed = vertical > 0;
        backwardPressed = vertical < 0;
        leftPressed = horizontal < 0;
        rightPressed = horizontal > 0;
        attackPressed = Input.GetButtonDown("Attack");
        jumpPressed = Input.GetButtonDown("Jump");
        dashPressed = Input.GetButtonDown("Dash");
    }

    /**
     * Turns the wolf so now they are running in a new direction. Additionally,
     * the wolf is snapped into place at the specified position. This is used to keep the
     * player centered on the track since turn commands are only issued from invisible
     * Turn gameobjects, which are placed on tracks at intersections/corners.
     * @param Vector3 position new position to center the player at.
     * @param bool turnLeft If the wolf should turn left.
     */
    public void turn(Vector3 position, bool turnLeft)
    {
        Vector3 pos = new Vector3(position.x, 0, position.z);
        transform.position = pos;

        float mult = turnLeft ? -1 : 1;
        transform.Rotate(vec_turn * mult);
        
        switch (facingDirection)
        {
            case FacingDir.UP_RIGHT:   facingDirection = turnLeft ? FacingDir.UP_LEFT : FacingDir.DOWN_RIGHT; break;
            case FacingDir.UP_LEFT:    facingDirection = turnLeft ? FacingDir.DOWN_LEFT : FacingDir.UP_RIGHT; break;
            case FacingDir.DOWN_RIGHT: facingDirection = turnLeft ? FacingDir.UP_RIGHT : FacingDir.DOWN_LEFT; break;
            case FacingDir.DOWN_LEFT:  facingDirection = turnLeft ? FacingDir.DOWN_RIGHT : FacingDir.UP_LEFT; break;
        }

        switch (facingDirection)
        {
            case FacingDir.UP_RIGHT:   vec_forward = -transform.right; break;
            case FacingDir.UP_LEFT:    vec_forward =  transform.forward; break;
            case FacingDir.DOWN_RIGHT: vec_forward = -transform.forward; break;
            case FacingDir.DOWN_LEFT:  vec_forward =  transform.right; break;
        }
        GameManager.i().cameraFollowing.SetOrientation(!turnLeft);
    }

    /**
     * Kills the player instantly, entering the killed-state.
     * After some time, the player is respawned at the last checkpoint or
     * initial spawn.
     */
    public void kill()
    {
        transitionState(State.DYING);
        GameManager.i().deaths += 1;
    }

    /**
     * Sets the wolf's state to paused or unpaused based on setPaused.
     * @param setPaused True if the wolf should be paused, false otherwise.
     */
    public void setPaused(bool setPaused)
    {
        if (setPaused && currentState != State.PAUSED)
            transitionState(State.PAUSED);
        else if (!setPaused && currentState == State.PAUSED)
            transitionState(previousState);
    }

    public void setToLevelStartState()
    {
        transitionState(State.LEVEL_START);
    }

    public void setToLevelEndState()
    {
        transitionState(State.LEVEL_END);
    }

    public void triggerLevelStarted()
    {
        transitionState(State.GRUONDED);
    }

    /**
     * Returns the speed at which the player is currently running.
     */
    private float getCurrentRunSpeedBasedOnTier()
    {
        switch (currentRunTier)
        {
            default:
            case -1: return 0f;
            case 0: return runSpeed0;
            case 1: return runSpeed1;
            case 2: return runSpeed2;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "TurnPoint")
        {
            canTurn = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "TurnPoint")
        {
            canTurn = false;
        }
    }
}
