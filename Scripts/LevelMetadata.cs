/**
 * Contains info about the level itself. There should be one per scene.
 */
using UnityEngine;

public class LevelMetadata : MonoBehaviour
{
    public string levelName;
    public int totalCollectables = 0;

    void Start()
    {
        totalCollectables = FindObjectsOfType<Collectable>().Length;
    }
}
