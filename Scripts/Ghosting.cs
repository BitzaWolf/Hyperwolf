/**
 * Ghosting handles a special effect where copies of a mesh are created at
 * regular intervals and they slowly become transparent and eventually invisible.
 */
using UnityEngine;

public class Ghosting : MonoBehaviour
{
    [Tooltip("The number of ghost images to create.")]
    public int numOfGhosts = 5;

    [Tooltip("How long each ghost image should last in seconds.")]
    public float ghostLifetime = 0.5f;

    [Range(0, 1), Tooltip("How transparent the end sprite should be.")]
    public float endAlpha = 0.0f;

    // The ghostPrefab contains the GhostingGhost script, which handles making a single
    //      mesh copy translucent and invisible.
    [Tooltip("Don't change!")]
    public GameObject ghostPrefab;
    
    // How much time should elapse between ghost spawns.
    private float ghostSpawnRate;

    // Time remaining until next ghost spawn.
    private float ghostSpawnTimer;

    void Start ()
    {
        ghostSpawnRate = ghostLifetime / numOfGhosts;
        ghostSpawnTimer = 0;
    }
	
	void Update ()
    {
        ghostSpawnTimer -= Time.deltaTime;
        if (ghostSpawnTimer > 0)
            return;

        // Time to spawn a new ghost!

        ghostSpawnTimer += ghostSpawnRate;

        GameObject ghost = Instantiate(ghostPrefab, transform.position, transform.rotation);
        ghost.transform.localScale = transform.lossyScale;
        GhostingGhost ghostScript = ghost.GetComponent<GhostingGhost>();
        
        ghostScript.timeRemainig = ghostLifetime;
        ghostScript.initTime = ghostLifetime;
        ghostScript.endAlpha = endAlpha;
    }
}
