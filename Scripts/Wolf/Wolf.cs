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
    [Tooltip("How fast the wolf slows down in the air.")]
    public float airDecelerationRate = 50;
    [Tooltip("How many units/sec^2 the player decelerates at when not pressing any input.")]
    public float speedDecelerationRate = 100;
    [Tooltip("How many units/sec^2 the player decelerates at when pressing backwards.")]
    public float stopDecelerationRate = 300;
    [Tooltip("Force used to make the player jump.")]
    public float jumpForceAmount = 100;

    public FMODUnity.StudioEventEmitter
        fm_running,
        fm_air;

    [Header("Privates")]
    [SerializeField]
    private State previousState;
    [SerializeField]
    private State previousDashState;
    [SerializeField]
    private FacingDir facingDirection;

    [SerializeField]
    private float
        aboutToLandCheckDistance,
        airTimer, // how long the player has been airborne, used for animations.
        dashTimer,
        deathTimer,
        groundBufferTimer,
        groundCheckDistance,
        speed,
        speedTierTimer;

    [SerializeField]
    private bool
        aboutToLand, // Used to prevent animations from changing rapidly.
        backwardPressed,
        dashPressed,
        forwardPressed,
        jumpPressed,
        leftPressed,
        rightPressed;

    [SerializeField]
    private int
        currentAirCharges,
        currentRunTier;

    [SerializeField]
    private Vector3
        vec_down,
        vec_forward,
        vec_offset,
        vec_turn,
        vec_pauseVelocity;

    private Animator anim;
    private Rigidbody rb;
    private BoxCollider coll;
    private Turn turnTrigger; // gets set when we enter a turn trigger

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
        backwardPressed = false;
        dashPressed = false;
        forwardPressed = false;
        leftPressed = false;
        rightPressed = false;
        currentAirCharges =  maximumAirCharges;
        currentRunTier = -1;
        vec_down = new Vector3(0, -1, 0);
        vec_forward = new Vector3(0, 0, 1);
        vec_turn = new Vector3(0, 90, 0);
        vec_offset = new Vector3(0, 1, 0);
        vec_pauseVelocity = new Vector3();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        coll = GetComponent<BoxCollider>();
        turnTrigger = null;
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
            speed -= stopDecelerationRate * Time.deltaTime;
            checkAndDowngradeSpeedTier();
            if (speed < 0)
            {
                speed *= -1;
                flipFacingDirection();
                // TODO switch to idle anim at 1 speed.
            }
            transform.Translate(vec_forward * speed * Time.deltaTime);
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
            if (backwardPressed && !forwardPressed)
            {
                flipFacingDirection();
                setSpeedtier(2);
            }
            else
            {
                if (currentRunTier < 2)
                    setSpeedtier(2);
            }
        }
        if ((leftPressed || rightPressed) && turnTrigger != null)
        {
            turn();
        }
        
        anim.speed = speed / 120;
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
        if (hasAirCharges() && dashPressed)
        {
            consumeAirCharge();
            Vector3 vel = rb.velocity;
            vel.y = 0;
            rb.velocity = vel;
            if (currentRunTier < 2)
            {
                setSpeedtier(2);
            }
            speed = getCurrentRunSpeedBasedOnTier();
            transitionState(State.DASHING);
        }
        if (hasAirCharges() && jumpPressed)
        {
            Vector3 vel = rb.velocity;
            vel.y = 0;
            rb.velocity = vel;
            consumeAirCharge();
            rb.AddForce(transform.up * jumpForceAmount);
        }
        if (speed > 0.1f)
        {
            speed -= airDecelerationRate * Time.deltaTime;
            if (speed <= 0.1f)
                speed = 0;
            checkAndDowngradeSpeedTier();
            transform.Translate(vec_forward * speed * Time.deltaTime);
        }

        checkForGroundAndTransition();
    }

    private void updateDashing()
    {
        dashTimer -= Time.deltaTime;
        transform.Translate(vec_forward * speed * Time.deltaTime);

        if (dashTimer <= 0)
        {
            transitionState(previousDashState);
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
                SpawnPoint spawn = GameManager.i().spawnPoint.GetComponent<SpawnPoint>();
                setFacingDirection(spawn.facingDirection);
                setPosition(spawn.transform.position);
                transitionState(State.GRUONDED);
                return;
            }

            Checkpoint cp = checkpoint.GetComponent<Checkpoint>();
            setFacingDirection(cp.getFacingDirection());
            setPosition(cp.transform.position);
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
            case State.DASHING:
                rb.useGravity = true;
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
            case State.DASHING:
                previousDashState = currentState;
                rb.useGravity = false;
                dashTimer = dashLengthTime;
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
     * Forces a certain speedtier to be applied to the wolf, instantly bumping the wolf to the
     * desired speed.
     */
    private void setSpeedtier(int tierNumber)
    {
        currentRunTier = tierNumber;
        speedTierTimer = 0;
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
        // MAYBE INCLUDE AS AN OPTION?
        // we need to invert the forward/backward based on if the wolf is facing up or down.
        // If they're facing up, positive vertical values produce forward input.
        // If they're facing down, positive vertical values produce backward input.
        bool isFacingUp = facingDirection == FacingDir.UP_LEFT || facingDirection == FacingDir.UP_RIGHT;
        int mult = isFacingUp ? 1 : -1;
        float vertical = Input.GetAxis("Vertical") * mult;
        float horizontal = Input.GetAxis("Horizontal") * mult;
        forwardPressed = vertical > 0;
        backwardPressed = vertical < 0;
        leftPressed = horizontal < 0;
        rightPressed = horizontal > 0;
        jumpPressed = Input.GetButtonDown("Jump");
        dashPressed = Input.GetButtonDown("Dash");
    }

    /**
     * Turns the wolf so now they are running in a new direction based on current inputs
     * and the turnTrigger we're currently inside.
     * @see Turn.cs for more information.
     */
    private void turn()
    {
        if (turnTrigger == null)
        {
            Debug.LogWarning("A call to turn, but no turnTrigger object is set!");
            return;
        }

        FacingDir newDirection = FacingDir.UP_RIGHT;
        switch (facingDirection)
        {
            case FacingDir.UP_RIGHT:   newDirection = leftPressed ? FacingDir.UP_LEFT : FacingDir.DOWN_RIGHT; break;
            case FacingDir.UP_LEFT:    newDirection = leftPressed ? FacingDir.DOWN_LEFT : FacingDir.UP_RIGHT; break;
            case FacingDir.DOWN_RIGHT: newDirection = leftPressed ? FacingDir.UP_RIGHT : FacingDir.DOWN_LEFT; break;
            case FacingDir.DOWN_LEFT:  newDirection = leftPressed ? FacingDir.DOWN_RIGHT : FacingDir.UP_LEFT; break;
        }

        if (! turnTrigger.allowsDirection(newDirection))
            return;

        setFacingDirection(newDirection);
        if (turnTrigger.centerOnTurn)
            setPosition(turnTrigger.transform.position, true);

        if (turnTrigger.onlyTurnOnce)
            turnTrigger = null;
    }

    /**
     * Sets the direction the wolf is facing.
     * Also modifies the camera's position (see cameraFollowing.SetOrientation).
     * @param newDirection The direction the wolf will face.
     */
    public void setFacingDirection(FacingDir newDirection)
    {
        facingDirection = newDirection;

        switch (facingDirection)
        {
            case FacingDir.UP_RIGHT:
                vec_turn.Set(0, 90, 0);
                break;

            case FacingDir.UP_LEFT:
                vec_turn.Set(0, 0, 0);
                break;

            case FacingDir.DOWN_RIGHT:
                vec_turn.Set(0, 180, 0);
                break;

            case FacingDir.DOWN_LEFT:
                vec_turn.Set(0, -90, 0);
                break;
        }

        transform.rotation = Quaternion.Euler(vec_turn);
        GameManager.i().cameraFollowing.SetOrientation(newDirection);
    }

    /**
     * Reverse the direction the player is facing. If they're facing up-right, they'll now
     * face down-left.
     */
    public void flipFacingDirection()
    {
        switch (facingDirection)
        {
            case FacingDir.UP_RIGHT:   setFacingDirection(FacingDir.DOWN_LEFT); break;
            case FacingDir.UP_LEFT:    setFacingDirection(FacingDir.DOWN_RIGHT); break;
            case FacingDir.DOWN_RIGHT: setFacingDirection(FacingDir.UP_LEFT); break;
            case FacingDir.DOWN_LEFT:  setFacingDirection(FacingDir.UP_RIGHT); break;
        }
    }

    /**
     * Set's the wolf's position.
     * @param newPosition The position the wolf will be set to.
     */
    public void setPosition(Vector3 newPosition)
    {
        setPosition(newPosition, false);
    }

    /**
     * Set's the wolf's position to the specified position, ignoring the
     * new position's y-component if specified.
     * @param newPosition the position the wolf will be set to.
     * @param ignoreY True if the wolf's y-component should not be set.
     */
    public void setPosition(Vector3 newPosition, bool ignoreY)
    {
        if (ignoreY)
            newPosition.y = transform.position.y;
        transform.position = newPosition;
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

    public FacingDir getFacingDirection()
    {
        return facingDirection;
    }

    /**
     * Returns true if the wolf has at least one air charge.
     */
    public bool hasAirCharges()
    {
        return currentAirCharges > 0;
    }

    public void consumeAirCharge()
    {
        if (currentAirCharges > 0)
            --currentAirCharges;
    }

    /**
     * Gives an air charge to the wolf, unless they're already at the maximum
     * number of air charges.
     */
    public void returnAirCharge()
    {
        if (currentAirCharges < maximumAirCharges)
            ++currentAirCharges;
    }

    /**
     * Sets the wolf's air charges to the maximum amount.
     */
    public void returnAllAirCharges()
    {
        currentAirCharges = maximumAirCharges;
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
            turnTrigger = other.GetComponent<Turn>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "TurnPoint")
        {
            turnTrigger = null;
        }
    }
}
