using UnityEngine;

public class ReflectionFollow : MonoBehaviour
{
    public Camera cam;

    private Vector3 vec;

    void Start()
    {
        vec = new Vector3();
    }

    void Update ()
    {
        vec.Set(
            cam.transform.position.x,
            cam.transform.position.y * -1,
            cam.transform.position.z
        );
        transform.position = vec;
	}
}
