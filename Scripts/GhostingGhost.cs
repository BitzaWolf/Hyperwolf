using UnityEngine;

public class GhostingGhost : MonoBehaviour
{
    public Material mat;
    public float timeRemainig;
    public float initTime;
    private bool isDead = false;
    public bool isModel = false;
    public float endAlpha;

    private void Start()
    {
        if (isModel)
        {
            SkinnedMeshRenderer mesh = GetComponentInChildren<SkinnedMeshRenderer>();
            Material src = mesh.material;
            mat = new Material(src);
            mesh.material = mat;
        }
        else
        {
            MeshRenderer mesh = GetComponent<MeshRenderer>();
            Material src = mesh.material;
            mat = new Material(src);
            mesh.material = mat;
        }
    }

    private void Update()
    {
        if (isDead)
        {
            Destroy(gameObject);
            return;
        }

        timeRemainig -= Time.deltaTime;
        if (timeRemainig <= 0)
        {
            // run one more loop
            isDead = true;
            timeRemainig = 0;
        }

        // default linear interpolation
        //float alpha = timeRemainig / initTime;

        // exponential
        float proportion = (timeRemainig * timeRemainig) / (initTime * initTime);
        float alpha = endAlpha + (1 - endAlpha) * proportion;
        Color c = mat.color;
        c.a = alpha;
        mat.color = c;
    }
}
