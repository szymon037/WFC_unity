using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// L - 0, R - 1, U - 2, D - 3, F - 4, B - 5

public abstract class Model
{
    public Cell[] grid;
    public int gridWidth;
    public int gridLength;
    public int gridDepth;
    private float startEntropy = 0;
    public static (int, int)[] stack;
    public static int stackSize = 0;
    public static int[] opposite = new int[] { 1, 0, 3, 2, 5, 4 };
    public static Tile[] tiles;
    public GameObject[][][] output;
    public int tileSize;
    public bool seamless;
    public int[,] dir = new int[6, 3] { { -1, 0, 0 }, { 1, 0, 0 }, { 0, 1, 0 }, { 0, -1, 0 }, { 0, 0, 1}, { 0, 0, -1 } }; // L R U D F B
    public Vector3 offset = Vector3.zero;
    protected Transform parent;
    protected bool chunkGeneration = false;
    public Transform outputTransform;
    public static int groundIndex = -1;
    ///TODO: clearing whole model on solve (so creating new Model is not needed)
    ///TODO: extrnal tile creator and processor - needed in infinity generator

    public Model(int gridWidth, int gridDepth, int gridLength, int tileSize, bool seamless, Transform parent = null)
    {
        this.gridWidth = gridWidth;
        this.gridLength = gridLength;
        this.gridDepth = gridDepth;
        this.tileSize = tileSize;
        this.seamless = seamless;
        this.parent = parent;

        stackSize = 0;
    }

    public void Init()
    {
        stack = new (int, int)[grid.Length * tiles.Length];
        output = new GameObject[gridWidth][][]; /// 3D
        for (int x = 0; x < gridWidth; x++)
        {
            output[x] = new GameObject[gridDepth][];
            for (int y = 0; y < gridDepth; y++)
            {
                output[x][y] = new GameObject[gridLength];
            }
        }
        
    }

    public void InitGrid()
    {
        int[][] compatible = new int[tiles.Length][];

        for (int i = 0; i < tiles.Length; i++)
        {
            compatible[i] = new int[6];
            for (int j = 0; j < 6; j++)
            {
                compatible[i][j] = tiles[i]._tileAdjacencies[j].Length;
            }
        }

        grid = new Cell[gridWidth * gridLength * gridDepth];

        for (int i = 0; i < tiles.Length; i++)
        {
            startEntropy += tiles[i]._weight;
        }

        for (int i = 0; i < grid.Length; i++)
        {
            grid[i] = new Cell(i, startEntropy, compatible);
        }
    }

    public bool Solve()
    {
        int result = 0;
        do
        {
            result = Observe();
        } while (result == 0);

        if (result == -1)
        {
            Debug.Log("Solution not found");
            return false;
        }
        else if (result == -2)
        {
            Debug.Log("DONE");
            GenerateOutput();
            return true;
        }
        return false;
    }

    private int Observe()
    {
        if (stackSize > 0)
        {
            Propagate();
            return 0;
        }

        int index = FindLowestEntropy();

        if (index == -1) // contradiction
            return -1;

        if (index == -2) // done
            return -2;

        grid[index].ChooseTile(); // chooses randomly tile from available tiles

        Propagate();

        return 0;
    }

    private void Propagate()
    {
        while (stackSize > 0)
        {
            var stackValue = stack[stackSize - 1];
            stackSize--;
            int cellIndex = stackValue.Item1;
            int tileIndex = stackValue.Item2;

            // L - 0, R - 1, U - 2, D - 3, F - 4, B - 5
            int[] IDs = FromID(cellIndex);
            int x = IDs[0];
            int y = IDs[1];
            int z = IDs[2];

            for (int d = 0; d < 6; d++)
            {

                int x2 = x + dir[d, 0];
                int y2 = y + dir[d, 1];
                int z2 = z + dir[d, 2];

                if (OnBorder(x2, y2, z2) && !seamless)
                    continue;

                if (x2 < 0)                  x2 += gridWidth;
                else if (x2 >= gridWidth)    x2 -= gridWidth;

                if (y2 < 0)                  y2 += gridDepth;
                else if (y2 >= gridDepth)    y2 -= gridDepth;

                if (z2 < 0)                  z2 += gridLength;
                else if (z2 >= gridLength)   z2 -= gridLength;

                int index2 = ID(x2, y2, z2);

                grid[index2].UpdatePossibilities(tileIndex, d);

            }
        }
    }

    private int FindLowestEntropy()
    {
        int index = -2;
        float lowestCustomEntropy = 9999f;

        for (int i = 0; i < grid.Length; i++)
        {
            int[] IDs = FromID(i);
            if (OnBorder(IDs[0], IDs[1], IDs[2]) && !seamless)
                continue;

            float customEntropy = grid[i]._entropy;


            if (customEntropy > 0f && customEntropy < lowestCustomEntropy)
            {
                lowestCustomEntropy = customEntropy;
                index = i;
            }
        }

        bool contradiction = false;
        for (int i = 0; i < grid.Length; i++) // checking if contradiction
        {
            contradiction |= (grid[i]._possibilities <= 0);
        }
        if (contradiction)
        {
            return -1;
        }

        return index;
    }

    public void InstantiateTiles()
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            Object.Instantiate(tiles[i]._tileGameObject, new Vector3(i * 5f, 0f, 0f), Quaternion.identity);
        }
    }

    protected int ID(int x, int y, int z)
    {
        return x + y * gridWidth * gridLength + z * gridWidth;
    }

    protected int[] FromID(int ID)
    {
        int y = ID / (gridWidth * gridLength);
        ID -= y * (gridWidth * gridLength);
        int x = ID % gridWidth;
        int z = ID / gridWidth;

        return new int[] { x, y, z };
    }

    public abstract void GenerateOutput();
    public abstract bool OnBorder(int x, int y, int z);


}
