using UnityEngine;

public class Goal : MonoBehaviour
{
    private static Color gizmoColor = new Color(66f / 255, 244f / 255, 69f / 255, 0.5f);

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
            GameManager gm = GameManager.i();
            gm.triggerLevelEnd(nextLevel);
            gm.playerWolf.phaseOut();
            gm.cameraFollowing.target = gameObject;
        }
    }
}
