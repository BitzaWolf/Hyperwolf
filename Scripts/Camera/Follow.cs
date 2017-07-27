/**
 * Forces the camera to follow a Gameobject target, offset by a certain vector if wanted.
 * This script contains two separate vector values depending on if the player is running left
 * or right. Since the player runs up-left or up-right, we need to move the camera so the player can
 * see appropriately far forward to dodge obstacles.
 */
using UnityEngine;

public class Follow : MonoBehaviour
{
    public GameObject target = null;
    public float distance = 10;
    // TODO replace with vector3. Also, enable this to work for a variety of screen sizes. Maybe use proportions?
    public float xOffsetL = 0, yOffsetL = 0;
    public float xOffsetR = 0, yOffsetR = 0;

    // When the camera changes direction, interpolate the shift so it's not so sudden.
    [ Tooltip("How long the camera should take to move.") ]
    public float changePositionDuration = 2;
    
    private bool
        useLeftOffset = true,
        moveCamera = false;
    private float moveTimer;
    private Vector3 offsetL, offsetR, curOffset;

	void Start ()
    {
        offsetL = transform.TransformVector(xOffsetL, yOffsetL, distance);
        offsetR = transform.TransformVector(xOffsetR, yOffsetR, distance);
        curOffset = offsetL;
    }
	
	void Update ()
    {
        if (target == null)
            return;

        if (moveCamera)
            updateCameraPosition();
        
        if (target != null)
        {
            Vector3 tarPos = target.transform.position;
            tarPos.y = 0;
            transform.position = tarPos - curOffset;
        }
	}

    private void updateCameraPosition()
    {
        moveTimer -= Time.deltaTime;
        if (moveTimer < 0)
            moveTimer = 0;

        float val = (moveTimer * moveTimer) / (changePositionDuration * changePositionDuration);

        if (useLeftOffset)
        {
            curOffset = offsetR * val + offsetL * (1 - val);
        }
        else
        {
            curOffset = offsetL * val + offsetR * (1 - val);
        }
    }

    public void SetOrientation(bool left)
    {
        if (left == useLeftOffset)
            return;

        useLeftOffset = left;
        moveCamera = true;
        moveTimer = changePositionDuration;
    }
}
