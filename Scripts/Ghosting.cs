using UnityEngine;

public class Ghosting : MonoBehaviour
{
    [Tooltip("The number of ghost images to create.")]
    public int numOfGhosts = 5;

    [Tooltip("How long each ghost image should last in seconds.")]
    public float ghostLifetime = 0.5f;

    [Range(0, 1), Tooltip("How transparent the end sprite should be.")]
    public float endAlpha = 0.0f;

    [Tooltip("Don't change!")]
    public GameObject ghostPrefab;

    [Tooltip("How long the effect lasts before turning itself off. 0 for never ending.")]
    public float effectTimer = 0;

    //private SpriteRenderer spriteRenderer;
    private float ghostSpawnRate;
    private float ghostSpawnTimer;
    private float effectReset;

    void Start ()
    {
        //spriteRenderer = GetComponent<SpriteRenderer>();
        ghostSpawnRate = ghostLifetime / numOfGhosts;
        ghostSpawnTimer = 0;
        effectReset = effectTimer;
        effectTimer = 0;
    }
	
	void Update ()
    {
        if (effectReset > 0)
        {
            if (effectTimer <= 0)
                return;

            effectTimer -= Time.deltaTime;
        }

        ghostSpawnTimer -= Time.deltaTime;
        if (ghostSpawnTimer > 0)
            return;

        ghostSpawnTimer += ghostSpawnRate;

        GameObject ghost = Instantiate(ghostPrefab, transform.position, transform.rotation);
        ghost.transform.localScale = transform.lossyScale;
        GhostingGhost ghostScript = ghost.GetComponent<GhostingGhost>();
        //MeshRenderer mesh = ghost.GetComponent<MeshRenderer>();

        //ghostRenderer.sprite = spriteRenderer.sprite;
        //ghostRenderer.color = spriteRenderer.color;
        ghostScript.timeRemainig = ghostLifetime;
        ghostScript.initTime = ghostLifetime;
        ghostScript.endAlpha = endAlpha;
    }
    
    public void Activate()
    {
        effectTimer = effectReset;
    }
}
