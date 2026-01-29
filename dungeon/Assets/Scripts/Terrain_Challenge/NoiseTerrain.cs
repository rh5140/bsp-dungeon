// Using starter code from GSND6460 taught by Chris Wren
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Terrain))]
public class BasicPerlinTerrain : MonoBehaviour
{
    [Header("Terrain Resolution")]
    [Range(33, 1025)]
    public int resolution = 513;

    [Header("Noise Settings")]
    public float scale = 20f;
    [Range(1, 8)]
    public int octaves = 4;
    [Range(0.01f, 1f)]
    public float persistence = 0.5f;
    [Range(1f, 4f)]
    public float lacunarity = 2f;

    [Header("Height Settings")]
    [Range(0f, 1f)]
    public float heightScale = 0.2f;

    Terrain terrain;
    TerrainData terrainData;

    [Header("Lake Settings")]
    [Range(80, 150)]
    public int radius = 120;
    int centerX; 
    int centerY; 

    void Awake()
    {
        // Hardcoding lake for now
        centerX = radius * 2;
        centerY = radius * 3 / 2;
        Initialize();
        GenerateTerrain();
    }

    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            Initialize();
            GenerateTerrain();
        }
    }

    void Start()
    {
        GenerateTrees();
    }

    void Initialize()
    {
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;

        // Ensure valid resolution
        resolution = Mathf.ClosestPowerOfTwo(resolution - 1) + 1;
        terrainData.heightmapResolution = resolution;
    }

    void GenerateTerrain()
    {
        float[,] heights = new float[resolution, resolution];

        // Lake stuff
        // Currently hardcoded, but idea is to randomize + spawn dungeon prefab in the lake...
        var b = new bool[resolution - 1,resolution - 1];

        for (int x = 0; x < resolution; x++)
        {
            for (int z = 0; z < resolution; z++)
            {
                float amplitude = 1f;
                float frequency = 1f;
                float noiseHeight = 0f;

                for (int i = 0; i < octaves; i++)
                {
                    
                    float nx = x / (float)(resolution - 1);
                    float nz = z / (float)(resolution - 1);

                    float sampleX = nx * scale * frequency;
                    float sampleZ = nz * scale * frequency;

                    float perlin = Mathf.PerlinNoise(sampleX, sampleZ);
                    noiseHeight += perlin * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }
                // Lower if in lake area
                if ((Mathf.Pow(x - centerX, 2) + Mathf.Pow(z - centerY, 2) <= Mathf.Pow(radius,2)))
                {
                    heights[z, x] = -20;
                }
                else
                {
                    heights[z, x] = Mathf.Clamp01(noiseHeight * heightScale);
                }
            }
        }

        terrainData.SetHeights(0, 0, heights);
    }

    // https://theor.xyz/mapgen/random-2d-points/
    float Halton (int idx, int nbase)
    {
        float fraction = 1;
        float result = 0;
        while (idx > 0)
        {
            fraction  /= nbase;
            result += fraction * (idx % nbase);
            idx = ~~(idx / nbase);
        }
        return result;
    }

    void GenerateTrees()
    {
        for (int i = 0; i < 500; i++)
        {
            float treeX = Halton(i, 2);
            float treeY = Halton(i, 3);
            //float treeZ = terrainData.GetHeight(treeX * resolution, treeY * resolution);
            // Skip if in lake
            if ((Mathf.Pow(treeX * resolution - centerX, 2) + Mathf.Pow(treeY * resolution - centerY, 2) <= Mathf.Pow(radius,2)))
            {
                continue;
            }
            // https://stackoverflow.com/questions/53880451/add-trees-to-terrain-c-sharp-with-treeinstance
            TreeInstance treeTemp = new TreeInstance();
            treeTemp.position = new Vector3(treeX, 0, treeY);
            treeTemp.prototypeIndex = 0;
            treeTemp.widthScale = 1f;
            treeTemp.heightScale = 1f;
            treeTemp.color = Color.white;
            treeTemp.lightmapColor = Color.white;
            terrain.AddTreeInstance(treeTemp);
        }
    }

    void GenerateLake()
    {
        // https://discussions.unity.com/t/create-terrain-hole-at-specific-position-on-terrain-by-script/837754/6
        var b = new bool[resolution - 1,resolution - 1];
        
        // int centerX = Random.Range(radius,resolution - radius - 1);
        // int centerY = Random.Range(radius,resolution - radius - 1);
        int centerX = radius * 2;
        int centerY = radius * 3 / 2;

        for (var x = 0; x < resolution - 1; x++ )
            for (var y = 0; y < resolution - 1; y++)
                b[x, y] = !(Mathf.Pow(x - centerX, 2) + Mathf.Pow(y - centerY, 2) <= Mathf.Pow(radius,2));
        terrainData.SetHoles(0, 0, b);
    }
}