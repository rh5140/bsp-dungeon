using UnityEngine;

public class PartitionRoom 
{
    public Vector2Int TopLeftCorner { get; set; }
    public Vector2Int BottomRightCorner { get; set; }

    public int Width { get => (int)(BottomRightCorner.x - TopLeftCorner.x); }
    public int Height { get => (int)(BottomRightCorner.y - TopLeftCorner.y); }

    public PartitionRoom(Vector2Int topLeftCorner, Vector2Int bottomRightCorner)
    {
        this.TopLeftCorner = topLeftCorner;
        this.BottomRightCorner = bottomRightCorner;
    }
}
