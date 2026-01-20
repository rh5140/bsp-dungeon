using System.Collections.Generic;
using UnityEngine;

public class PartitionCell
{
    List<PartitionCell> _childrenNodeList;

    public Vector2Int TopLeftCorner { get; set; }
    public Vector2Int BottomRightCorner { get; set; }
    public PartitionCell Parent { get; set; }

    public List<PartitionCell> ChildrenNodeList { get => _childrenNodeList; }
    public bool IsLeaf { get => _childrenNodeList.Count == 0; }

    public int Width { get => (int)(BottomRightCorner.x - TopLeftCorner.x); }
    public int Height { get => (int)(BottomRightCorner.y - TopLeftCorner.y); }

    public bool WasSplitHorizontal { get; set; }

    public PartitionRoom Room { get; set; }

    public PartitionCell(Vector2Int topLeftCorner, Vector2Int bottomRightCorner, PartitionCell parent = null)
    {
        this.TopLeftCorner = topLeftCorner;
        this.BottomRightCorner = bottomRightCorner;
        this.Parent = parent;
        _childrenNodeList = new List<PartitionCell>();
    }

    public void AddChild(PartitionCell child)
    {
        _childrenNodeList.Add(child);
    }

    public void RemoveChild(PartitionCell child)
    {
        _childrenNodeList.Remove(child);
    }

    public override string ToString()
    {
        return "TL: " + TopLeftCorner + ", BR: " + BottomRightCorner + ", Width:" + Width + ", Height: " + Height;
    }
}
