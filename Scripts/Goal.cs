/**
 * Goal is the tirgger for the end of a level. Each goal stores which level it leads to next.
 * This opens up possibilites where a level could have multiple goals and lead to multiple different levels.
 */
using UnityEngine;

public class Goal : MonoBehaviour
{
    private static Color gizmoColor = new Color(66f / 255, 244f / 255, 69f / 255, 0.5f);

    // The next level (Unity Scene) this Goal takes the player to.
    public string nextLevel;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(transform.position, transform.localScale);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            performLevelEnd();
        }
    }

    private void performLevelEnd()
    {
        GameManager gm = GameManager.i();
        gm.triggerLevelEnd(nextLevel);
        gm.playerWolf.setToLevelEndState();
        gm.cameraFollowing.target = gameObject;
    }

    /**
     * Triggers this end-level goal code-wise.
     * aka, cheating.
     */
    public void cheatTrigger()
    {
        performLevelEnd();
    }
}
