using UnityEngine;

public class ForestGenerator : MonoBehaviour
{
    [Header("Spawn Range")]
    public float xMin = 1;
    public float xMax = 199;
    public float yMin = 1;
    public float yMax = 199;

    [Header("Prefab Processing")]
    public GameObject treeToSpawn;
    public float rollMin = -25f;
    public float rollMax = 0f;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
