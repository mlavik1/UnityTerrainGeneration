using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TerrainGridCell
{
    public int X;
    public int Y;

    public TerrainGridCell(int inX, int inY)
    {
        X = inX;
        Y = inY;
    }
}

public class TerrainCell : MonoBehaviour
{
    public float[,] mCellHeights;
}
