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

    public State currentState = State.GRUONDED;
    [Tooltip("How long the player is in the dashing state where they cannot act.")]
    public float dashLengthTime = 0.5f;
    [Tooltip("How long the death state plays before respawning.")]
    public float deathLengthTime = 0.3f;
    [Tooltip("How many seconds the player has after being detected off the ground, but will remain in the grounded state.")]
    public float groundedBuffer = 0.15f;
    [Tooltip("How many air charges the player has at their disposal.")]
    public int maximumAirCharges;

    public FMODUnity.StudioEventEmitter
        fm_running,
        fm_air;

    private State previousState;

    private float
        aboutToLandCheckDistance,
        airTimer, // how long the player has been airborne, used for animations.
        dashTimer,
        deathTimer,
        groundBufferTimer,
        groundCheckDistance;

    private bool
        aboutToLand, // Used to prevent animations from changing rapidly.
        attackPressed,
        backwardPressed,
        dashPressed,
        forwardPressed,
        jumpPressed,
        leftPressed,
        rightPressed;

    private int currentAirCharges;

    private Vector3
        vec_down,
        vec_offset,
        vec_turn,
        vec_pauseVelocity;

    private Animator anim;
    private Rigidbody rb;
    private BoxCollider coll;

    private void Start()
    {
        previousState = State.GRUONDED;
        aboutToLandCheckDistance = 20;
        airTimer = 0;
        dashTimer = 0;
        groundBufferTimer = 0;
        groundCheckDistance = 1;
        aboutToLand = false;
        attackPressed = false;
        backwardPressed = false;
        dashPressed = false;
        forwardPressed = false;
        leftPressed = false;
        rightPressed = false;
        currentAirCharges =  maximumAirCharges;
        vec_down = new Vector3(0, -1, 0);
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
        // Move forward based on velocity/speed
        // if no forward/backward pressed, slow down to a stop
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
     * Returns true if the player wants to and is in a state to be able to turn the intended direction.
     * The player can only turn when they are on the ground (the GROUNDED state).
     * @param direction True if the direction to check is left, false if right.
     * @returns True if the player wants to (pressed the correct input) and is in a state to turn the queried
     * direction.
     */
    public bool wantsToTurn(bool turnLeft)
    {
        bool inputPressed = (turnLeft && leftPressed) || (!turnLeft && rightPressed);
        return (inputPressed && currentState == State.GRUONDED);
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
}
