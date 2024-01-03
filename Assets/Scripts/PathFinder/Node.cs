using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Vector2Int Position;
    public int GCost; // Custo do caminho do ponto de in�cio at� este n�
    public int HCost; // Custo heur�stico (estimativa) do ponto at� o destino
    public int FCost { get { return GCost + HCost; } } // Custo total (G + H)
    public Node Parent; // N� anterior no caminho

    public Node(Vector2Int position)
    {
        Position = position;
    }
}
