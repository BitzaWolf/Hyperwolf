using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private static Color gizmoColor = new Color(221f / 255, 190f / 255, 64f / 255, 0.5f);

    public bool faceLeft = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            GameManager.i().lastCheckpoint = gameObject;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(transform.position, transform.localScale);
    }
}
