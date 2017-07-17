using UnityEngine;

public class Wolf : MonoBehaviour
{
    public float runSpeed = 60;
    public float jumpForce = 13;
    public float dashLength = 0.5f;
    public float dashMultiplier = 2;
    public bool isRunningLeft = false;

    [FMODUnity.EventRef]
    public string runningSFX = "";
    private FMOD.Studio.EventInstance ev_running;
    [FMODUnity.EventRef]
    public string jumpingSFX = "";
    private FMOD.Studio.EventInstance ev_jumping;

    private const float FORCE_MULT = 1000000;

    private Animator anim;
    private Vector3 vec_move, vec_jump, vec_down, vec_offset, vec_turn;
    private Rigidbody rb;
    private BoxCollider coll;
    private bool onGround = true, hasDash = true,
        wantsToTurnLeft = false, wantsToTurnRight = false,
        doMove = true;
    private float dashTimer = 0;

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
        ev_running.start(); // TODO need to set attenuation
        ev_jumping.start();
    }

    void Update()
    {
        moveWolf();
        checkForGround();
        checkJump();
        checkTurnInput();

        if (Input.GetKeyDown(KeyCode.F2))
        {
            phaseOut();
        }


        // TODO use a killbox instead. I suppose we could keep this as a backup.
        if (transform.position.y <= -142)
        {
            kill();
        }
    }

    private void moveWolf()
    {
        if (!doMove)
            return;

        float mult = 1;

        if (Input.GetButtonDown("Dash") && hasDash)
        {
            GetComponent<Ghosting>().Activate();
            dashTimer = dashLength;
            hasDash = false;
        }
        if (dashTimer > 0)
        {
            mult = dashMultiplier;
            dashTimer -= Time.deltaTime;
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
            hasDash = true; // only -set- if currently has no dash.
        }

        anim.SetBool("InAir", !onGround); // Maybe change to only set when ground status changes?
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

    public bool wantsToTurn(bool turnLeft)
    {
        if (!onGround)
            return false;

        return (turnLeft) ? wantsToTurnLeft : wantsToTurnRight;
    }

    /**
     * Turns the wolf so now they are running in a new direction.
     */
    public void turn(Vector3 position, bool faceLeft)
    {
        Vector3 pos = new Vector3(position.x, 0, position.z);
        transform.position = pos;
        if (isRunningLeft == faceLeft)
            return; // no change

        float mult = faceLeft ? -1 : 1;
        transform.Rotate(vec_turn * mult);
        isRunningLeft = faceLeft;
        GameManager.i().cameraFollowing.SetOrientation(!faceLeft);
    }

    /**
     * Kill the wolf, warping them back to the last checkpoint
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

    public void phaseOut()
    {
        // disable gravity (honestly, all physics)
        rb.useGravity = false;
        coll.enabled = false;
        doMove = false;
        Vector3 force = new Vector3();
        if (isRunningLeft)
            force.z = 1e+07f;
        else
            force.x = 1e+07f;
        rb.AddForce(force);
        // activate fancy shader render thingy.
    }

    /**
     * Just undos the effects from phaseOut
     */
    public void phaseIn()
    {
        rb.useGravity = true;
        coll.enabled = true;
        doMove = true;
    }
}
