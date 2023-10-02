using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance; 
    public List<Level> levels = new List<Level>(); // List to store different levels

    private void Awake()
    {
        // Check if an instance of LevelManager already exists
        if (instance)
        {
            Destroy(gameObject); // Destroy the duplicate instance to ensure there's only one
        }
        else
        {
            instance = this; 
            DontDestroyOnLoad(gameObject); // Ensure that this object persists across scenes
        }
        
        GenerateLevels(); // Call the method to generate predefined levels
    }

    private void GenerateLevels()
    {
        // Create a new Level object with specific parameters and add it to the levels list
        Level level1 = new Level(5, 5, 4, 2, 4, 5);
        levels.Add(level1);
    }
    
    public List<Level> Levels
    {
        get => levels;
        set => levels = value;
    }
}
