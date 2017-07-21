/**
 * This is the main player!
 */
using UnityEngine;

public class Wolf : MonoBehaviour
{
    public float runSpeed = 60;
    public float jumpForce = 13;
    public float dashLength = 0.5f;
    // How much faster should the player run when dashing?
    public float dashMultiplier = 2;
    // Direction player is facing. Semmantically easier than interpreting rotation info.
    public bool isRunningLeft = false;

    [FMODUnity.EventRef]
    public string runningSFX = "";
    private FMOD.Studio.EventInstance ev_running;
    [FMODUnity.EventRef]
    public string jumpingSFX = "";
    private FMOD.Studio.EventInstance ev_jumping;

    private Animator anim;
    private Vector3 vec_move, vec_jump, vec_down, vec_offset, vec_turn;
    private Rigidbody rb;
    private BoxCollider coll;
    private bool onGround = true,
        hasDash = true,
        wantsToTurnLeft = false,
        wantsToTurnRight = false,
        doMove = true,
        isPhasedOut = false;
    private float dashTimer = 0;

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
        anim = GetComponent<Animator>();
        ev_running = FMODUnity.RuntimeManager.CreateInstance(runningSFX);
        ev_jumping = FMODUnity.RuntimeManager.CreateInstance(jumpingSFX);
        ev_running.set3DAttributes(new FMOD.ATTRIBUTES_3D());
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(ev_running, transform, rb);
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(ev_jumping, transform, rb);
        ev_running.start(); // TODO need to set FMOD attenuation
        ev_jumping.start();
    }

    void Update()
    {
        moveWolf();
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
            GetComponent<Ghosting>().enabled = true;
            dashTimer = dashLength;
            hasDash = false;
        }
        if (dashTimer > 0)
        {
            mult = dashMultiplier;
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
                GetComponent<Ghosting>().enabled = false;
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
     * Checks to see if there is ground reasonably below the player. If so, they can jump and should
     * be in a running animation. If not, they should be in a falling animation.
     * Ground is checked via a Raycast. This lets us use any solid physics object as a suitable
     * ground!
     */
    private void checkForGround()
    {
        bool nextState = Physics.Raycast(transform.position + vec_offset, vec_down, 10f);
        if (!onGround && nextState)
        {
            ev_jumping.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            ev_running.start();
        }
        onGround = nextState;
        if (!hasDash && onGround)
        {
            hasDash = true;
        }

        // TODO minor optimization, only set the variable when the ground status -changes-.
        anim.SetBool("InAir", !onGround);
    }
    
    private void checkJump()
    {
        if (Input.GetButtonDown("Jump") && onGround)
        {
            anim.SetTrigger("JumpPressed");
            rb.AddForce(vec_jump);
            ev_running.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            ev_jumping.start();
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
}
