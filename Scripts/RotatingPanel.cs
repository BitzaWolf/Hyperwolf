using UnityEngine;

public class RotatingPanel : MonoBehaviour
{
    private const float ROTATION_RATE = 40;
    private const float MOVE_SPEED = 2;

    [ExecuteInEditMode]
    void Update ()
    {
        transform.Rotate(new Vector3(0, ROTATION_RATE * Time.deltaTime, 0));
        transform.Translate(new Vector3(0, MOVE_SPEED * Time.deltaTime, 0));
	}
}
