using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarPathFinding : MonoBehaviour
{
    public GameObject PathTarget;

    public List<AStarNode> Path;

    AStarGrid grid;

    private void Awake()
    {
        grid = GameObject.FindGameObjectWithTag("AStar").GetComponent<AStarGrid>();
    }

    private void Update()
    {
        if (PathTarget != null)
        {
            FindPath(this.transform.position, PathTarget.transform.position);
        }
    }

    void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        AStarNode startNode = grid.NodeFromWorldPoint(startPos);
        AStarNode targetNode = grid.NodeFromWorldPoint(targetPos);

        List<AStarNode> openNodes = new List<AStarNode>();
        HashSet<AStarNode> closedNodes = new HashSet<AStarNode>();

        openNodes.Add(startNode);

        while(openNodes.Count > 0)
        {
            AStarNode currentNode = openNodes[0];

            for (int i = 1; i < openNodes.Count; i++)
            {
                if (openNodes[i].FCost < currentNode.FCost || (openNodes[i].FCost == currentNode.FCost && openNodes[i].HCost < currentNode.HCost))
                {
                    currentNode = openNodes[i];
                }
            }

            openNodes.Remove(currentNode);
            closedNodes.Add(currentNode);

            if (currentNode == targetNode)
            {
                RetracePath(startNode, targetNode);
                return;
            }

            foreach (AStarNode neighbor in grid.GetNeighbors(currentNode))
            {
                if (!neighbor.IsWalkable || closedNodes.Contains(neighbor)) continue;

                int newCostToNeighbor = currentNode.GCost + GetDistance(currentNode, neighbor);

                if (newCostToNeighbor < neighbor.GCost || !openNodes.Contains(neighbor))
                {
                    neighbor.GCost = newCostToNeighbor;
                    neighbor.HCost = GetDistance(neighbor, targetNode);

                    neighbor.Parent = currentNode;

                    if (!openNodes.Contains(neighbor))
                    {
                        openNodes.Add(neighbor);
                    }
                }
            }
        }
    }

    void RetracePath(AStarNode startNode, AStarNode endNode)
    {
        Path = new List<AStarNode>();
        AStarNode currentNode = endNode;

        while(currentNode != startNode)
        {
            Path.Add(currentNode);
            currentNode = currentNode.Parent;
        }

        Path.Reverse();

        grid.Paths.Add(Path);
    }

    int GetDistance(AStarNode nodeA, AStarNode nodeB)
    {
        int distX = Mathf.Abs(nodeA.GridValX - nodeB.GridValX);
        int distZ = Mathf.Abs(nodeA.GridValZ - nodeB.GridValZ);

        if (distX > distZ)
        {
            return (14 * distZ) + (10 * (distX - distZ));
        }

        return (14 * distX) + (10 * (distZ - distX));
    }
}
