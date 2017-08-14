/**
 * Collectables are little trinkets the player can grab while playing through a level.
 * They don't server much of a purpose outside of a 100% completion.
 * 
 * Collectables spin when alive. When the player touches one, they explode into mini squares.
 * This requires a little bit of fancy work since we need to remove the mesh when a collectable is
 * grabbed, but we still need to play the particle effect until it ends. Once it ends, then the gameobject
 * can be deleted.
 */
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
            GameManager.i().collectablesGot += 1;
            GameManager.i().playerWolf.returnAirCharge();
            mesh.enabled = false;
            particles.Play();
            isAlive = false;
        }
    }
}
