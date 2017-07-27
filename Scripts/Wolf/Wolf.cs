﻿/**
 * This is the main player!
 */
using UnityEngine;

public class Wolf : MonoBehaviour
{
    public float runSpeed = 60;
    public float jumpForce = 13;
    public float dashLength = 0.5f;
    [Tooltip("How far below the player to check if they're on something solid.")]
    public float groundCheckDistance = 5f;
    [Tooltip("How far below the player to check to see if they're about to land.")]
    public float aboutToLandCheckDistance = 10f;
    // How much faster should the player run when dashing?
    public float dashMultiplier = 2;
    // Direction player is facing. Semmantically easier than interpreting rotation info.
    public bool isRunningLeft = false;
    public FMODUnity.StudioEventEmitter fm_running, fm_air;
    [Range(0, 1)]
    [Tooltip("How much time from when the player is detected in air, but can still jump.")]
    public float jumpBuffer = 0.3f;
    public int maxAirCharges = 1;

    private Ghosting ghosting;
    private Animator anim;
    private Vector3 vec_move, vec_jump, vec_down, vec_offset, vec_turn;
    private Rigidbody rb;
    private BoxCollider coll;
    private bool
        onGround = true,
        hasDash = true,
        wantsToTurnLeft = false,
        wantsToTurnRight = false,
        doMove = true,
        isPhasedOut = false,
        aboutToLand = false;
    private float
        dashTimer = 0,
        jumpBufferTimer = 0,
        timeInAir = 0;
    private int
        airCharges = 0;

    // State for when we pause/unpause the game
    private bool isPaused = false;
    private Vector3 unpause_velocity = new Vector3();

    /**
     * Initialize a few variables.
     * Vectors are initialized and cached so the game isn't constantly creating new Vector3 objects.
     */
    private void Start()
    {
        vec_move = new Vector3();
        vec_jump = new Vector3(0, jumpForce * GameManager.FORCE_MULT, 0);
        vec_down = new Vector3(0, -10, 0);
        vec_offset = new Vector3(0, 1f, 0);
        vec_turn = new Vector3(0, 90, 0);
        rb = GetComponent<Rigidbody>();
        coll = GetComponent<BoxCollider>();
        ghosting = GetComponent<Ghosting>();
        anim = GetComponent<Animator>();
        jumpBufferTimer = jumpBuffer;
        airCharges = maxAirCharges;
    }

    private void printVec(string name, FMOD.VECTOR v)
    {
        Debug.Log(name + "(" + v.x + ", " + v.y + ", " + v.z + ")");
    }

    void Update()
    {
        moveWolf();
        checkAboutToLand();
        checkForGround();
        checkJump();
        checkTurnInput();

        // TODO use a killbox instead. I suppose we could keep this as a backup.
        if (transform.position.y <= -500)
        {
            kill();
        }
    }

    /**
     * Logic to move the wolf forward. The game actually directly manipulates the transform position instead
     * of using forces, velocity, or the physics engine in general. The up side of this strategy is that it's
     * very simple to code, makes it trivial to turn the player instantly, the running speed is set. The down
     * side is that there are no physics interactions with the wolf along the forward/running direction.
     */
    private void moveWolf()
    {
        if (!doMove)
            return;

        float mult = 1;

        if (Input.GetButtonDown("Dash") && hasDash)
        {
            ghosting.enabled = true;
            dashTimer = dashLength;
            hasDash = false;
        }
        if (dashTimer > 0)
        {
            mult = dashMultiplier;
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
                ghosting.enabled = false;
        }

        vec_move.Set(0, 0, 0);
        if (isRunningLeft)
        {
            vec_move.z = runSpeed * mult * Time.deltaTime;
        }
        else
        {
            vec_move.x = runSpeed * mult * Time.deltaTime;
        }
        transform.position += vec_move;
    }

    /**
     * Checks to see if the player is about to land. If they are, we need to switch to the landing
     * animation. Otherwise the landing animation (transitioning from being in the air) will
     * play while they're on the ground, which looks wrong.
     */
    private void checkAboutToLand()
    {
        if (onGround)
            return;

        // Don't perform the check ASAP, wait a bit. This prevents the anim from changing as soon as the player jumps.
        timeInAir += Time.deltaTime;
        if (timeInAir <= 0.5f)
            return;

        aboutToLand = Physics.Raycast(transform.position + vec_offset, Vector3.Normalize(vec_down + transform.forward), aboutToLandCheckDistance);
        if (aboutToLand)
        {
            anim.SetBool("InAir", false);
        }
    }

    /**
     * Checks to see if there is ground reasonably below the player. If so, they can jump and should
     * be in a running animation. If not, they should be in a falling animation.
     * Ground is checked via a Raycast. This lets us use any solid physics object as a suitable
     * ground!
     */
    private void checkForGround()
    {
        bool nextState = Physics.Raycast(transform.position + vec_offset, vec_down, groundCheckDistance);
        bool groundBuffer = jumpBufferTimer >= 0;
        if (!onGround && nextState)
        {
            jumpBufferTimer = jumpBuffer;
            airCharges = maxAirCharges;
            fm_air.Stop();
            fm_running.Play();
            Vector3 vel = rb.velocity;
            vel.y = 0;
            rb.velocity = vel;
            timeInAir = 0;
            aboutToLand = false;
        }
        else if (onGround && !nextState)
        {
            fm_air.Play();
            fm_running.Stop();
        }
        onGround = nextState;
        if (!hasDash && onGround)
        {
            hasDash = true;
        }

        // Although setting every update cycle, fixes edge cases where the player barely misses landing,
        // which sets inAir to false without ever actually landing, so we can't just set on the onground-offground
        // transition. TLDR: bad programming, but it works and other higher priorities.
        if (!aboutToLand && !groundBuffer)
        {
            anim.SetBool("InAir", !onGround);
            //anim.ResetTrigger("JumpPressed");
        }
    }
    
    private void checkJump()
    {
        bool groundBuffer = jumpBufferTimer >= 0;
        if (!onGround && jumpBufferTimer > 0)
        {
            jumpBufferTimer -= Time.deltaTime;
        }
        if (Input.GetButtonDown("Jump") && groundBuffer && airCharges > 0)
        {
            anim.SetTrigger("JumpPressed");
            rb.AddForce(vec_jump);
            fm_air.Play();
            fm_running.Stop();
            --airCharges;
        }
    }

    private void checkTurnInput()
    {
        wantsToTurnLeft = Input.GetAxis("Horizontal") < 0;
        wantsToTurnRight = Input.GetAxis("Horizontal") > 0;
    }

    /**
     * Checks to see if the player wants to turn in a certain direction or not.
     * Returns true if the player wants to turn in the direction specified.
     */
    public bool wantsToTurn(bool turnLeft)
    {
        if (!onGround)
            return false;

        return (turnLeft) ? wantsToTurnLeft : wantsToTurnRight;
    }

    /**
     * Turns the wolf so now they are running in a new direction. Additionally,
     * the wolf is snapped into place at the specified position. This is used to keep the
     * player centered on the track since turn commands are only issued from invisible
     * Turn gameobjects, which are placed on tracks at intersections/corners.
     */
    public void turn(Vector3 position, bool faceLeft)
    {
        Vector3 pos = new Vector3(position.x, 0, position.z);
        transform.position = pos;
        if (isRunningLeft == faceLeft)
            return; // we're already facing the new direction.

        float mult = faceLeft ? -1 : 1;
        transform.Rotate(vec_turn * mult);
        isRunningLeft = faceLeft;
        GameManager.i().cameraFollowing.SetOrientation(!faceLeft);
    }

    /**
     * Kill the wolf, warping them back to the last checkpoint, or Spawn if no checkpoint
     * has been touched yet.
     */
    public void kill()
    {
        GameManager.i().deaths += 1;

        GameObject checkpoint = GameManager.i().lastCheckpoint;
        if (checkpoint == null)
        {
            GameObject spawn = GameManager.i().spawnPoint;
            turn(spawn.transform.position, spawn.GetComponent<SpawnPoint>().faceLeft);
            return;
        }
        
        turn(checkpoint.transform.position, checkpoint.GetComponent<Checkpoint>().faceLeft);
    }

    /**
     * Disables the wolf and frees it of all collision. This is a handy effect for finishing
     * a level or touching an object that should kill the wolf.
     */
    public void phaseOut()
    {
        // disable gravity (honestly, all physics)
        rb.useGravity = false;
        coll.enabled = false;
        doMove = false;
        isPhasedOut = true;
        ghosting.enabled = false;

        // TODO create fancy shader effect to show the wolf actually phasing out.
    }

    /**
     * Undoes the effects caused by phaseOut().
     */
    public void phaseIn()
    {
        if (!isPhasedOut)
            return;

        rb.useGravity = true;
        coll.enabled = true;
        doMove = true;
    }

    /**
     * Lets the wolf move or not. If movement is true, then the wolf will move forward. If movement
     * is false, then the wolf will remain stationary, but still be affected by gravity.
     */
    public void allowMovement(bool movement)
    {
        doMove = movement;
    }

    /**
     * Lets the wolf completely pause, stopping all movement, physics, and animations. If isPaused is false,
     * then the wolf resumes movement, physics, and animations.
     */
    public void setPaused(bool setPaused)
    {
        if (!isPaused && setPaused)
        {
            rb.useGravity = false;
            unpause_velocity = rb.velocity;
            rb.velocity = new Vector3();
            coll.enabled = false;
            doMove = false;
            anim.speed = 0;
            isPaused = true;
        }
        else if (isPaused && !setPaused)
        {
            rb.useGravity = true;
            rb.velocity = unpause_velocity;
            coll.enabled = true;
            doMove = true;
            anim.speed = 1;
            isPaused = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + vec_offset, Vector3.Normalize(vec_down + transform.forward) * aboutToLandCheckDistance);
    }
}
