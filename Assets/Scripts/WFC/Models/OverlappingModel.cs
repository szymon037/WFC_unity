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
    public List<GameObject> gameObjects = new List<GameObject>();
    private bool overlapTileCreation = true;
    public OverlappingModel(int gridWidth, int gridDepth, int gridLength, int tileSize, bool seamless, int N, int N_depth, bool tileProcessing, GameObject[][][] inputMap, bool overlapTileCreation) : base(gridWidth, gridDepth, gridLength, tileSize, seamless)
    {
        W = inputMap.Length;        // x
        D = inputMap[0].Length;     // y   
        L = inputMap[0][0].Length;  // z
        indexMap = new byte[W, D, L];
        this.N = N;
        this.N_depth = N_depth;
        this.overlapTileCreation = overlapTileCreation;

        GameObject empty = new GameObject("Empty"); // TODO: make a prefab and load it at start? cuz this GO stays in scene

        for (int z = 0; z < L; z++)
            for (int y = 0; y < D; y++)
                for (int x = 0; x < W; x++)
                {
                    int i = 0;
                    GameObject currentGameObject = inputMap[x][y][z];
                    if (currentGameObject == null)
                        currentGameObject = empty;

                    for (int c = 0; c < gameObjects.Count; c++)
                    {
                        if (currentGameObject.name == gameObjects[c].name) /// TODO: NULL (CAUSE: SOLUTION NOT FOUND IN TILED)
                        {
                            //Debug.Log("same go: " + currentGameObject.name);
                            break;
                        }
                        ++i;
                    }

                    if (gameObjects.Count == i) gameObjects.Add(currentGameObject);
                    indexMap[x, y, z] = (byte)i;
                }
        //Debug.Log("gameObjects.Count: " + gameObjects.Count);

        // tile creation
        List <Tile> newTiles = new List<Tile>();
        for (int z = 0; z < L; z++)     /// 3D: should tile creation overlap in Y?
            for (int y = 0; y < D; y++)
                for (int x = 0; x < W; x++)
                {
                    Tile tile = CreateTile(x, y, z);
                    if (tile != null)
                        newTiles.Add(tile);
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
                newTiles[i]._tileAdjacencies[d] = l.ToArray();
            }
        }

        tiles = newTiles.ToArray();
        InitGrid();
        Init();
    }

    // Constructor for infinity generation
    public OverlappingModel(int gridWidth, int gridLength, int gridDepth, int tileSize, int N, int N_depth, bool tileProcessing, GameObject[][][] inputMap, int[][] neighbourCells, GameObject chunkGO) : base(gridWidth, gridLength, gridDepth, tileSize, false, chunkGO)
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
                            //Debug.Log("same go: " + currentGameObject.name);
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

        /*if (tileProcessing)
            ProcessTiles(); // creates rotations and reflections*/
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
                newTiles[i]._tileAdjacencies[d] = l.ToArray();
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
    }

    /*private void ProcessTiles()
    {
        List<Tile> newTiles = new List<Tile>();
        foreach (KeyValuePair<long, Tile> entry in tilesDictionary)
        {
            Tile rotation = entry.Value;

            for (int j = 0; j < 4; j++)
            {
                newTiles.Add(rotation);
                newTiles.Add(ReflectTile(rotation));

                if (j < 3)
                    rotation = RotateTile(rotation);
            }
        }

        for (int i = 0; i < newTiles.Count; i++)
        {
            CountWeights(newTiles[i]);
        }
    }
    */
    private Tile CreateTile(int x, int y, int z)
    {
        if (!overlapTileCreation && ((x + N) >= W || (y + N_depth) >= D || (z + N) > L))
            return null;

        byte[] map = new byte[N * N_depth * N];

        for (int i_z = 0; i_z < N; i_z++)
            for (int i_y = 0; i_y < N_depth; i_y++)
                for (int i_x = 0; i_x < N; i_x++)
                    map[i_y * N * N + i_z * N + i_x] = indexMap[(x + i_x) % W, (y + i_y) % D, (z + i_z) % L];

        return new Tile(map, N, N_depth, gameObjects.Count, 0f, 1f);
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
        Transform parent = new GameObject("WFC_output_overlapping").transform;
        for (int z = 0; z < gridLength; z++)
            for (int y = 0; y < gridDepth; y++)
                for (int x = 0; x < gridWidth; x++)
                {
                    /// TODO: REMEMBER ABOUT chunkGeneration - DIFFERENT TYPE OF INSTANTIATE
                    int id = ID(x, y, z);
                    if (grid[id].GetTile() != null)
                    {
                        if (gameObjects[grid[id].GetTile()._tileValues[0]] == null)
                            Debug.Log("1");
                        if (grid[id].GetTile() == null)
                            Debug.Log("2");
                        //GameObject go = Object.Instantiate(gameObjects[grid[id].GetTile()._tileValues[0]], new Vector3(x, y, z) * tileSize + offset, grid[id].GetTile()._transform.rotation * gameObjects[grid[id].GetTile()._tileValues[0]].transform.rotation, chunkGO.transform);
                        GameObject go = Object.Instantiate(gameObjects[grid[id].GetTile()._tileValues[0]], new Vector3(x, y, z) * tileSize + offset, grid[id].GetTile()._transform.rotation * gameObjects[grid[id].GetTile()._tileValues[0]].transform.rotation, parent);
                        go.transform.localScale = new Vector3(1f, 1f, grid[id].GetTile()._transform.lossyScale.z * gameObjects[grid[id].GetTile()._tileValues[0]].transform.localScale.z);
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

                        GameObject go = Object.Instantiate(gameObjects[grid[id].GetTile()._tileValues[t_id]], new Vector3(x, y, z) * tileSize + offset, grid[id].GetTile()._transform.rotation * gameObjects[grid[id].GetTile()._tileValues[t_id]].transform.rotation, parent);
                        go.transform.localScale = new Vector3(1f, 1f, grid[id].GetTile()._transform.lossyScale.z * gameObjects[grid[id].GetTile()._tileValues[t_id]].transform.localScale.z);
                    }

                }


        Debug.Log("output generation: overlapping");
    }

    public override bool OnBorder(int x, int y, int z)
    {
        return (x < 0 || y < 0 || z < 0 || 
                x > gridWidth - N || y > gridDepth - N_depth || z > gridLength - N);
    }

    public Tile RotateTile(Tile tile) /// TODO 3D 
    {
        byte[] newValues = new byte[tile._tileValues.Length];
        for (int x = 0; x < N; x++)
        {
            for (int y = 0; y < N; y++)
            {
                newValues[y * N + x] = tile._tileValues[N - y - 1 + x * N];     // 90 clockwise
            }
        }
        return new Tile(newValues, N, N_depth, gameObjects.Count, 90f, 1f);
    }

    public Tile ReflectTile(Tile tile)  /// TODO 3D
    {
        byte[] newValues = new byte[tile._tileValues.Length];
        for (int x = 0; x < N; x++)
        {
            for (int y = 0; y < N; y++)
            {
                newValues[y * N + x] = tile._tileValues[N - x - 1 + y * N];
            }
        }
        return new Tile(newValues, N, N_depth, gameObjects.Count, 0f, -1f);
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

}