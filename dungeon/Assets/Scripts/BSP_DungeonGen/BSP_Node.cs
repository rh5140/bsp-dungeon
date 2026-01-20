using UnityEngine;

public class BSP_Node
{
    public virtual Vector2Int TopLeftCorner { get; set; }
    public virtual Vector2Int BottomRightCorner { get; set; }

    public virtual int Width { get => (int)(BottomRightCorner.x - TopLeftCorner.x); }
    public virtual int Height { get => (int)(BottomRightCorner.y - TopLeftCorner.y); }

    public BSP_Node(Vector2Int topLeftCorner, Vector2Int bottomRightCorner)
    {
        this.TopLeftCorner = topLeftCorner;
        this.BottomRightCorner = bottomRightCorner;
    }
}
