using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BSP_Grammar : MonoBehaviour
{
    /*  Pseudocode from pcgbook.com
        Chapter 3: Constructive generation methods for dungeons and levels
        by Noor Shaker, Antonios Liapis, Julian Togelius, Ricardo Lopes, and Rafael Bidarra
        1: start with the entire dungeon area (root node of the BSP tree)
        2: divide the area along a horizontal or vertical line
        3: select one of the two new partition cells
        4: if this cell is bigger than the minimal acceptable size:
        5: go to step 2 (using this cell as the area to be divided)
        6: select the other partition cell, and go to step 4
        7: for every partition cell:
        8: create a room within the cell by randomly choosing two points (top left and bottom right) within its boundaries
        9: starting from the lowest layers, draw corridors to connect rooms corresponding to children of the same parent
    */

    [Header("Dungeon Parameters")]
    [SerializeField] int _dungeonWidth;
    [SerializeField] int _dungeonHeight;
    [SerializeField] int _minCellWidth;
    [SerializeField] int _minCellHeight;
    [SerializeField] int _offset;

    [Header("Random Seed")]
    [SerializeField] bool _useRandomSeed = true;
    [SerializeField] int _seed = 0;

    [Header("Materials")]
    [SerializeField] Material _floorMaterial;

    [Header("Prefabs")]
    [SerializeField] GameObject tilePrefab;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject startPrefab;
    [SerializeField] GameObject bossPrefab;
    [SerializeField] GameObject exitPrefab;
    [SerializeField] GameObject doorPrefab;
    [SerializeField] GameObject breakablePrefab;
    [SerializeField] GameObject lootPrefab;

    [SerializeField] GameObject enemyPrefab;
    [SerializeField] GameObject enemy2Prefab;
    [SerializeField] GameObject enemy3Prefab;

    [SerializeField] GameObject otherPrefab;

    List<BSP_Node> _nodes;
    List<BSP_Node> _rooms;
    List<BSP_Node> _corridors;
    PartitionCell rootNode;

    List<PartitionCell> _cells;

    List<DungeonTile> placedTiles;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_useRandomSeed)
        {
            _seed = System.Environment.TickCount;
        }
        Random.InitState(_seed);
        CreateDungeon();
    }

    void CreateDungeon()
    {
        Vector2Int rootBottomRight = new Vector2Int(_dungeonWidth, _dungeonHeight); // check if 1 off errors later
        rootNode = new PartitionCell(Vector2Int.zero, rootBottomRight); 

        _nodes = new List<BSP_Node>();
        _cells = new List<PartitionCell>();
        _rooms = new List<BSP_Node>();
        _corridors = new List<BSP_Node>();
        placedTiles = new List<DungeonTile>();

        Partition(rootNode);
        CreateRooms(rootNode);
        CreateParentRooms(rootNode);
        CreateCorridors(rootNode);
        // CreateFloorMesh();
        CreateFloorTiles();
        RemoveInteriorWalls();

        SetUpDungeon();
    }

    void SetUpDungeon()
    {
        int numRooms = _rooms.Count();
        // First room is entrance / player spawn point
        Instantiate(playerPrefab, _rooms[0].Center, Quaternion.identity);
        Instantiate(startPrefab, _rooms[0].Center,Quaternion.identity);

        // Initial implementation was a little similar to grammar already...
        for (int i = 1; i < numRooms; i++)
        {
            // Enforce penultimate room has boss
            if (i == numRooms - 2)
            {
                Instantiate(bossPrefab, _rooms[i].Center, Quaternion.identity, transform);
            }
            // Enforce last room has exit
            else if (i == numRooms - 1)
            {
                Instantiate(exitPrefab, _rooms[i].Center, exitPrefab.transform.rotation, transform);
            }
            else if (_cells[i].IsLeaf) // again very awkward using separate list -- pretty sure i could do it all with the same list... i think cells != rooms though
            {
                int rand = Random.Range(0,5);
                if (rand == 1) // 20% chance of tiny enemy instead of treasure
                {
                    Instantiate(otherPrefab, _rooms[i].Center, Quaternion.identity,transform);
                }
                else PopulateWithLoot(_rooms[i]);
            }
            else
            {
                PopulateEnemiesBasedOnDistance(_rooms[i]);
            }
        }

        foreach (Corridor corridor in _corridors)
        {
            PopulateCorridor(corridor);
        }
    }

    void PopulateEnemiesBasedOnDistance(BSP_Node node)
    {
        float distance = Vector2.Distance(node.Center, _rooms[0].Center);
        if (distance > 50) // VERY ARBITRARY distance
        {
            int rand = Random.Range(0,100);
            if (rand < 40) // 40% medium enemy
            {
                Instantiate(enemy2Prefab, node.Center, Quaternion.identity, transform);
            }
            else // 60% big enemy
            {
                Instantiate(enemy3Prefab, node.Center, Quaternion.identity, transform);
            }
        }
        else if (distance > 10) // middle distance
        {
            int rand = Random.Range(0,100);
            if (rand < 40) // 40% small enemy
            {
                Instantiate(enemyPrefab, node.Center, Quaternion.identity, transform);
            }
            else if (rand < 90) // 50% medium enemy
            {
                Instantiate(enemy2Prefab, node.Center, Quaternion.identity, transform);
            }
            else // 10% big enemy
            {
                Instantiate(enemy3Prefab, node.Center, Quaternion.identity, transform);
            }
        }
        else
        {
            Instantiate(enemyPrefab, node.Center, Quaternion.identity, transform);
        }
    }

    void PopulateRoom(BSP_Node node)
    {
        int rand = Random.Range(0,6);
        if (rand > 2)
        {
            Instantiate(enemyPrefab, node.Center, Quaternion.identity, transform);
        }
        else if (rand == 1)
        {
           Instantiate(lootPrefab, node.Center, Quaternion.identity, transform);
        }
        else
        {
            Instantiate(enemyPrefab, node.Center, Quaternion.identity, transform);
        }
    }

    void PopulateWithLoot(BSP_Node node)
    {
        int rand = Random.Range(0,10);
        GameObject loot = Instantiate(lootPrefab, node.Center, Quaternion.identity, transform);
        if (rand == 1) // 10% chance of being big treasure
        {
            loot.transform.localScale = new Vector3(2,2,2);
        }
    }

    void PopulateCorridor(BSP_Node corridor)
    {
        int rand = Random.Range(0,10);
        switch (rand)
        {
            case 0:
                break;
            case 1: // breakable
                GenerateDoors(corridor, breakablePrefab);
                // Instantiate(lootPrefab, corridor.Center, Quaternion.identity);
                break;
            default:
                GenerateDoors(corridor, doorPrefab);
                break;
        }
        return;
    }

    void GenerateDoors(BSP_Node corridor, GameObject door)
    {
        Corridor corr = (Corridor) corridor;
        // need to check which direction the corridor is in
        if (corr.splitHorizontal)
        {
            Instantiate(door, new Vector3(corridor.TopLeftCorner.x + 2.5f, 2f, corridor.TopLeftCorner.y), Quaternion.identity); // super hardcoded sorry
            Instantiate(door, new Vector3(corridor.BottomRightCorner.x - 2.5f, 2f, corridor.BottomRightCorner.y), Quaternion.identity);
        }
        else
        {
            Instantiate(door, new Vector3(corridor.TopLeftCorner.x, 2f, corridor.TopLeftCorner.y + 2.5f), Quaternion.Euler(0,90,0)); // super hardcoded sorry
            Instantiate(door, new Vector3(corridor.BottomRightCorner.x, 2f, corridor.BottomRightCorner.y - 2.5f), Quaternion.Euler(0,90,0));
        }
    }

    void CreateFloorTiles()
    {
        List<BSP_Node> floorMesh = _rooms.Concat(_corridors).ToList();
        foreach (BSP_Node node in floorMesh)
        {
            GameObject parentGO = new GameObject("Node | " + node);
            parentGO.transform.parent = this.transform;

            for (int x = node.TopLeftCorner.x; x < node.Width + node.TopLeftCorner.x; x++)
            {
                for (int y = node.TopLeftCorner.y; y < node.Height + node.TopLeftCorner.y; y++)
                {
                    if (HasTile(x, y)) 
                    {
                        continue;
                    }
                    GameObject tile = Instantiate(tilePrefab, new Vector3(x, 0, y), Quaternion.identity);
                    tile.transform.parent = parentGO.transform;
                    placedTiles.Add(new DungeonTile(x, y, tile));
                }
            }
        }
    }

    void RemoveInteriorWalls()
    {
        foreach (DungeonTile tile in placedTiles)
        {
            Transform n = tile.tile.transform.Find("Wall_N");
            Transform s = tile.tile.transform.Find("Wall_S");
            Transform e = tile.tile.transform.Find("Wall_E");
            Transform w = tile.tile.transform.Find("Wall_W");

            if (HasTile(tile.x, tile.y + 1)) Destroy(n?.gameObject);
            if (HasTile(tile.x, tile.y - 1)) Destroy(s?.gameObject);
            if (HasTile(tile.x + 1, tile.y)) Destroy(e?.gameObject);
            if (HasTile(tile.x - 1, tile.y)) Destroy(w?.gameObject);
        }
    }

    bool HasTile(int x, int y)
    {
        return placedTiles.Exists(t => t.x == x && t.y == y);
    }

    void CreateFloorMesh()
    {
        List<BSP_Node> floorMesh = _rooms.Concat(_corridors).ToList();
        foreach (BSP_Node node in floorMesh)
        {
            CreateMesh(node);
        }
    }

    void Partition(PartitionCell node)
    {
        _nodes.Add(node);
        _cells.Add(node); // Really awkward to duplicate code.. but too tired to think about how to use my inheritance smartly
        if (node.Width <= (_minCellWidth * 2 + _offset * 2) && node.Height <= (_minCellHeight * 2 + _offset * 2))
        {
            return;
        }

        bool divideHorizontal = Random.value > 0.5; // i think i swapped "horizontal" and "vertical"

        PartitionCell left, right;

        if ((divideHorizontal && node.Height > (_minCellHeight * 2 + _offset * 2)) || node.Width < (_minCellWidth * 2 + _offset * 2)) // width
        // split with horizontal line -> top/bottom partitions (left = top, right = bottom)
        {
            node.WasSplitHorizontal = true;
            int y = Random.Range(_minCellHeight, node.Height - _minCellHeight);
            left = new PartitionCell(node.TopLeftCorner, new Vector2Int(node.BottomRightCorner.x, node.TopLeftCorner.y + y), node);
            right = new PartitionCell(new Vector2Int(node.TopLeftCorner.x, node.TopLeftCorner.y + y), node.BottomRightCorner, node);
        }
        else // split with vertical line -> left/right partitions
        {
            node.WasSplitHorizontal = false;
            int x = Random.Range(_minCellWidth, node.Width - _minCellWidth);
            left = new PartitionCell(node.TopLeftCorner, new Vector2Int(node.TopLeftCorner.x + x, node.BottomRightCorner.y), node);
            right = new PartitionCell(new Vector2Int(node.TopLeftCorner.x + x, node.TopLeftCorner.y), node.BottomRightCorner, node);
        }

        node.AddChild(left);
        node.AddChild(right);

        Partition(left);
        Partition(right);
    }

    void CreateRooms(PartitionCell node)
    {
        if (!node.IsLeaf)
        {
            CreateRooms(node.ChildrenNodeList[0]);
            CreateRooms(node.ChildrenNodeList[1]);
            return;
        }
        int leftBound = node.TopLeftCorner.x + _offset;
        int rightBound = node.BottomRightCorner.x - _offset;
        int upperBound = node.TopLeftCorner.y + _offset;
        int lowerBound = node.BottomRightCorner.y - _offset;

        Vector2Int topLeft = new Vector2Int(Random.Range(leftBound, leftBound + _offset), Random.Range(upperBound, upperBound + _offset)); 
        Vector2Int bottomRight = new Vector2Int(Random.Range(topLeft.x + _minCellWidth, rightBound), Random.Range(topLeft.y + _minCellHeight, lowerBound));

        node.Room = new PartitionRoom(topLeft, bottomRight);
        _rooms.Add(node.Room);
    }

    void CreateParentRooms(PartitionCell node)
    {
        if (node.Room != null)
        {
            return;
        }
        PartitionCell left = node.ChildrenNodeList[0];
        PartitionCell right = node.ChildrenNodeList[1];

        node.Room = GetLowerRoom(node);

        CreateParentRooms(left);
        CreateParentRooms(right);
    }

    // Not really correct because it always chooses the "left" node even if it's not the right one for the corridor
    PartitionRoom GetLowerRoom(PartitionCell node) 
    {
        if (node.Room != null)
        {
            return node.Room;
        }
        
        PartitionCell left = node.ChildrenNodeList[0];
        PartitionCell right = node.ChildrenNodeList[1];

        if (left.Room != null)
        {
            return left.Room;
        }
        else if (right.Room != null)
        {
            return right.Room;
        }
        else
        {
            return GetLowerRoom(left);
        }
    }

    void CreateCorridors(PartitionCell node)
    {
        if (node.IsLeaf)
        {
            return;
        }
        PartitionCell left = node.ChildrenNodeList[0];
        PartitionCell right = node.ChildrenNodeList[1];

        if (left.Room != null && right.Room != null)
        {
            Corridor corridor = new Corridor(left.Room, right.Room, node.WasSplitHorizontal, node.TopLeftCorner, node.BottomRightCorner);
            _corridors.Add(corridor);
        }

        CreateCorridors(left);
        CreateCorridors(right);
    }

    void CreateMesh(BSP_Node room) // Mesh generation largely referenced from https://github.com/SunnyValleyStudio/Unity_Procedural_Dungeon_binary_space_partitioning/blob/master/Version%202%20-%20Finished%20scripts/Scripts/DungeonCreator.cs
    {
        Vector2Int bottomLeftCorner = new Vector2Int(room.TopLeftCorner.x, room.BottomRightCorner.y);
        Vector2Int bottomRightCorner = new Vector2Int(room.BottomRightCorner.x, room.BottomRightCorner.y);
        Vector2Int topLeftCorner = new Vector2Int(room.TopLeftCorner.x, room.TopLeftCorner.y);
        Vector2Int topRightCorner = new Vector2Int(room.BottomRightCorner.x, room.TopLeftCorner.y);

        Vector3 bottomLeftV = new Vector3(bottomLeftCorner.x, 0, bottomLeftCorner.y);
        Vector3 bottomRightV = new Vector3(topRightCorner.x, 0, bottomLeftCorner.y);
        Vector3 topLeftV = new Vector3(bottomLeftCorner.x, 0, topRightCorner.y);
        Vector3 topRightV = new Vector3(topRightCorner.x, 0, topRightCorner.y);

        Vector3[] vertices = new Vector3[]
        {
            topLeftV,
            topRightV,
            bottomLeftV,
            bottomRightV
        };

        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
        }

        int[] triangles = new int[]
        {
            0,
            1,
            2,
            2,
            1,
            3
        };
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        GameObject dungeonFloor = new GameObject("TL: " + room.TopLeftCorner + ", BR: " + room.BottomRightCorner, typeof(MeshFilter), typeof(MeshRenderer));

        dungeonFloor.transform.position = Vector3.zero;
        dungeonFloor.transform.localScale = Vector3.one;
        dungeonFloor.GetComponent<MeshFilter>().mesh = mesh;
        dungeonFloor.GetComponent<MeshRenderer>().material = _floorMaterial;
        dungeonFloor.transform.parent = transform;
    }

}
