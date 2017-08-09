/**
 * A trigger-plane that instantly kills the player on touch.
 */
using UnityEngine;

public class KillPlane : MonoBehaviour
{
    public Mesh gizmoMesh;
    private static Color gizmoColor = new Color(1, 0, 0, 0.6f);

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawMesh(gizmoMesh, transform.position, transform.rotation, transform.lossyScale);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            GameManager.i().playerWolf.kill();
        }
    }
}
