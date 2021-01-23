/// TODO:
/// - tiles processing

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class OverlappingModel : Model
{
    private int W, L, D;
    byte[,,] indexMap;
    int N;
    int N_depth;
    private Dictionary<long, Tile> tilesDictionary = new Dictionary<long, Tile>();
    //public List<GameObject> gameObjects = new List<GameObject>();
    public List<Tile> tilesList = new List<Tile>();
    private bool overlapTileCreation = true;

    public OverlappingModel(int gridWidth, int gridDepth, int gridLength, int tileSize, bool seamless, int N, int N_depth, bool tileProcessing, Tile[][][] inputMap, Tile[] tileArray, bool overlapTileCreation, Transform parent = null) : base(gridWidth, gridDepth, gridLength, tileSize, seamless, parent)
    {

        W = inputMap.Length;        // x
        D = inputMap[0].Length;     // y   
        L = inputMap[0][0].Length;  // z
        indexMap = new byte[W, D, L];
        this.N = N;
        this.N_depth = N_depth;
        this.overlapTileCreation = overlapTileCreation;

        /// TODO: fill tileArray with empty tiles before starting WFC
        //tilesList = new List<Tile>(tileArray);

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

                    /*if (!tilesList.Exists(t => t.GetName() == currentTile.GetName()))
                    {
                        tilesList.Add(currentTile);
                        i = tilesList.Count - 1;
                    }*/

                    for (int c = 0; c < tilesList.Count; c++)
                    {
                        if (currentTile.GetName() == tilesList[c].GetName()) /// TODO: NULL (CAUSE: SOLUTION NOT FOUND IN TILED)
                        {
                            //Debug.Log("same go: " + currentGameObject.name);
                            break;
                        }
                        ++i;
                    }

                    if (tilesList.Count == i) tilesList.Add(currentTile);
                    indexMap[x, y, z] = (byte)i;
                    //indexMap[x, y, z] = (byte)inputMap[x][y][z];
                }
        //Debug.Log("gameObjects.Count: " + gameObjects.Count);

        // tile creation
        List <Tile> newTiles = new List<Tile>();
        for (int z = 0; z < L; z++)
            for (int y = 0; y < D; y++)
                for (int x = 0; x < W; x++)
                {
                    Tile tile = CreateTile(x, y, z);
                    //Tile tile = CreateTile(0, 0, 0);
                    if (tile != null)
                        newTiles.Add(tile);
                }

        List<Tile> processTiles = new List<Tile>();

        int maxProcessIndex = (tileProcessing) ? 8 : 1;
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
        /*Debug.Log("Tiles adj:");
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].PrintTileValues();
            for (int j = 0; j < 6; j++)
            {
                if (tiles[i]._tileAdjacencies[j].Length > 0 && j != 2 && j != 3)
                Debug.Log(tiles[i]._tileAdjacencies[j].Length);
            }
        }*/
        
        InitGrid();
        Init();
    }

    // Constructor for infinity generation
    /*public OverlappingModel(int gridWidth, int gridLength, int gridDepth, int tileSize, int N, int N_depth, bool tileProcessing, GameObject[][][] inputMap, int[][] neighbourCells, Transform parent = null) : base(gridWidth, gridLength, gridDepth, tileSize, false, parent)
    {
        chunkGeneration = true;

        W = inputMap.Length;        // x
        D = inputMap[0].Length;     // y   
        L = inputMap[0][0].Length;  // z
        indexMap = new byte[W, D, L];
        this.N = N;
        this.N_depth = N_depth;

        for (int z = 0; z < L; z++)
            for (int y = 0; y < D; y++)
                for (int x = 0; x < W; x++)
                {
                    int i = 0;
                    GameObject currentGameObject = inputMap[x][y][z];

                    for (int c = 0; c < gameObjects.Count; c++)
                    {
                        if (currentGameObject.name == gameObjects[c].name) /// TODO: NULL (CAUSE: SOLUTION NOT FOUND IN TILED)
                        {
                            break;
                        }
                        ++i;
                    }

                    if (gameObjects.Count == i) gameObjects.Add(currentGameObject);
                    indexMap[x, y, z] = (byte)i;
                }

        // tile creation
        List<Tile> newTiles = new List<Tile>();
        for (int z = 0; z < L; z++)     /// 3D: should tile creation overlap in Y?
            for (int y = 0; y < D; y++)
                for (int x = 0; x < W; x++)
                {
                    Tile tile = CreateTile(x, y, z);
                    if (tile != null)
                        newTiles.Add(tile);
                    //CountWeights(tile);
                }

        int maxProcessIndex = (tileProcessing) ? 8 : 1;
        foreach (Tile tile in newTiles)
        {
            Tile[] tilesToProcess = new Tile[8];
            tilesToProcess[0] = tile;
            if (tileProcessing)
            {
                tilesToProcess[1] = RotateTile(tilesToProcess[0]);
                tilesToProcess[2] = ReflectTile(tilesToProcess[0]);
                tilesToProcess[3] = RotateTile(tilesToProcess[2]);
                tilesToProcess[4] = ReflectTile(tilesToProcess[2]);
                tilesToProcess[5] = RotateTile(tilesToProcess[4]);
                tilesToProcess[6] = ReflectTile(tilesToProcess[4]);
                tilesToProcess[7] = ReflectTile(tilesToProcess[6]);
            }


            for (int i = 0; i < maxProcessIndex; i++)
                CountWeights(tilesToProcess[i]);
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
                newTiles[i].SetAdjacencies(d, l);
            }
        }

        tiles = newTiles.ToArray();
        InitGrid();
        Init();

        /// PARSING NEIGBOURS
        for (int i = 0; i < 4; i++)
        {
            if (neighbourCells[i] == null)
                continue;

            int dirIndex = (i > 1) ? i + 2 : i; // i'm so sorry
            if (dirIndex == 0 || dirIndex == 1) // L
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
    }*/

    private Tile CreateTile(int x, int y, int z)
    {
        if ((y + N_depth) > D)
            return null;

        if (!overlapTileCreation && ((x + N) > W || (y + N_depth) > D || (z + N) > L))
            return null;

        byte[] map = new byte[N * N_depth * N];

        for (int i_z = 0; i_z < N; i_z++)
            for (int i_y = 0; i_y < N_depth; i_y++)
                for (int i_x = 0; i_x < N; i_x++)
                    map[i_y * N * N + i_z * N + i_x] = indexMap[(x + i_x) % W, (y + i_y) % D, (z + i_z) % L];

        //bool ground = (y == 0) ? true : false;

        return new Tile(map, 0f, 1f/*, ground*/);
    }
    // L - 0, R - 1, U - 2, D - 3, F - 4, B - 5

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
                        GameObject go = Object.Instantiate(tilesList[tiles[i]._tileValues[id]]._tileGameObject, new Vector3(x, y, z) * tileSize + tileoffset, tilesList[tiles[i]._tileValues[id]]._rotation, outputTransform);
                        go.transform.localScale = tilesList[tiles[i]._tileValues[id]]._scale;
                        go.name = tilesList[tiles[i]._tileValues[id]].GetName();



                    }
                }
            }

            Vector3 vertOffset = tileoffset + Vector3.up * (N_depth + 1) * tileSize;
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
            }

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
        outputTransform = new GameObject("WFC_output_overlapping").transform;
        if (parent != null)
            outputTransform.parent = parent;

        for (int z = 0; z < gridLength; z++)
            for (int y = 0; y < gridDepth; y++)
                for (int x = 0; x < gridWidth; x++)
                {
                    /// TODO: REMEMBER ABOUT chunkGeneration - DIFFERENT TYPE OF INSTANTIATE
                    int id = ID(x, y, z);
                    if (grid[id].GetTile() != null)
                    {
                        //GameObject go = Object.Instantiate(gameObjects[grid[id].GetTile()._tileValues[0]], new Vector3(x, y, z) * tileSize + offset, grid[id].GetTile()._transform.rotation * gameObjects[grid[id].GetTile()._tileValues[0]].transform.rotation, chunkGO.transform);
                        GameObject go = Object.Instantiate(tilesList[grid[id].GetTile()._tileValues[0]]._tileGameObject, new Vector3(x, y, z) * tileSize + offset, tilesList[grid[id].GetTile()._tileValues[0]]._rotation, outputTransform);
                        go.transform.localScale = tilesList[grid[id].GetTile()._tileValues[0]]._scale;
                    }
                    else /// TODO: CHECK IF THIS TILE (x2, y2, z2) IS NULL AS WELL
                    {
                        int x2 = Mathf.Min(x, gridWidth - N);
                        int y2 = Mathf.Min(y, gridDepth - N_depth);
                        int z2 = Mathf.Min(z, gridLength - N);

                        int x_id = x - x2;
                        int y_id = y - y2;
                        int z_id = z - z2;

                        id = ID(x2, y2, z2);
                        int t_id = y_id * N * N + z_id * N + x_id;

                        GameObject go = Object.Instantiate(tilesList[grid[id].GetTile()._tileValues[t_id]]._tileGameObject, new Vector3(x, y, z) * tileSize + offset, tilesList[grid[id].GetTile()._tileValues[t_id]]._rotation, outputTransform);
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

        byte[] newValues = new byte[tile._tileValues.Length];
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
                        newValues[id] = (byte)(tilesList.Count - 1);
                    }
                    else if (rotatedModule != null)
                    {
                        newValues[id] = (byte)(tilesList.IndexOf(tilesList.Find(t => t.GetName() == rotatedModule.GetName())));
                        //Debug.Log("index: " + newValues[id]);
                        //Debug.Log("tile exists: " + rotatedModule.GetName());
                    }
                    /*if (rotatedModule == null)
                        Debug.Log("rotatedModule null");*/
                }
        return new Tile(newValues, rotationY + 90f, 1f);
    }

    public Tile ReflectTile(Tile tile)
    {
        Quaternion rotation = tile._rotation;
        float rotationY = rotation.eulerAngles.y;

        byte[] newValues = new byte[tile._tileValues.Length];
        for (int x = 0; x < N; x++)
            for (int y = 0; y < N_depth; y++)
                for (int z = 0; z < N; z++)
                {
                    int id = x + y * N * N + z * N;
                    int reflectedId = N - x - 1 + z * N + y * N * N;
                    newValues[id] = tile._tileValues[reflectedId];
                    //Debug.Log("rot b4 trflect: " + tilesList[newValues[id]]._transform.rotation.eulerAngles.ToString());
                    Tile reflectedModule = TilesManager.ReflectTile(tilesList[newValues[id]], true); // rotated tile model/module
                    //Debug.Log("rot after trflect: " + reflectedModule._transform.rotation.eulerAngles.ToString());

                    if (reflectedModule != null && !tilesList.Exists(t => t.GetName() == reflectedModule.GetName()))
                    {
                        tilesList.Add(reflectedModule);
                        newValues[id] = (byte)(tilesList.Count - 1);

                    }
                    else if (reflectedModule != null)
                    {
                        newValues[id] = (byte)(tilesList.IndexOf(tilesList.Find(t => t.GetName() == reflectedModule.GetName())));
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

        //Debug.Log("tileA._tileValues.Length: " + N);

        for (int z = minZ; z < maxZ; z++)
            for (int y = minY; y < maxY; y++)
                for (int x = minX; x < maxX; x++)
                {
                    if (tileA._tileValues[y * N * N + z * N + x] != tileB._tileValues[(y - dirY) * N * N + (z - dirZ) * N + (x - dirX)])
                        return false;
                }

        return true;
    }

    protected int ID_N(int x, int y, int z)
    {
        return x + y * N * N + z * N;
    }

    protected int[] FromID_N(int ID_N)
    {
        int y = ID_N / (N * N);
        ID_N -= y * (N * N);
        int x = ID_N % N;
        int z = ID_N / N;

        return new int[] { x, y, z };
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