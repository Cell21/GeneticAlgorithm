using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Animal : MonoBehaviour
{
    public float moveSpeed = 0.25f;
    public float hunger = 100; // Initial hunger level, for example, 100
    public float hungerDecayRate = 5; // Amount of hunger to decay
    public float hungerDecayInterval = 2f; // Time in seconds between each hunger decay
    

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

    protected virtual void Update()
    {
      
    }


    protected IEnumerator FollowPath()
    {
        if (path != null) {
            foreach (Vector2Int tile in path)
            {
                Debug.Log("Movendo para o tile: " + tile);

                // Calcula a posição do mundo para o centro do tile
                Vector3 worldPosition = tileMap.GetCellCenterWorld(new Vector3Int(tile.x, tile.y, 0));

                // Move instantaneamente o coelho para a posição do tile
                transform.position = worldPosition;

                Debug.Log("Chegou ao tile: " + tile);

                // Aguarda um pouco antes de mover para o próximo tile
                yield return new WaitForSeconds(0.5f);
            }
        }
        else
        {
            Debug.Log("O path esta vazio");
        }

        Debug.Log("Chegou ao destino final.");
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
            yield return new WaitForSeconds(hungerDecayInterval);
            hunger -= hungerDecayRate;
            if (hunger < 0) hunger = 0; // Prevent hunger from going below zero
        }
    }
}
