using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Animal : MonoBehaviour
{
    public float moveSpeed = 0.25f;
    public float fullness = 100; // Initial hunger level, for example, 100
    public float fulnessIncreaseRate = 5; // Amount of hunger to decay
    public float fullnessIncreaseInterval = 2f; // Time in seconds between each hunger decay
    

    public Tilemap tileMap;

    public  List<Vector2Int> path; // Caminho a seguir

    public bool pathfinderExecuted = false;

    protected virtual void Start()
    {

        // Encontra o Tilemap na cena
        tileMap = FindObjectOfType<Tilemap>();

        if (tileMap == null)
        {
            Debug.LogError("Tilemap não encontrado na cena!");
        }

        StartCoroutine(HungerDecayCoroutine());
    }

    protected IEnumerator FollowPath()
    {
        Debug.Log("Coroutine Started");
        pathfinderExecuted = true;
        if (path != null) {
            foreach (Vector2Int tile in path)
            {
                // Calcula a posição do mundo para o centro do tile
                Vector3 worldPosition = tileMap.GetCellCenterWorld(new Vector3Int(tile.x, tile.y, 0));

                // Move instantaneamente o coelho para a posição do tile
                transform.position = worldPosition;

                // Aguarda um pouco antes de mover para o próximo tile
                yield return new WaitForSeconds(0.5f);
            }
        }
        else
        {
            Debug.Log("O path esta vazio");
        }
        pathfinderExecuted = false;
        path.Clear();
    }

    public Vector3Int GetTilePosition()
    {
        return tileMap.WorldToCell(transform.position);
    }

    IEnumerator HungerDecayCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(fullnessIncreaseInterval);
            fullness -= fulnessIncreaseRate;
            if (fullness <= 0) fullness = 0; // Prevent hunger from going below zero
        }
    }
}
