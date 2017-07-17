using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    private static Color gizmoColor = new Color(66f / 255, 203f / 255, 244f / 255, 0.8f);
    private static float gizmoRadius = 5;
    public bool faceLeft = false;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, gizmoRadius);
    }
}
