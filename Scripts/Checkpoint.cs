/**
 *  Checkpoints are used in the game to warp the player back to if they die.
 */
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private static Color gizmoColor = new Color(221f / 255, 190f / 255, 64f / 255, 0.5f);

    public Wolf.FacingDir facingDirection = Wolf.FacingDir.UP_RIGHT;

    public Wolf.FacingDir getFacingDirection()
    {
        return facingDirection;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            GameManager.i().lastCheckpoint = gameObject;
            facingDirection = GameManager.i().playerWolf.getFacingDirection();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(transform.position, transform.localScale);
    }
}
