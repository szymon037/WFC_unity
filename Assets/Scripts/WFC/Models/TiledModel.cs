using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Xml;
using System.Text;

class TiledModel : Model
{
    public TiledModel(int gridWidth, int gridDepth, int gridLength, int tileSize, bool seamless, bool processTiles, string setName = null, Tile[][][] inputMap = null, Transform parent = null) : base(gridWidth, gridDepth, gridLength, tileSize, seamless, parent)
    {
        this.tileSize = tileSize;
        if (setName != null)
            TilesManager.LoadTilesTiled(setName, processTiles);
        if (TilesManager.tilesTiled == null)
        {
            Debug.LogError("Could not load tiles!");
            return;
        }
        tiles = TilesManager.tilesTiled;

        InitGrid();
        Init();

        if (inputMap != null)
            ProcessInputMap(inputMap);
    }

    // infinite chunk generation
    public TiledModel(int gridWidth, int gridLength, int gridDepth, int tileSize, bool processTiles, string setName, int[][] neighbourCells, Transform parent) : base(gridWidth, gridLength, gridDepth, tileSize, false, parent)
    {
        chunkGeneration = true;
        /// TODO: process tiles in infinity mode only once
        this.tileSize = tileSize;
        if (setName != null)
            TilesManager.LoadTilesTiled(setName, processTiles);
        if (TilesManager.tilesTiled == null)
        {
            Debug.LogError("Could not load tiles!");
            return;
        }
        tiles = TilesManager.tilesTiled;
        ///
        InitGrid();
        Init();

        /// PARSING NEIGHBOURS
        for (int i = 0; i < 4; i++)
        {
            if (neighbourCells[i] == null)
                continue;

            int dirIndex = (i > 1) ? i + 2 : i; // i'm so sorry
            if (dirIndex == 0 || dirIndex == 1)
            {
                int x = (dirIndex == 0) ? 0 : gridWidth - 1;
                for (int j = 0; j < neighbourCells[i].Length; j++)
                {
                    int y = j / gridLength;
                    int z = j % gridLength;

                    int index = ID(x, y, z);

                    for (int m = 0; m < tiles.Length; m++)
                        if (m != neighbourCells[i][j])
                            grid[index].UpdatePossibilities(m, opposite[dirIndex]);
                }
            }

            else if (dirIndex == 4 || dirIndex == 5)
            {
                int z = (dirIndex == 4) ? gridLength - 1 : 0;
                for (int j = 0; j < neighbourCells[i].Length; j++)
                {
                    int y = j / gridLength;
                    int x = j % gridLength;

                    int index = ID(x, y, z);


                    for (int m = 0; m < tiles.Length; m++)
                        if (m != neighbourCells[i][j])
                            grid[index].UpdatePossibilities(m, opposite[dirIndex]);
                }
            }
           
        }
    }
    
    public override void GenerateOutput()
    {
        outputTransform = new GameObject("WFC_output_tiled").transform;
        if (parent != null)
        {
            outputTransform.parent = parent;
            outputTransform.transform.localPosition= Vector3.zero;
        }

        for (int z = 0; z < gridLength; z++)
            for (int y = 0; y < gridDepth; y++)
                for (int x = 0; x < gridWidth; x++)
                {
                    GameObject go = null;
                    if (chunkGeneration)
                    {
                        go = Object.Instantiate(grid[ID(x, y, z)]._tile._tileGameObject, Vector3.zero, grid[ID(x, y, z)]._tile._rotation, outputTransform);
                        go.transform.localScale = grid[ID(x, y, z)]._tile._scale;
                        go.transform.localPosition = new Vector3(x, y, z) * tileSize;
                        go.name = grid[ID(x, y, z)]._tile.GetName();
                    }
                    else
                    {
                        go = Object.Instantiate(grid[ID(x, y, z)]._tile._tileGameObject, new Vector3(x, y, z) * tileSize + offset, grid[ID(x, y, z)]._tile._rotation, outputTransform);
                        go.transform.localScale = grid[ID(x, y, z)]._tile._scale;
                        go.name = grid[ID(x, y, z)]._tile.GetName();
                    }
                    output[x][y][z] = go;
                }
    }

    private void ProcessInputMap(Tile[][][] inputMap)
    {
        for (int x = 0; x < inputMap.Length; x++)
            for (int y = 0; y < inputMap[0].Length; y++)
                for (int z = 0; z < inputMap[0][0].Length; z++)
                    for (int i = 0; i < tiles.Length; i++)
                        if (inputMap[x][y][z] != null && inputMap[x][y][z].GetName() == tiles[i].GetName())
                        {
                            int index = ID(x, y, z);
                            if (grid[index]._entropy > 0f) // TODO: same tile is found twice. fix this
                                grid[index].ChooseTile(i);

                        }

    }

    public override bool OnBorder(int x, int y, int z)
    {
        return (x < 0 || y < 0 || z < 0 || 
                x >= gridWidth || y >= gridDepth || z >= gridLength);
    }
}
