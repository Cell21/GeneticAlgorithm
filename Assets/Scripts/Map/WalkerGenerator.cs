using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WalkerGenerator : MonoBehaviour
{
    // Singleton instance
    public static WalkerGenerator Instance { get; private set; }

    //Tiles
    public enum GridTiles
    {
        FLOOR,
        WALL,
        EMPTY
    }

    //Map Creation Variables 
    public GridTiles[,] gridHandler;
    public List<WalkerObject> walkerObjects;
    public Tilemap tileMap;
    public Tile floorTile;
    public Tile wallTile;
    public int mapWidth = 30;
    public int mapHeight = 30;
    public int maximumWalkers = 10;
    public int tileCount = default;
    public float fillPercentage = 0.4f;
    public float waitTime = 0.05f;


    //Bush
    public GameObject bushPrefab;
    public GameObject bushParent;

    //Rabbit
    public GameObject rabbitPrefab;
    private float timeSinceStart = 0f; // Tempo desde o início da execução
    private bool isRabbitSpawned = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Ensures only one instance exists
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Makes the object persistent across scenes
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        InitializeGrid();

        // Calculate the center of the grid in world coordinates
        UnityEngine.Vector3 centerOfGrid = tileMap.GetCellCenterWorld(new Vector3Int(mapWidth / 2, mapHeight / 2, 0));

        // Adjust the camera position to center it on the grid
        Camera.main.transform.position = new UnityEngine.Vector3(centerOfGrid.x, centerOfGrid.y, Camera.main.transform.position.z);
        
    }


    private void Update()
    {
        timeSinceStart += Time.deltaTime;
        if (!isRabbitSpawned && timeSinceStart >= 15f)
        {
            // Agora, encontre uma posição caminhável aleatória para o coelho.
            Vector2Int randomTilePosition = FindRandomWalkableTile();

            // Instancie o coelho na posição encontrada.
            UnityEngine.Vector3 worldPosition = tileMap.GetCellCenterWorld(new Vector3Int(randomTilePosition.x, randomTilePosition.y, 0));
            GameObject rabbitGameObject = Instantiate(rabbitPrefab, worldPosition, UnityEngine.Quaternion.identity);
            isRabbitSpawned=true;
        } 
        
    }

    //---------------------------------------------------------------MAP CREATION------------------------------------------------------------------------------
    void InitializeGrid()
    {
        gridHandler = new GridTiles[mapWidth, mapHeight];

        for (int x = 0; x < gridHandler.GetLength(0); x++)
        {
            for (int y = 0; y < gridHandler.GetLength(1); y++)
            {
                gridHandler[x, y] = GridTiles.EMPTY;
            }
        }

        walkerObjects = new List<WalkerObject>();

        Vector3Int TileCenter = new Vector3Int(gridHandler.GetLength(0) / 2, gridHandler.GetLength(1) / 2, 0);

        WalkerObject curWalker = new WalkerObject(new Vector2(TileCenter.x, TileCenter.y), GetDirection(), 0.5f);

        gridHandler[TileCenter.x, TileCenter.y] = GridTiles.FLOOR;
        tileMap.SetTile(TileCenter, floorTile);

        walkerObjects.Add(curWalker);

        StartCoroutine(CreateFloors());


    }

    Vector2 GetDirection()
    {
        int choice = Mathf.FloorToInt(Random.value * 3.99f);

        switch (choice)
        {
            case 0:
                return Vector2.down;
            case 1:
                return Vector2.left;
            case 2:
                return Vector2.up;
            case 3:
                return Vector2.right;
            default:
                return Vector2.zero;
        }
    }

    IEnumerator CreateFloors()
    {
        while ((float)tileCount / (float)gridHandler.Length < fillPercentage)
        {
            bool hasCreatedFloor = false;
            foreach (WalkerObject curWalker in walkerObjects)
            {
                Vector3Int curPos = new Vector3Int((int)curWalker.Position.x, (int)curWalker.Position.y, 0);

                if (gridHandler[curPos.x, curPos.y] != GridTiles.FLOOR)
                {
                    tileMap.SetTile(curPos, floorTile);
                    tileCount++;
                    gridHandler[curPos.x, curPos.y] = GridTiles.FLOOR;

                    float bushSpawnChance = 0.05f;
                    if (UnityEngine.Random.value < bushSpawnChance)
                    {
                        SpawnBush(curPos);
                    }

                    hasCreatedFloor = true;
                }

            }

            //Walker Methods
            ChanceToRemove();
            ChanceToRedirect();
            TryCreateNewWalker();
            UpdatePosition();

            if (hasCreatedFloor)
            {
                yield return new WaitForSeconds(waitTime);
            }

            StartCoroutine(CreateWalls());


        }
    }

    void ChanceToRemove()
    {
        int updatedCount = walkerObjects.Count;
        for (int i = 0; i < updatedCount; i++)
        {
            if (Random.value < walkerObjects[i].ChanceToChange && walkerObjects.Count > 1)
            {
                walkerObjects.RemoveAt(i);
                break;
            }
        }
    }

    void ChanceToRedirect()
    {
        for (int i = 0; i < walkerObjects.Count; i++)
        {
            if (Random.value < walkerObjects[i].ChanceToChange)
            {
                WalkerObject curWalker = walkerObjects[i];
                curWalker.Direction = GetDirection();
                walkerObjects[i] = curWalker;
            }
        }
    }

    void TryCreateNewWalker()
    {
        int updatedCount = walkerObjects.Count;
        for (int i = 0; i < updatedCount; i++)
        {
            if (Random.value < walkerObjects[i].ChanceToChange && walkerObjects.Count < maximumWalkers)
            {
                Vector2 newDirection = GetDirection();
                Vector2 newPosition = walkerObjects[i].Position;

                WalkerObject newWalker = new WalkerObject(newPosition, newDirection, 0.5f);
                walkerObjects.Add(newWalker);
            }
        }
    }

    void UpdatePosition()
    {
        for (int i = 0; i < walkerObjects.Count; i++)
        {
            WalkerObject FoundWalker = walkerObjects[i];
            FoundWalker.Position += FoundWalker.Direction;
            FoundWalker.Position.x = Mathf.Clamp(FoundWalker.Position.x, 1, gridHandler.GetLength(0) - 2);
            FoundWalker.Position.y = Mathf.Clamp(FoundWalker.Position.y, 1, gridHandler.GetLength(1) - 2);
            walkerObjects[i] = FoundWalker;

        }
    }

    IEnumerator CreateWalls()
    {
        for (int x = 0; x < gridHandler.GetLength(0) - 1; x++)
        {
            for (int y = 0; y < gridHandler.GetLength(1) - 1; y++)
            {
                if (gridHandler[x, y] == GridTiles.FLOOR)
                {
                    bool hasCreatedWall = false;

                    if (gridHandler[x + 1, y] == GridTiles.EMPTY)
                    {
                        tileMap.SetTile(new Vector3Int(x + 1, y, 0), wallTile);
                        gridHandler[x + 1, y] = GridTiles.WALL;
                        hasCreatedWall = true;
                    }
                    if (gridHandler[x - 1, y] == GridTiles.EMPTY)
                    {
                        tileMap.SetTile(new Vector3Int(x - 1, y, 0), wallTile);
                        gridHandler[x - 1, y] = GridTiles.WALL;
                        hasCreatedWall = true;
                    }
                    if (gridHandler[x, y + 1] == GridTiles.EMPTY)
                    {
                        tileMap.SetTile(new Vector3Int(x, y + 1, 0), wallTile);
                        gridHandler[x, y + 1] = GridTiles.WALL;
                        hasCreatedWall = true;
                    }
                    if (gridHandler[x, y - 1] == GridTiles.EMPTY)
                    {
                        tileMap.SetTile(new Vector3Int(x, y - 1, 0), wallTile);
                        gridHandler[x, y - 1] = GridTiles.WALL;
                        hasCreatedWall = true;
                    }

                    if (hasCreatedWall)
                    {
                        yield return new WaitForSeconds(waitTime);
                    }
                }
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    //---------------------------------------------------------------PREFAB SPAWN RELATED----------------------------------------------------------------------
    public Vector2Int FindRandomWalkableTile()
    {
        List<Vector2Int> walkableTiles = new List<Vector2Int>();

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (gridHandler[x, y] == GridTiles.FLOOR)
                {
                    walkableTiles.Add(new Vector2Int(x, y));
                }
            }
        }

        if (walkableTiles.Count > 0)
        {
            return walkableTiles[Random.Range(0, walkableTiles.Count)];
        }

        return new Vector2Int(-1, -1); // Retorna um valor inválido se não houver tiles caminháveis

    }
    public void SpawnBush(Vector3Int position)
    {
        UnityEngine.Vector3 bushPosition = tileMap.GetCellCenterWorld(position);
        GameObject newBush = Instantiate(bushPrefab, bushPosition, UnityEngine.Quaternion.identity);
        newBush.name = "Bush_" + position.x + "_" + position.y;

        if (bushParent != null)
        {
            newBush.transform.SetParent(bushParent.transform);
        }

    }
}