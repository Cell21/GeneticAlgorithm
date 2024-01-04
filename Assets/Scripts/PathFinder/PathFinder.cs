using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using System.IO;

public class PathFinder : MonoBehaviour
{
    private static PathFinder _instance;
    public static PathFinder Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PathFinder>();
            }
            return _instance;
        }
    }

    private Dictionary<Vector2Int, Node> nodes = new Dictionary<Vector2Int, Node>();
    

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
    {
        Debug.Log($"FindPath called with start: {start}, end: {end}");
        InitializeNodes();
        Node startNode = GetOrCreateNode(start);
        Node endNode = GetOrCreateNode(end);

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost < currentNode.FCost ||
                    (openSet[i].FCost == currentNode.FCost && openSet[i].HCost < currentNode.HCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode.Position == end)
            {
                return RetracePath(startNode, endNode);
            }

            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                if (closedSet.Contains(neighbor) || !IsTileWalkable(neighbor.Position))
                {
                    continue;
                }

                int newCost = currentNode.GCost + GetDistance(currentNode.Position, neighbor.Position);
                if (newCost < neighbor.GCost || !openSet.Contains(neighbor))
                {
                    neighbor.GCost = newCost;
                    neighbor.HCost = GetDistance(neighbor.Position, end);
                    neighbor.Parent = currentNode;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        return new List<Vector2Int>(); // Caminho não encontrado
    }

    private void InitializeNodes()
    {
        Debug.Log("Initializing nodes");
        nodes.Clear(); // Clear the dictionary

        // Iterate through the grid and create nodes for walkable tiles
        for (int x = 0; x < WalkerGenerator.Instance.mapWidth; x++)
        {
            for (int y = 0; y < WalkerGenerator.Instance.mapHeight; y++)
            {
                Vector2Int position = new Vector2Int(x, y);
                if (IsTileWalkable(position))
                {
                    Node newNode = new Node(position);
                    nodes.Add(position, newNode);
                }
            }
        }
        Debug.Log($"Created nodes for {nodes.Count} walkable tiles");
    }

    private Node GetOrCreateNode(Vector2Int position)
    {
        if (nodes.TryGetValue(position, out Node node))
        {
            Debug.Log($"Retrieved existing node at position {position}");
            return node;
        }

        Debug.Log($"Creating new node at position {position}");
        Node newNode = new Node(position);
        nodes.Add(position, newNode);
        return newNode;
    }

    private List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighborPosition = node.Position + direction;
            if (nodes.TryGetValue(neighborPosition, out Node neighborNode))
            {
                neighbors.Add(neighborNode);
            }
        }

        return neighbors;
    }

    public bool IsTileWalkable(Vector2Int position)
    {
        if (position.x < 0 || position.x >= WalkerGenerator.Instance.mapWidth ||
            position.y < 0 || position.y >= WalkerGenerator.Instance.mapHeight)
        {
            return false;
        }

        return WalkerGenerator.Instance.gridHandler[position.x, position.y] == WalkerGenerator.GridTiles.FLOOR;
    }

    private List<Vector2Int> RetracePath(Node startNode, Node endNode)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Node currentNode = endNode;

        while (currentNode != null && currentNode != startNode)
        {
            path.Add(currentNode.Position);
            Debug.Log("No em que estou" + currentNode.Position);
            currentNode = currentNode.Parent;
        }

        path.Reverse();
        return path;
    }

    private int GetDistance(Vector2Int a, Vector2Int b)
    {
        int distX = Mathf.Abs(a.x - b.x);
        int distY = Mathf.Abs(a.y - b.y);
        return distX + distY;
    }
}