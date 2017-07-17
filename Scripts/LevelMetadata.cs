using UnityEngine;

public class LevelMetadata : MonoBehaviour
{
    public string levelName;
    public int collectablesGot = 0;
    public int totalCollectables = 0;

    void Start()
    {
        totalCollectables = FindObjectsOfType<Collectable>().Length;
    }
}
