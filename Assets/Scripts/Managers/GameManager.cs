using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private bool isDeadlock = false; // Variable to track if a deadlock has occurred
    
    public bool IsDeadlock
    {
        get => isDeadlock;
        set => isDeadlock = value;
    }

    private void Update()
    {
        // Check if the game board is generated
        if (TileManager.instance.IsBoardGenerated)
        {
            // If a deadlock has occurred, regenerate the game board
            if (isDeadlock)
            {
                RegenerateBoard();
                isDeadlock = false; // Reset the deadlock flag
            }
        }
    }

    private void RegenerateBoard()
    {
        // Destroy all existing tiles in the game board
        foreach (Tile tile in TileManager.instance.Tiles)
        {
            Destroy(tile.gameObject);
        }
        TileManager.instance.Tiles.Clear(); // Clear the list of tiles
        
        // Start the coroutine to generate a new game board
        StartCoroutine(TileManager.instance.GenerateTiles());
    }
}