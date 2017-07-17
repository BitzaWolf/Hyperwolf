using UnityEngine;

public class Turn : MonoBehaviour
{
    static Color gizmoColor = new Color(66f / 255, 244f / 255, 232f / 255, 0.5f);

    public bool turnLeft = true;
    public bool forceTurn = false;
    private float cooldown = 0;

    private void Update()
    {
        if (cooldown > 0)
            cooldown -= Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (cooldown > 0)
        {
            cooldown -= Time.deltaTime;
            return;
        }

        if (other.gameObject.tag == "Player")
        {
            Wolf wuff = other.GetComponent<Wolf>(); // maybe change to singleton?
            if (forceTurn || wuff.wantsToTurn(turnLeft))
            {
                wuff.turn(transform.position, turnLeft);
                cooldown = 3;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(transform.position, transform.localScale);
    }
}
