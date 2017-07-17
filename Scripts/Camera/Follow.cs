using UnityEngine;

public class Follow : MonoBehaviour
{
    public GameObject target = null;
    public float distance = 10;
    public float xOffsetL = 0, yOffsetL = 0;
    public float xOffsetR = 0, yOffsetR = 0;
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
        
        transform.position = target.transform.position - curOffset;
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
