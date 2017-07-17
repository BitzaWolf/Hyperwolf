/**
 * Turn handles the logic of letting the player turn left or right down a track.
 * Attached to the gameobject is a trigger collider, on collision we check to see
 * if the player wants to turn or not. If they do, then we turn the player!
 */
using UnityEngine;

public class Turn : MonoBehaviour
{
    static Color gizmoColor = new Color(66f / 255, 244f / 255, 232f / 255, 0.5f);

    // Does this trigger turn the player left or right?
    public bool turnLeft = true;

    // Force the player to turn. Useful for situations with a corner. Not fun to fall off for no reason.
    public bool forceTurn = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Wolf wuff = GameManager.i().playerWolf;
            if (forceTurn || wuff.wantsToTurn(turnLeft))
            {
                wuff.turn(transform.position, turnLeft);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(transform.position, transform.localScale);
    }
}
