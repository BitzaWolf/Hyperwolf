/**
 * Forces the camera to follow a Gameobject target, offset by a certain vector.
 * This script contains four separate vector values depending on the direction the player
 * is facing so the player can see appropriately far forward to dodge obstacles.
 */
using UnityEngine;

public class Follow : MonoBehaviour
{
    public GameObject target = null;

    // When the camera changes direction, interpolate the shift so it's not so sudden.
    [Tooltip("How long the camera should take to move.")]
    public float changePositionDuration = 2;
    [Tooltip("How to transition from one offset location to the other.")]
    public AnimationCurve shiftCurve;

    [Tooltip("The camera's offset from the target")]
    [SerializeField]
    private Vector3
        offsetUpRight,
        offsetUpLeft,
        offsetDownRight,
        offsetDownLeft;

    private Vector3 initialOffset, currentOffset, finalOffset;
    private Wolf.FacingDir offsetDirection;
    private float moveTimer;

	void Start ()
    {
        moveTimer = 0;
        offsetDirection = Wolf.FacingDir.UP_RIGHT;
        initialOffset = offsetUpRight;
        currentOffset = offsetUpRight;
        finalOffset = offsetUpRight;
    }
	
	void Update ()
    {
        if (target == null)
            return;
        
        if (moveTimer > 0)
            shiftCameraPosition();
        
        transform.position = target.transform.position - currentOffset;
	}

    private void shiftCameraPosition()
    {
        moveTimer -= Time.deltaTime;
        if (moveTimer < 0)
        {
            moveTimer = 0;
            currentOffset = finalOffset;
        }

        float val = shiftCurve.Evaluate(1 - (moveTimer / changePositionDuration));
        currentOffset = finalOffset * val + initialOffset * (1 - val);
    }

    /**
     * Changes the camera's offset from the target so that the camera shows area
     * forward of the direction being faced.
     * @param Wolf.FacingDir Direction camera should reference to offset from
     * the camera.
     */
    public void SetOrientation(Wolf.FacingDir newDirection)
    {
        if (offsetDirection == newDirection)
            return;

        moveTimer = changePositionDuration;
        initialOffset = currentOffset;
        offsetDirection = newDirection;
        
        switch(offsetDirection)
        {
            case Wolf.FacingDir.UP_RIGHT:   finalOffset = offsetUpRight; break;
            case Wolf.FacingDir.UP_LEFT:    finalOffset = offsetUpLeft; break;
            case Wolf.FacingDir.DOWN_RIGHT: finalOffset = offsetDownRight; break;
            case Wolf.FacingDir.DOWN_LEFT:  finalOffset = offsetDownLeft; break;
        }
    }
}
