using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Vector2Int Position;
    public int GCost; // Custo do caminho do ponto de início até este nó
    public int HCost; // Custo heurístico (estimativa) do ponto até o destino
    public int FCost { get { return GCost + HCost; } } // Custo total (G + H)
    public Node Parent; // Nó anterior no caminho

    public Node(Vector2Int position)
    {
        Position = position;
    }
}
