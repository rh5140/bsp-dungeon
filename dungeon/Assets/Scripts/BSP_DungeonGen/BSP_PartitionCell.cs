using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class PartitionCell : BSP_Node
{
    List<PartitionCell> _childrenNodeList;

    public PartitionCell Parent { get; set; }

    public List<PartitionCell> ChildrenNodeList { get => _childrenNodeList; }
    public bool IsLeaf { get => !_childrenNodeList.Any(); }

    public int Width { get => (int)(BottomRightCorner.x - TopLeftCorner.x); }
    public int Height { get => (int)(BottomRightCorner.y - TopLeftCorner.y); }

    public bool WasSplitHorizontal { get; set; }

    public PartitionRoom Room { get; set; }

    public PartitionCell(Vector2Int topLeftCorner, Vector2Int bottomRightCorner, PartitionCell parent = null) : base(topLeftCorner, bottomRightCorner)
    {
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
}
