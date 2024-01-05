using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bunny : Animal
{
    //Estados do coelho
    public enum BunnyState
    {
        Exploring,
        Hungry,
        Eating,
        SearchingForMatingPartner
    }
    private BunnyState currentState;

    //Variaveis da exploração
    private HashSet<Vector2Int> visitedTiles = new HashSet<Vector2Int>();

    //Variaveis de Comer
    private bool bushFound = false;
    private Vector2Int bushPosition;
    private Bush bush;
    private bool MovingToBush = false;
    
    
    // Start is called before the first frame update
    protected new void Start()
    {
        base.Start();
        ChangeState(BunnyState.Exploring);
    }

    // Update is called once per frame
    protected void Update()
    {

        CheckFullnessAndUpdateState();
        
        if (!base.pathfinderExecuted) {
            switch (currentState)
            {
                case BunnyState.Exploring:
                    UpdateExploring();
                    break;
                case BunnyState.Hungry:
                    UpdateHungry();
                    break;
                case BunnyState.SearchingForMatingPartner:
                    UpdateSearchingForMatingPartner();
                    break;
                case BunnyState.Eating:
                    StartCoroutine(EatBush());
                    break;
            }
        }
    }

    //-------------------------------------------------------------EXPLORING CODE----------------

    private void UpdateExploring()
    {
        Vector3Int currentTile = base.GetTilePosition();
        Vector2Int currentTileConverted = new Vector2Int(currentTile.x, currentTile.y);

        List<Vector2Int> adjacentTiles = GetAdjacentTiles(currentTileConverted);
        Vector2Int nextTile;

        if (adjacentTiles.Count > 0)
        {
            // If there are unvisited adjacent tiles, choose one
            nextTile = ChooseNextTile(adjacentTiles, currentTileConverted);
        }
        else
        {
            // No unvisited adjacent tiles, choose a random adjacent tile
            nextTile = ChooseRandomAdjacentTile(currentTileConverted);
        }

        // Only update path and start coroutine if the next tile is different
        if (nextTile != currentTileConverted)
        {
            base.path = new List<Vector2Int> { nextTile };
            StartCoroutine(FollowPath());
            visitedTiles.Add(nextTile); // Mark this tile as visited
        }
    
    }

    private List<Vector2Int> GetAdjacentTiles(Vector2Int currentTile)
    {
        List<Vector2Int> adjacentTiles = new List<Vector2Int>();

        // Directions: up, down, left, right
        Vector2Int[] directions = new Vector2Int[]
        {
        new Vector2Int(0, 1),  // up
        new Vector2Int(0, -1), // down
        new Vector2Int(-1, 0), // left
        new Vector2Int(1, 0)   // right
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int adjacentTile = currentTile + dir;
            if (PathFinder.Instance.IsTileWalkable(adjacentTile) && !visitedTiles.Contains(adjacentTile))
            {
                adjacentTiles.Add(adjacentTile);
            }
        }

        return adjacentTiles;
    }

    private Vector2Int ChooseNextTile(List<Vector2Int> adjacentTiles, Vector2Int currentTile)
    {
        if (adjacentTiles.Count == 0) return currentTile; // Stay in place if no unvisited options

        // Randomly select an adjacent tile
        int randomIndex = Random.Range(0, adjacentTiles.Count);
        return adjacentTiles[randomIndex];
    }

    private Vector2Int ChooseRandomAdjacentTile(Vector2Int currentTile)
    {
        // Get all adjacent tiles regardless of visited status
        List<Vector2Int> allAdjacentTiles = GetAllAdjacentTiles(currentTile);
        if (allAdjacentTiles.Count == 0) return currentTile; // No adjacent tiles, stay in place

        // Randomly select any adjacent tile
        int randomIndex = Random.Range(0, allAdjacentTiles.Count);
        return allAdjacentTiles[randomIndex];
    }

    private List<Vector2Int> GetAllAdjacentTiles(Vector2Int currentTile)
    {
        List<Vector2Int> adjacentTiles = new List<Vector2Int>();
        // Directions: up, down, left, right
        Vector2Int[] directions = new Vector2Int[]
        {
        new Vector2Int(0, 1),  // up
        new Vector2Int(0, -1), // down
        new Vector2Int(-1, 0), // left
        new Vector2Int(1, 0)   // right
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int adjacentTile = currentTile + dir;
            if (PathFinder.Instance.IsTileWalkable(adjacentTile))
            {
                adjacentTiles.Add(adjacentTile);
            }
        }

        return adjacentTiles;
    }

    //-------------------------------------------------------------------------------------------
    //---------------------------------------Search for food-------------------------------------

    private void UpdateHungry() {
        //Continue a explorar à procura de um arbusto
        if (!bushFound && !MovingToBush)
        {
            UpdateExploring();
        }
        else
        {
            MoveToBush();
            ChangeState(BunnyState.Eating);
            bushFound = false;
        }
    }

    private void MoveToBush()
    {
        MovingToBush = true;
        Vector3Int currentTile = base.GetTilePosition();
        Vector2Int currentTileConverted = new Vector2Int(currentTile.x, currentTile.y);

        base.path = PathFinder.Instance.FindPath(currentTileConverted, bushPosition);

        if (base.path == null)
        {
            Debug.Log("Path was null");
        }
        else
        {
            Debug.Log("Path found:");
            foreach (var tile in base.path)
            {
                Debug.Log("Tile: " + tile);
            }
        }
        StartCoroutine(FollowPath());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (currentState == BunnyState.Hungry && !MovingToBush)
        {
            bush = collision.GetComponent<Bush>();
            Vector3Int bushPositionNotConverted = tileMap.WorldToCell(collision.transform.position);
            // Assign the grid coordinates to bushPosition
            bushPosition = new Vector2Int(bushPositionNotConverted.x, bushPositionNotConverted.y);

            bushFound = true;
        }
    }

    //---------------------------------------------------------------------------------------------------------------
    //---------------------------------------------EATING------------------------------------------------------------

    private IEnumerator EatBush()
    {
        while (bush != null && fullness < 100 && bush.foodAmount > 0)
        {
            bush.Eat(1); // The bunny eats a portion of the bush
            fullness += 5; // Increase the bunny's fullness

            yield return new WaitForSeconds(1); // Wait for a second before eating again
        }

        MovingToBush = false;
        ChangeState(BunnyState.Exploring);
        bush = null; // Reset the target bush
    }

    //---------------------------------------------------------------------------------------------------------------
    //----------------------------------------------Change States---------------------------------------------
    private void CheckFullnessAndUpdateState()
    {
        if (fullness > 50 && currentState != BunnyState.Exploring)
        {
            ChangeState(BunnyState.Exploring);
        }
        else if (fullness < 40 && currentState != BunnyState.Hungry && currentState != BunnyState.Eating)
        {
            ChangeState(BunnyState.Hungry);
        }
    }

    private void UpdateSearchingForMatingPartner()
    {
        // Searching for a mating partner logic
        // Transition to other states based on conditions
    }

    private void ChangeState(BunnyState newState)
    {
        Debug.Log("Changing state from " + currentState + " to " + newState);
        currentState = newState;
    }

    private void KillBunny()
    {
        //Kill bunny code
    }

}