using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Model : MonoBehaviour
{
    public Cell[] grid;
    public int gridWidth;
    public int gridHeight;
    private float startEntropy = 0;
    public static (int, int)[] stack;
    public static int stackSize = 0;
    public static int[] opposite = new int[] { 1, 0, 3, 2 };
    public static Tile[] tiles;
    public GameObject[][] output;
    public int tileSize;

    public Model(int gridWidth, int gridHeight, int tileSize)
    {
        this.gridWidth = gridWidth;
        this.gridHeight = gridHeight;
        this.tileSize = tileSize;

        stackSize = 0;
    }

    public void Init()
    {
        stack = new (int, int)[grid.Length * tiles.Length];
        output = new GameObject[gridWidth][]; /// 3D
        for (int i = 0; i < gridWidth; i++)
        {
            output[i] = new GameObject[gridHeight];
        }
        
    }

    public void InitGrid()
    {
        int[][] compatible = new int[tiles.Length][];

        for (int i = 0; i < tiles.Length; i++)
        {
            compatible[i] = new int[4];
            for (int j = 0; j < 4; j++)
            {
                compatible[i][j] = tiles[i]._adjacencies[j].Length;
            }
        }

        grid = new Cell[gridWidth * gridHeight];

        for (int i = 0; i < tiles.Length; i++)
        {
            startEntropy += tiles[i]._weight;
        }

        for (int i = 0; i < grid.Length; i++)
        {
            grid[i] = new Cell(i, startEntropy, compatible);
        }
    }

    public void Solve()
    {
        int result = 0;
        do
        {
            result = Observe();
        } while (result == 0);

        if (result == -1)
        {
            Debug.Log("Solution not found");
            return;
        }
        else if (result == -2)
        {
            Debug.Log("DONE");
            GenerateOutput();
        }
    }

    private int Observe()
    {
        //updateStack.Clear();
        int index = FindLowestEntropy();

        if (index == -1) // contradiction
            return -1;

        if (index == -2) // done
            return -2;

        //updateStack.Add(index);
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

            int U = cellIndex - gridWidth;
            int D = cellIndex + gridWidth;
            int L = cellIndex - 1;
            int R = cellIndex + 1;

            // L - 0, R - 1, U - 2, D - 3

            if (!OnBorder(L))
                grid[L].UpdatePossibilities(tileIndex, 0);

            if (!OnBorder(R))
                grid[R].UpdatePossibilities(tileIndex, 1);

            if (!OnBorder(U))
                grid[U].UpdatePossibilities(tileIndex, 2);

            if (!OnBorder(D))
                grid[D].UpdatePossibilities(tileIndex, 3);

        }
    }

    private int FindLowestEntropy()
    {
        int index = -2;
        float lowestCustomEntropy = 9999f;

        for (int i = 0; i < grid.Length; i++)
        {
            if (OnBorder(i))
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
            //Console.WriteLine("grid[i]._possibilities: " + grid[i]._possibilities);
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
            Instantiate(tiles[i]._tileGameObject, new Vector3(i * 5f, 0f, 0f), Quaternion.identity);
        }
    }

    public abstract void GenerateOutput();
    public abstract bool OnBorder(int index);


}
