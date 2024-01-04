using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bunny : Animal
{
    //Estados do coelho
    public enum BunnyState
    {
        Exploring,
        SearchingForFood,
        Eating,
        SearchingForMatingPartner
    }
    private BunnyState currentState;

    //Variaveis da exploração
    private HashSet<Vector2Int> visitedTiles = new HashSet<Vector2Int>();

    //Variaveis da procura de comida
    private GameObject targetBush = null;
    private List<GameObject> detectedBushes = new List<GameObject>();

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
                case BunnyState.SearchingForFood:
                    UpdateSearchingForFood();
                    if (HasReachedBush())
                    {
                        Debug.Log("Chegamos ao arbusto");
                        ChangeState(BunnyState.Eating);
                    }
                    break;
                case BunnyState.SearchingForMatingPartner:
                    UpdateSearchingForMatingPartner();
                    break;
                case BunnyState.Eating:
                    EatBush(targetBush.GetComponent<Bush>());
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
    private void UpdateSearchingForFood()
    {
        if (targetBush == null)
        {
            UpdateTargetBush(); // Find the closest bush
            if (targetBush == null)
            {
                // If no target bush is found, continue exploring
                Debug.Log("No target bush found. Continuing to explore.");
                ChangeState(BunnyState.Exploring);
                return;
            }
        }

        Vector2Int bushTile = Vector2Int.FloorToInt(targetBush.transform.position);
        Vector3Int currTile = GetTilePosition();
        Vector2Int currTile2D = new Vector2Int(currTile.x, currTile.y);

        if (bushTile != currTile2D && !base.pathfinderExecuted)
        {
            Debug.Log($"Attempting to find path from {currTile2D} to {bushTile}");
            base.path = PathFinder.Instance.FindPath(currTile2D, bushTile);
            if (base.path != null && base.path.Count > 0)
            {
                StartCoroutine(FollowPath());
            }
            else
            {
                Debug.Log("No valid path found");
            }
        }

        if (HasReachedBush())
        {
            Debug.Log("Reached the bush");
            ChangeState(BunnyState.Eating);
        }
    }

    private void UpdateTargetBush()
    {
        float minDistance = float.MaxValue;
        GameObject closestBush = null;

        foreach (var bush in detectedBushes)
        {
            float distance = Vector2.Distance(transform.position, bush.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestBush = bush;
            }
        }

        if (closestBush != targetBush)
        {
            targetBush = closestBush;
            Debug.Log("Target bush updated to: " + targetBush.name);
            if (currentState == BunnyState.SearchingForFood)
            {
                FindPathToBush();
            }
        }
    }

    private void CheckFullnessAndUpdateState()
    {
        if (fullness > 50 && currentState != BunnyState.Exploring)
        {
            ChangeState(BunnyState.Exploring);
        }
        else if (fullness < 40 && currentState != BunnyState.SearchingForFood && currentState != BunnyState.Eating)
        {
            ChangeState(BunnyState.SearchingForFood);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Bush"))
        {
            Debug.Log("Bush detected: " + other.gameObject.name);
            detectedBushes.Add(other.gameObject);
            UpdateTargetBush();
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Bush") && !detectedBushes.Contains(other.gameObject))
        {
            detectedBushes.Add(other.gameObject);
            UpdateTargetBush();
        }
    }

    private void FindPathToBush()
    {
        if (targetBush != null)
        {
            Vector2Int bushTile = Vector2Int.FloorToInt(targetBush.transform.position);
            Vector3Int currTile = GetTilePosition();
            Vector2Int currTile2D = new Vector2Int(currTile.x, currTile.y);

            if (bushTile != currTile2D)
            {
                Debug.Log($"Finding path to bush at {bushTile}");
                base.path = PathFinder.Instance.FindPath(currTile2D, bushTile);
                if (base.path != null && base.path.Count > 0)
                {
                    StartCoroutine(FollowPath());
                }
                else
                {
                    Debug.Log("No valid path found");
                }
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------
    //---------------------------------------------EATING------------------------------------------------------------

    private bool HasReachedBush()
    {
        if (targetBush == null)
        {
            return false;
        }

        Vector2Int bushTile = Vector2Int.FloorToInt(targetBush.transform.position);
        Vector3Int currTile = GetTilePosition();

        return bushTile.x == currTile.x && bushTile.y == currTile.y;
    }

    private void EatBush(Bush bush)
    {
        if (bush != null && HasReachedBush())
        {
            bush.Eat(2); // Bunny eats a portion of the bush
            RaiseFullness();

            if (fullness >= 100 || bush.foodAmount == 0)
            {
                ChangeState(BunnyState.Exploring);
                targetBush = null; // Clear target bush after eating
            }
        }
    }

    private void RaiseFullness()
    {
        fullness += base.fulnessIncreaseRate; // Adjust as needed
        fullness = Mathf.Min(fullness, 100); // Ensure fullness doesn't go above 100
    }

    //-------------------------------------------------------------------------------------------



    private void UpdateSearchingForMatingPartner()
    {
        // Searching for a mating partner logic
        // Transition to other states based on conditions
    }

    private void ChangeState(BunnyState newState)
    {
        Debug.Log("Changing state from " + currentState + " to " + newState);
        currentState = newState;

        if (newState == BunnyState.SearchingForFood)
        {
            FindPathToBush();
        }
    }

    private void KillBunny()
    {
        //Kill bunny code
    }
}