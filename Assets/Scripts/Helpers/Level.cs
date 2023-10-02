using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level
{
    public int row, column, colorNumber, firstCondition,secondCondition,thirdCondition;

    // Constructor to initialize the level parameters
    public Level(int row, int column, int colorNumber, int firstCondition, int secondCondition, int thirdCondition)
    {
        this.row = row;
        this.column = column;
        this.colorNumber = colorNumber;
        this.firstCondition = firstCondition;
        this.secondCondition = secondCondition;
        this.thirdCondition = thirdCondition;
    }
}
