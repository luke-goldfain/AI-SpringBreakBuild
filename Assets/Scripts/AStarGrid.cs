using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script based on Sebastian Lague's Youtube tutorial for A*
public class AStarGrid : MonoBehaviour
{
    public List<GameObject> Agents;

    public LayerMask UnwalkableMask;
    public Vector3 GridWorldSize;
    public float NodeRadius;
    AStarNode[,] grid;

    float nodeDiameter;
    int gridSizeX, gridSizeZ;

    public List<AStarNode> Path;

    private void Start()
    {
        nodeDiameter = NodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(GridWorldSize.x / nodeDiameter);
        gridSizeZ = Mathf.RoundToInt(GridWorldSize.z / nodeDiameter);

        Agents = new List<GameObject>();

        GameObject[] preys = GameObject.FindGameObjectsWithTag("Prey");
        GameObject[] preds = GameObject.FindGameObjectsWithTag("Predator");

        foreach(GameObject p in preys)
        {
            Agents.Add(p);
        }
        foreach(GameObject p in preds)
        {
            Agents.Add(p);
        }

        CreateGrid();
    }

    private void Update()
    {
        // Update occupied-ness of each node
        if (grid != null)
        {
            foreach (AStarNode n in grid)
            {
                n.IsOccupied = false;
            }

            foreach (GameObject a in Agents)
            {
                NodeFromWorldPoint(a.transform.position).IsOccupied = true;
            }
        }
    }

    private void CreateGrid()
    {
        grid = new AStarNode[gridSizeX, gridSizeZ];

        Vector3 worldBottomLeft = transform.position - Vector3.right * GridWorldSize.x / 2 - Vector3.forward * GridWorldSize.z / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + NodeRadius) + Vector3.forward * (z * nodeDiameter + NodeRadius);

                bool walkable = !(Physics.CheckSphere(worldPoint, NodeRadius, UnwalkableMask));

                grid[x, z] = new AStarNode(walkable, worldPoint, x, z);
            }
        }
    }

    public AStarNode NodeFromWorldPoint(Vector3 worldPos)
    {
        float percentX = (worldPos.x + GridWorldSize.x / 2) / GridWorldSize.x;
        float percentZ = (worldPos.z + GridWorldSize.z / 2) / GridWorldSize.z;
        percentX = Mathf.Clamp01(percentX);
        percentZ = Mathf.Clamp01(percentZ);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int z = Mathf.RoundToInt((gridSizeZ - 1) * percentZ);

        return grid[x, z];
    }

    public List<AStarNode> GetNeighbors(AStarNode node)
    {
        List<AStarNode> neighbors = new List<AStarNode>();

        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                if (x == 0 && z == 0) continue;

                int checkX = node.GridValX + x;
                int checkZ = node.GridValZ + z;

                if (checkX >= 0 && checkX < gridSizeX &&
                    checkZ >= 0 && checkZ < gridSizeZ)
                {
                    neighbors.Add(grid[checkX, checkZ]);
                }
            }
        }

        return neighbors;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, GridWorldSize);

        if (grid != null)
        {
            foreach (AStarNode n in grid)
            {
                if (n.IsOccupied) Gizmos.color = Color.cyan;
                else if (n.IsWalkable) Gizmos.color = Color.white;
                else Gizmos.color = Color.red;

                if(Path != null)
                {
                    if (Path.Contains(n))
                    {
                        Gizmos.color = Color.blue;
                    }
                }

                Gizmos.DrawWireCube(n.WorldPosition, Vector3.one * (nodeDiameter - .1f));
            }


        }
    }
}
