// https://github.com/SunnyValleyStudio/Unity_Procedural_Dungeon_binary_space_partitioning/blob/master/Version%202%20-%20Finished%20scripts/Scripts/CorridorNode.cs
// https://github.com/SunnyValleyStudio/Unity_Procedural_Dungeon_binary_space_partitioning/blob/master/Version%202%20-%20Finished%20scripts/Scripts/CorridorsGenerator.cs
using UnityEngine;

public class Corridor
{
    public Vector2Int TopLeftCorner { get; set; }
    public Vector2Int BottomRightCorner { get; set; }
    PartitionRoom left;
    PartitionRoom right;
    bool splitHorizontal;
    int width;

    public Corridor(PartitionRoom left, PartitionRoom right, bool splitHorizontal, int width=5)
    {
        this.left = left;
        this.right = right;

        this.splitHorizontal = splitHorizontal;
        this.width = width;
        CreateCorridor();
    }

    void CreateCorridor()
    {
        Debug.Log("CREATE CORRIDOR CALLED");
        Debug.Log("corridor width: " + width);
        if (splitHorizontal)
        {
            int distance = right.TopLeftCorner.y - left.BottomRightCorner.y;

            int leftBound = left.TopLeftCorner.x > right.TopLeftCorner.x ? left.TopLeftCorner.x : right.TopLeftCorner.x;
            int rightBound = left.BottomRightCorner.x > right.BottomRightCorner.x ? right.BottomRightCorner.x : left.BottomRightCorner.x;
            int x = (leftBound + rightBound) / 2;

            TopLeftCorner = new Vector2Int(x, left.BottomRightCorner.y);
            BottomRightCorner = new Vector2Int(x + width, right.TopLeftCorner.y);
        }
        else // left = top, right = bottom
        {
            int distance = left.BottomRightCorner.x - right.TopLeftCorner.x;

            int upperBound = left.TopLeftCorner.y > right.TopLeftCorner.y ? right.TopLeftCorner.y : left.TopLeftCorner.y;
            int lowerBound = left.BottomRightCorner.y > right.BottomRightCorner.y ? left.BottomRightCorner.y : right.BottomRightCorner.y;
            int y = (upperBound + lowerBound) / 2;

            TopLeftCorner = new Vector2Int(left.BottomRightCorner.x, y);
            BottomRightCorner = new Vector2Int(right.TopLeftCorner.x, y + width);
        }

        Debug.Log("TLC: " + TopLeftCorner + ", BRC: " + BottomRightCorner);
    }
}
