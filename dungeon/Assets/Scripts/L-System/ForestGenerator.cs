using UnityEngine;

public class ForestGenerator : MonoBehaviour
{
    [Header("Spawn Range")]
    public float xMin = 1;
    public float xMax = 199;
    public float zMin = 1;
    public float zMax = 199;

    [Header("Prefab Processing")]
    public int numTrees = 50;
    public GameObject[] treeToSpawn;
    public float rollMin = -25f;
    public float rollMax = 25f;
    
    void Start()
    {
        SpawnTrees();
    }

    void SpawnTrees()
    {
        for (int i = 0; i < numTrees; i++)
        {
            GameObject newTree = Instantiate(ReturnRandomPrefab(), new Vector3(GetRandomPoint(xMin, xMax), 0, GetRandomPoint(zMin, zMax)), Quaternion.identity);
            newTree.transform.rotation *= Quaternion.Euler(0, 0, GetRandomPoint(rollMin, rollMax)); 
        }
    }

    GameObject ReturnRandomPrefab()
    {
        return treeToSpawn[Random.Range(0,treeToSpawn.Length)];
    }

    float GetRandomPoint(float min, float max)
    {
        return Random.Range(min, max);
    }
}
