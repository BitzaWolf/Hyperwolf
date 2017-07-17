using UnityEngine;

public class Collectable : MonoBehaviour
{
    public float rotationRate;

    private bool isAlive = true;
    private Vector3 vec_rot;
    private MeshRenderer mesh;
    private ParticleSystem particles;

	void Start()
    {
        vec_rot = new Vector3(0, 0, rotationRate);
        mesh = GetComponent<MeshRenderer>();
        particles = GetComponent<ParticleSystem>();
	}
	
	void Update()
    {
        if (!isAlive && particles.isStopped)
        {
            Destroy(gameObject);
            return;
        }

        vec_rot.z = rotationRate;
        transform.Rotate(vec_rot);
	}

    private void OnTriggerEnter(Collider c)
    {
        if (c.gameObject.tag == "Player")
        {
            GameManager.i().levelMetadata.collectablesGot += 1;
            mesh.enabled = false;
            particles.Play();
            isAlive = false;
        }
    }
}
