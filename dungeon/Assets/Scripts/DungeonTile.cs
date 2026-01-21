using UnityEngine;

public class DungeonTile
{
    public int x, y;
    public GameObject tile;

    public DungeonTile(int x, int y, GameObject tile)
    {
        this.x = x;
        this.y = y;
        this.tile = tile;
    }
}
