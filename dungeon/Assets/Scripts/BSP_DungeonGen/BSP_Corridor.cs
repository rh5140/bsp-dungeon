// https://github.com/SunnyValleyStudio/Unity_Procedural_Dungeon_binary_space_partitioning/blob/master/Version%202%20-%20Finished%20scripts/Scripts/CorridorNode.cs
// https://github.com/SunnyValleyStudio/Unity_Procedural_Dungeon_binary_space_partitioning/blob/master/Version%202%20-%20Finished%20scripts/Scripts/CorridorsGenerator.cs
using UnityEngine;

public class Corridor : BSP_Node
{
    PartitionRoom left;
    PartitionRoom right;
    public bool splitHorizontal;
    int width;

    public Corridor(PartitionRoom left, PartitionRoom right, bool splitHorizontal, 
                    Vector2Int topLeftCorner, Vector2Int bottomRightCorner, int width=5) : base(topLeftCorner, bottomRightCorner)
    {
        this.left = left;
        this.right = right;

        this.splitHorizontal = splitHorizontal;
        this.width = width;
        CreateCorridor();
    }

    void CreateCorridor()
    {
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
    }
}
