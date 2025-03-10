﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class OverlappingModel : Model
{
    private int W, L, D;
    int[,,] indexMap;
    int N;
    int N_depth;
    private Dictionary<System.Numerics.BigInteger, Tile> tilesDictionary = new Dictionary<System.Numerics.BigInteger, Tile>();
    public List<Tile> tilesList = new List<Tile>();
    private bool overlapTileCreation = true;
    public OverlappingModel(int gridWidth, int gridDepth, int gridLength, int tileSize, bool seamless, int N, int N_depth, bool tileProcessing, Tile[][][] inputMap, bool overlapTileCreation, Transform parent = null) : base(gridWidth, gridDepth, gridLength, tileSize, seamless, parent)
    {
        W = inputMap.Length;        // x
        D = inputMap[0].Length;     // y   
        L = inputMap[0][0].Length;  // z
        indexMap = new int[W, D, L];
        this.N = N;
        this.N_depth = N_depth;
        this.overlapTileCreation = overlapTileCreation;

        GameObject emptyGO = Resources.Load<GameObject>("Tiles\\Empty");
        Tile emptyTile = new Tile(emptyGO);
        tilesList.Add(emptyTile);

        for (int z = 0; z < L; z++)
            for (int y = 0; y < D; y++)
                for (int x = 0; x < W; x++)
                {
                    int i = 0;
                    Tile currentTile;
                    if (inputMap[x][y][z] == null)
                        currentTile = emptyTile;
                    else
                        currentTile = inputMap[x][y][z];
                    for (int c = 0; c < tilesList.Count; c++)
                    {
                        if (currentTile.GetName() == tilesList[c].GetName())
                            break;

                        ++i;
                    }

                    if (tilesList.Count == i) tilesList.Add(currentTile);
                    indexMap[x, y, z] = i;
                }
        // tile creation
        List <Tile> newTiles = new List<Tile>();
        for (int z = 0; z < L; z++)
            for (int y = 0; y < D; y++)
                for (int x = 0; x < W; x++)
                {
                    Tile tile = CreateTile(x, y, z);
                    if (tile != null)
                        newTiles.Add(tile);
                }

        List<Tile> processTiles = new List<Tile>();

        foreach (Tile tile in newTiles)
        {
            Tile[] tilesToProcess = new Tile[8];
            tilesToProcess[0] = tile;

            if (tileProcessing)
            {
                tilesToProcess[1] = RotateTile(tilesToProcess[0]);
                tilesToProcess[2] = RotateTile(tilesToProcess[1]);
                tilesToProcess[3] = RotateTile(tilesToProcess[2]);
                tilesToProcess[4] = ReflectTile(tilesToProcess[0]);
                tilesToProcess[5] = ReflectTile(tilesToProcess[1]);
                tilesToProcess[6] = ReflectTile(tilesToProcess[2]);
                tilesToProcess[7] = ReflectTile(tilesToProcess[3]);
            }

            processTiles.AddRange(tilesToProcess);
        }

        foreach (Tile t in processTiles)
        {
            t.CalculateIndex(tilesList.Count);
            CountWeights(t);
        }

        newTiles = tilesDictionary.Values.ToList();
        for (int i = 0; i < newTiles.Count; i++)
        {
            for (int d = 0; d < 6; d++)
            {
                List<int> l = new List<int>();
                for (int j = 0; j < newTiles.Count; j++)
                {
                    if (CheckAdjacencies(newTiles[i], newTiles[j], d))
                        l.Add(j);
                }
                //newTiles[i]._tileAdjacencies[d] = l.ToArray();
                newTiles[i].SetAdjacencies(d, l);
            }
        }

        tiles = newTiles.ToArray();
        InitGrid();
        Init();
        outputTransform = new GameObject("WFC_output_overlapping").transform;
        if (parent != null)
            outputTransform.parent = parent;

        if (tilesList.Count > 31)
        {
            base.unsolvable = true;
            Debug.Log("Too many tiles for level - solution can't be found!");
        }
    }

    private Tile CreateTile(int x, int y, int z)
    {
        if ((y + N_depth) > D)
            return null;

        if (!overlapTileCreation && ((x + N) > W || (y + N_depth) > D || (z + N) > L))
            return null;

        int[] map = new int[N * N_depth * N];

        for (int i_z = 0; i_z < N; i_z++)
            for (int i_y = 0; i_y < N_depth; i_y++)
                for (int i_x = 0; i_x < N; i_x++)
                {
                    map[i_y * N * N + i_z * N + i_x] = indexMap[(x + i_x) % W, (y + i_y) % D, (z + i_z) % L];
                }

        //bool ground = (y == 0) ? true : false;

        return new Tile(map, 0f, 1f/*, ground*/);
    }
    // L - 0, R - 1, U - 2, D - 3, F - 4, B - 5

    public void GenerateInputMap(Tile[][][] inputMap)
    {
        GameObject p = new GameObject("inputMap");

        for (int z = 0; z < L; z++)
            {

                for (int y = 0; y < D; y++)
                {
                    for (int x = 0; x < W; x++)
                    {
                        if (inputMap[x][y][z] == null || inputMap[x][y][z]._tileGameObject == null)
                            continue;

                        GameObject go = UnityEngine.Object.Instantiate(inputMap[x][y][z]._tileGameObject, new Vector3(x, y, z) * tileSize, inputMap[x][y][z]._rotation, p.transform);
                        //go.transform.localScale = tilesList[tiles[i]._tileValues[id]]._scale;
                        //go.name = tilesList[tiles[i]._tileValues[id]].GetName();



                    }
                }
            }

    }

    public void GenerateAllTiles()
    {
        if (outputTransform == null)
            outputTransform = new GameObject("WFC_output_overlapping").transform;
        if (parent != null)
            outputTransform.parent = parent;

        int aID = 5;

        Vector3 tileoffset = offset + Vector3.right * (gridWidth * tileSize + (N + 1) * tileSize);
        for (int i = 0; i < tiles.Length; i++)
        {
            for (int z = 0; z < N; z++)
            {
                for (int y = 0; y < N_depth; y++)
                {
                    for (int x = 0; x < N; x++)
                    {
                        int id = x + y * N * N + z * N;
                        GameObject go = UnityEngine.Object.Instantiate(tilesList[tiles[i]._tileValues[id]]._tileGameObject, new Vector3(x, y, z) * tileSize + tileoffset, tilesList[tiles[i]._tileValues[id]]._rotation, outputTransform);
                        go.transform.localScale = tilesList[tiles[i]._tileValues[id]]._scale;
                        go.name = tilesList[tiles[i]._tileValues[id]].GetName();



                    }
                }
            }

            /*Vector3 vertOffset = tileoffset + Vector3.up * (N_depth + 1) * tileSize;
            for (int j = 0; j < tiles[i]._tileAdjacencies[aID].Length; j++)
            {
                for (int z = 0; z < N; z++)
                {
                    for (int y = 0; y < N_depth; y++)
                    {
                        for (int x = 0; x < N; x++)
                        {
                            int id = x + y * N * N + z * N;
                            GameObject go = Object.Instantiate(tilesList[tiles[tiles[i]._tileAdjacencies[aID][j]]._tileValues[id]]._tileGameObject, new Vector3(x, y, z) * tileSize + vertOffset, tilesList[tiles[tiles[i]._tileAdjacencies[aID][j]]._tileValues[id]]._rotation, outputTransform);
                            go.transform.localScale = tilesList[tiles[tiles[i]._tileAdjacencies[aID][j]]._tileValues[id]]._scale;



                        }
                    }
                }
                vertOffset += Vector3.up * (N_depth + 1) * tileSize;
            }*/

            tileoffset += Vector3.right * (N + 1) * tileSize;
        }
    }

    private void CountWeights(Tile tile)
    {
        if (tilesDictionary.ContainsKey(tile._index))
            tilesDictionary[tile._index]._weight++;
        else
        {
            tilesDictionary.Add(tile._index, tile);
            tilesDictionary[tile._index]._weight = 1;
        }
    }

    public override void GenerateOutput()
    {
        

        for (int z = 0; z < gridLength; z++)
            for (int y = 0; y < gridDepth; y++)
                for (int x = 0; x < gridWidth; x++)
                {
                    /// REMEMBER ABOUT chunkGeneration - DIFFERENT TYPE OF INSTANTIATE
                    int id = ID(x, y, z);
                    if (grid[id].GetTile() != null)
                    {
                        GameObject go = UnityEngine.Object.Instantiate(tilesList[grid[id].GetTile()._tileValues[0]]._tileGameObject, new Vector3(x, y, z) * tileSize + offset, tilesList[grid[id].GetTile()._tileValues[0]]._rotation, outputTransform);
                        go.transform.localScale = tilesList[grid[id].GetTile()._tileValues[0]]._scale;
                    }
                    else
                    {
                        int x2 = Mathf.Min(x, gridWidth - N);
                        int y2 = Mathf.Min(y, gridDepth - N_depth);
                        int z2 = Mathf.Min(z, gridLength - N);

                        int x_id = x - x2;
                        int y_id = y - y2;
                        int z_id = z - z2;

                        id = ID(x2, y2, z2);
                        int t_id = y_id * N * N + z_id * N + x_id;

                        if (grid[id].GetTile() == null)
                            return;

                        GameObject go = UnityEngine.Object.Instantiate(tilesList[grid[id].GetTile()._tileValues[t_id]]._tileGameObject, new Vector3(x, y, z) * tileSize + offset, tilesList[grid[id].GetTile()._tileValues[t_id]]._rotation, outputTransform);
                        go.transform.localScale = tilesList[grid[id].GetTile()._tileValues[t_id]]._scale;
                    }

                }
        
        Debug.Log("output generation: overlapping");
    }

    public override bool OnBorder(int x, int y, int z)
    {
        return (x < 0 || y < 0 || z < 0 || 
                x > gridWidth - N || y > gridDepth - N_depth || z > gridLength - N);
    }

    public Tile RotateTile(Tile tile)
    {
        Quaternion rotation = tile._rotation;
        float rotationY = rotation.eulerAngles.y;

        int[] newValues = new int[tile._tileValues.Length];
        for (int x = 0; x < N; x++)
            for (int y = 0; y < N_depth; y++)
                for (int z = 0; z < N; z++)
                {
                    int id = x + y * N * N + z * N;
                    int rotatedId = N - z - 1 + x * N + y * N * N;
                    // change value!
                    newValues[id] = tile._tileValues[rotatedId];     // 90 clockwise
                    Tile rotatedModule = TilesManager.RotateTile(tilesList[newValues[id]]); // rotated tile model/module
                    if (rotatedModule != null && !tilesList.Exists(t => t.GetName() == rotatedModule.GetName()))
                    {
                        tilesList.Add(rotatedModule);
                        newValues[id] = (tilesList.Count - 1);
                    }
                    else if (rotatedModule != null)
                        newValues[id] = (tilesList.IndexOf(tilesList.Find(t => t.GetName() == rotatedModule.GetName())));
                }

        return new Tile(newValues, rotationY + 90f, 1f);
    }

    public Tile ReflectTile(Tile tile)
    {
        Quaternion rotation = tile._rotation;
        float rotationY = rotation.eulerAngles.y;

        int[] newValues = new int[tile._tileValues.Length];
        for (int x = 0; x < N; x++)
            for (int y = 0; y < N_depth; y++)
                for (int z = 0; z < N; z++)
                {
                    int id = x + y * N * N + z * N;
                    int reflectedId = N - x - 1 + z * N + y * N * N;
                    newValues[id] = tile._tileValues[reflectedId];
                    Tile reflectedModule = TilesManager.ReflectTile(tilesList[newValues[id]], true); // rotated tile model/module

                    if (reflectedModule != null && !tilesList.Exists(t => t.GetName() == reflectedModule.GetName()))
                    {
                        tilesList.Add(reflectedModule);
                        newValues[id] = (tilesList.Count - 1);

                    }
                    else if (reflectedModule != null)
                    {
                        newValues[id] = (tilesList.IndexOf(tilesList.Find(t => t.GetName() == reflectedModule.GetName())));
                    }

                }
        return new Tile(newValues, rotationY, -1f);
    }

    public bool CheckAdjacencies(Tile tileA, Tile tileB, int d)
    {
        int dirX = dir[d, 0];
        int dirY = dir[d, 1];
        int dirZ = dir[d, 2];

        int minX = (dirX < 0) ? 0 : dirX;
        int minZ = (dirZ < 0) ? 0 : dirZ;
        int minY = (dirY < 0) ? 0 : dirY;
        int maxX = Mathf.Min(dirX + N, N);
        int maxY = Mathf.Min(dirY + N_depth, N_depth);
        int maxZ = Mathf.Min(dirZ + N, N);

        for (int z = minZ; z < maxZ; z++)
            for (int y = minY; y < maxY; y++)
                for (int x = minX; x < maxX; x++)
                {
                    if (tileA._tileValues[y * N * N + z * N + x] != tileB._tileValues[(y - dirY) * N * N + (z - dirZ) * N + (x - dirX)])
                        return false;
                }

        return true;
    }

    private void PrintInputMap(int[][][] inputMap)
    {
        int W = inputMap.Length;        // x
        int D = inputMap[0].Length;     // y   
        int L = inputMap[0][0].Length;  // z

        string map = string.Empty;
        for (int z = 0; z < L; z++)
        {
            for (int x = 0; x < W; x++)
            {
                map += inputMap[x][0][z].ToString() + " ";
            }

        }
        Debug.Log(map);
    }

}