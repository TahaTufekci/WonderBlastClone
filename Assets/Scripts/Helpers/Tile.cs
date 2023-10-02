using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TileType tileType;

    private HashSet<Tile> neighbours;
    private int posX, posY;

    // Set the X and Y coordinates of the tile
    public void SetCoordinates(int x, int y)
    {
        posX = x;
        posY = y;
        gameObject.name = "(" + x + ") (" + y + ")";
    }

    // Set the neighboring tiles of this tile
    public void SetNeighbours(HashSet<Tile> neighbourList)
    {
        neighbours = neighbourList;
    }

    // Called when the tile is clicked
    private void OnMouseDown()
    {
        if (TileManager.instance.IsClickable)
        {
            // Update the neighbors and notify the TileManager that this tile was clicked
            neighbours = TileManager.instance.CheckBoard(this);
            TileManager.instance.TileClicked(this);
        }
    }

    public HashSet<Tile> Neighbours
    {
        get => neighbours;
        set => neighbours = value;
    }

    public SpriteRenderer SpriteRenderer
    {
        get => spriteRenderer;
        set => spriteRenderer = value;
    }

    public TileType TileType
    {
        get => tileType;
        set => tileType = value;
    }

    public int PosX
    {
        get => posX;
        set => posX = value;
    }

    public int PosY
    {
        get => posY;
        set => posY = value;
    }
}
