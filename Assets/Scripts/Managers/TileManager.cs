using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    #region Lists
    private List<Tile> controlList = new List<Tile>(); //List of tiles to check the neighbours
    private List<Tile> tiles = new List<Tile>(); //List of all tiles that we created
    [SerializeField] private List<Item> itemList = new List<Item>(); // Items that we use to change the sprites of tiles
    private HashSet<Tile> activeTileList = new HashSet<Tile>(); //List of all neighbours including the tile itself
    #endregion
    
    public static TileManager instance;
    private static Level level;
    private Transform[] spawnPoints;

    [SerializeField] private GameObject board;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private SpriteRenderer boardRenderer;
    [SerializeField] private SpriteRenderer tileRenderer;
    [SerializeField] private GameManager gameManager;


    private float scaleRateWidth;
    private float scaleRateHeight;
    private float newTileWidth;
    private float newTileHeight;

    private bool isBoardGenerated;
    private bool isClickable;


    private int controlIndex; // Index of the tile that should check the neighbours
    private float spawnPointYPosition = 9; // Threshold number for Y position of spawn point 
    private float spawnPointXPosition = -5.12f; // Threshold number for X position of spawn point 

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
       
        level = LevelManager.instance.levels[0]; // Initiate the level 
        isClickable = false;
        CalculateTileScale();
        CreateSpawnPoints();
        StartCoroutine(GenerateTiles());
    }
     
    // Generate all tiles
     public IEnumerator GenerateTiles()
     {
         for (int i = 0; i < level.row; i++)
         {
             for ( int j = 0; j < level.column; j++)
             {
                 GameObject tile = Instantiate(tilePrefab, spawnPoints[j]);
                 InitializeTile(tile,j,i);
                 yield return new WaitForSeconds(0.1f);
             }
         }
         CheckNeighboursAndDeadlock();
         yield return new WaitForSeconds(0.1f * level.column); // Give delay for tiles to drop
         isClickable = true;
         isBoardGenerated = true;
     }
     // Arrange and give some properties to the tiles
     private void InitializeTile(GameObject tempTile, int xAxis, int yAxis)
     {
         Tile tile = tempTile.GetComponent<Tile>();
         int typeRandomized = UnityEngine.Random.Range(0, level.colorNumber);
         tile.TileType = itemList[typeRandomized].tileType; // Give random tile type
         tile.SpriteRenderer.sprite = itemList[typeRandomized].sprite[0]; // Give random sprite
         tiles.Add(tile);
         tile.SetCoordinates(xAxis,yAxis);
         tile.transform.localScale = new Vector2(scaleRateWidth,scaleRateHeight); // Adjust the scale of tile
         
         Rigidbody2D rb = tile.GetComponent<Rigidbody2D>();
         Vector2 velocity = rb.velocity;
         velocity.y = -10;  // Give some velocity to tiles for smooth play
         rb.velocity = velocity;
     }

     private void CheckNeighboursAndDeadlock()
     {
         int neighbourListCounter = 0; // Counter to check the deadlock if all tiles have only 1 neighbour
         foreach (Tile tile in tiles)
         {
             tile.SetNeighbours(CheckBoard(tile));
             CheckForSpriteChange(tile.Neighbours.Count, tile.Neighbours, tile.TileType);
    
             if (tile.Neighbours.Count != 1) 
             {
                 neighbourListCounter++;
             }
         }

         if (neighbourListCounter == 0) // If all tiles have only 1 neighbour then it is a deadlock
         {
             gameManager.IsDeadlock = true;
         }
     }

    private void CreateSpawnPoints()
     {
         spawnPoints = new Transform[level.column];
         float xPositionStart = spawnPointXPosition + newTileWidth / 2;
         
         for (int i = 0; i < level.column; i++)
         {
              
             float xPosition = xPositionStart + newTileWidth * i;
             float yPosition = spawnPointYPosition + newTileHeight; 
             
             Vector3 spawnPosition = new Vector3(xPosition, yPosition, 0);
             GameObject spawnPointObject = new GameObject("SpawnPoint_" + i);
             spawnPointObject.transform.position = spawnPosition;
             spawnPoints[i] = spawnPointObject.transform;
         }
     }

    // Calculate the tile scale according to the board
    private void CalculateTileScale()
     {
         float boardWidth = boardRenderer.bounds.size.x;
         float boardHeight = boardRenderer.bounds.size.y;

         float originalTileWidth = tileRenderer.bounds.size.x; 
         float originalTileHeight = tileRenderer.bounds.size.y;

         newTileWidth = boardWidth / level.column;
         newTileHeight = boardHeight / level.row;
         
         scaleRateWidth = newTileWidth / originalTileWidth / board.transform.localScale.x * tilePrefab.transform.localScale.x;
         scaleRateHeight = newTileHeight / originalTileHeight / board.transform.localScale.y * tilePrefab.transform.localScale.y;
     }
     
    public void TileClicked(Tile tile)
    {
        isClickable = false; // Close the click to prevent miscalculations and bugs 
        if (tile.Neighbours.Count >= 2) // If there is more than 2 neighbours, destroy them
        {
            StartCoroutine(DestroyNeighbours(tile.Neighbours)); 
        }
    }
   
    private IEnumerator DestroyNeighbours(HashSet<Tile> neighbours){
        foreach (Tile tile in neighbours)
        {
            tiles.Remove(tile);
            GenerateTileAtColumn(tile.PosX,tile.PosY); // Generate new tiles using the column info of destroyed tiles
            if(tile != null) Destroy(tile.gameObject); 
            yield return new WaitForSeconds(0.1f); // Give delay for calculations
        }
        StartCoroutine(ReassignCoordinates()); // Reassign coordinates for recalculations of neighbours
    }
    
    private void GenerateTileAtColumn(int x,int y)
    {
        GameObject tile = Instantiate(tilePrefab, new Vector3(spawnPoints[x].transform.position.x,spawnPoints[x].transform.position.y + newTileHeight,0),Quaternion.identity,spawnPoints[x]); // Instantiate new tiles in a higher position to avoid bugs
        InitializeTile(tile,x,y);
    }
    
    private IEnumerator ReassignCoordinates()
    {
        yield return new WaitForEndOfFrame();
        // Recalculate the parents to arrange tile coordinates
        foreach (Tile tile in tiles)
        {
            int parentIndex = 0;
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (tile && spawnPoints[i] == tile.transform.parent)
                {
                    parentIndex = i;
                    break;
                }
            }
            tile.SetCoordinates(parentIndex,tile.transform.GetSiblingIndex());
        }

        // Sort tiles list according to X and Y coordinates
        tiles.Sort((a, b) =>
        {
            int posYComparison = a.PosY.CompareTo(b.PosY);
            if (posYComparison != 0)
            {
                return posYComparison;
            }
            return a.PosX.CompareTo(b.PosX);
        });

        // Recalculate the coordinates
        for (int i = 0; i < tiles.Count; i++)
        {
            Tile tile = tiles[i];
            tile.SetCoordinates(i % level.column, i / level.column);
        }

        CheckNeighboursAndDeadlock();
        yield return new WaitForSeconds(0.1f * level.column); // Give delay for new tiles to drop
        isClickable = true;
    }
    
    public HashSet<Tile> CheckBoard(Tile tile)
    {
        controlList.Clear();
        activeTileList.Clear();
        controlIndex = 0;
        controlList.Add(tile);

        do
        {
            Tile tempTile = controlList[controlIndex];
            CalculateNeighbours(tempTile);
            controlIndex++; // Go to the next tile
        } while (controlIndex < controlList.Count);
       
        return activeTileList;
    }
    
    private void CalculateNeighbours(Tile tile) 
    {
        activeTileList.Add(tile);

        for (int i = -1; i <= 1; i += 2)   //Check for the X-axis
        {
            int targetPosX = tile.PosX + i;
            if (targetPosX >= 0 && targetPosX < level.column)
            {
                Tile targetTile = GetTile(targetPosX, tile.PosY); // Get the target tile
                if (targetTile != null && targetTile.TileType == tile.TileType )
                {
                   
                    activeTileList.Add(tile);

                    if (!controlList.Contains(targetTile))
                    {
                        controlList.Add(targetTile);
                    }
                }
            }
           
        }

        for (int i = -1; i <= 1; i += 2)   //Check for the Y-axis
        {
            int targetPosY = tile.PosY + i;
            if (targetPosY >= 0 && targetPosY < level.row)
            {
                Tile targetTile = GetTile(tile.PosX, targetPosY); // Get the target tile
                if (targetTile != null && targetTile.TileType == tile.TileType)
                {
                    
                    activeTileList.Add(tile);

                    if (!controlList.Contains(targetTile))
                    {
                        controlList.Add(targetTile);
                    }
                }
            }
           
        }
    }
    // Get the tile for neighbour checking
    private Tile GetTile(int x, int y)
    {
        int index = (y * level.column) + x;
        if (index >= 0 && index < tiles.Count)
        {
            return tiles[index];
        }
        return null; 
        
    }
    
    // Check the conditions and give the icons
    private void CheckForSpriteChange(int counter, HashSet<Tile> neighbours,TileType tileType)
    {
        if (counter <= level.firstCondition)
        {
            ChangeSprite(neighbours,itemList[(int)tileType].sprite[0]); // default icon
        }
        else if (counter > level.firstCondition && counter <= level.secondCondition)
        {
            ChangeSprite(neighbours,itemList[(int)tileType].sprite[1]); // first icon
        }
        else if (counter > level.secondCondition && counter <= level.thirdCondition)
        {
            ChangeSprite(neighbours,itemList[(int)tileType].sprite[2]); // second icon 
        }
        else
        {
            ChangeSprite(neighbours,itemList[(int)tileType].sprite[3]); // third icon
        }
        
    }
    
    //Change the sprites of all neighbours
    private void ChangeSprite(HashSet<Tile> neighbours, Sprite sprite)
    {
        foreach (var tile in neighbours)
        {
            tile.SpriteRenderer.sprite = sprite;
        }

    }
    
    public List<Tile> Tiles
    {
        get => tiles;
        set => tiles = value;
    }
    public bool IsClickable
    {
        get => isClickable;
        set => isClickable = value;
    }
    public bool IsBoardGenerated
    {
        get => isBoardGenerated;
        set => isBoardGenerated = value;
    }
}
