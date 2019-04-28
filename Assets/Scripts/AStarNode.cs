using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script based on sebastian Lague's Youtube tutorial for A*
public class AStarNode 
{
    public bool IsWalkable;
    public bool IsOccupied;
    public Vector3 WorldPosition;

    public int GridValX;
    public int GridValZ;

    public int GCost;
    public int HCost;

    public AStarNode Parent;

    public int FCost
    {
        get
        {
            return GCost + HCost;
        }
    }

    public AStarNode(bool walkable, Vector3 worldPos, int gridX, int gridZ)
    {
        IsWalkable = walkable;
        WorldPosition = worldPos;

        GridValX = gridX;
        GridValZ = gridZ;
    }
}
