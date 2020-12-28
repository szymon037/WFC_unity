/// TODO:
/// - tiles processing
/// - drawing whole tiles on edges

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class OverlappingModel : Model
{
    private int W, H;
    byte[,] indexMap;
    int N;
    private Dictionary<long, Tile> tilesDictionary = new Dictionary<long, Tile>();
    public List<GameObject> gameObjects = new List<GameObject>();
    int ae = 0;
    public OverlappingModel(int gridWidth, int gridHeight, int tileSize, bool seamless, int N, bool tileProcessing, GameObject[][] inputMap) : base(gridWidth, gridHeight, tileSize, seamless)
    {
        W = inputMap.Length;
        H = inputMap[0].Length;
        indexMap = new byte[W, H];
        this.N = N;

        for (int y = 0; y < H; y++)
        {
            for (int x = 0; x < W; x++)
            {
                int i = 0;
                GameObject currentGameObject = inputMap[x][y];

                for (int c = 0; c < gameObjects.Count; c++)
                {
                    if (currentGameObject.name == gameObjects[c].name)
                    {
                        //Debug.Log("same go: " + currentGameObject.name);
                        break;
                    }
                    ++i;
                }

                if (gameObjects.Count == i) gameObjects.Add(currentGameObject);
                indexMap[x, y] = (byte)i;
            }
        }
        Debug.Log("gameObjects.Count: " + gameObjects.Count);

        // tile creation
        List <Tile> newTiles = new List<Tile>();
        for (int y = 0; y < H; y++)
        {
            for (int x = 0; x < W; x++)
            {
                Tile tile = CreateTile(x, y, N, gameObjects.Count);
                CountWeights(tile);
            }
        }
        //newTiles = tilesDictionary.Values.ToList();
        Debug.Log("Tiles count: " + newTiles.Count);
        if (tileProcessing)
            ProcessTiles(); // creates rotations and reflections
        //Debug.Log("ae: " + ae);
        newTiles = tilesDictionary.Values.ToList();
        //newTiles.RemoveAt(newTiles.Count - 1);
        Debug.Log("Tiles count: " + newTiles.Count);

        for (int i = 0; i < newTiles.Count; i++)
        {
            for (int d = 0; d < 4; d++)
            {
                List<int> l = new List<int>();
                for (int j = 0; j < newTiles.Count; j++)
                {
                    if (CheckAdjacencies(newTiles[i], newTiles[j], d))
                        l.Add(j);
                }
                newTiles[i]._adjacencies[d] = l.ToArray();
            }
        }

        tiles = newTiles.ToArray();
        InitGrid();
        Init();
    }

    private void ProcessTiles()
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

    private Tile CreateTile(int x, int y, int N, int gameObjectCount)
    {
        byte[] map = new byte[N * N];
        for (int i_y = 0; i_y < N; i_y++)
        {
            for (int i_x = 0; i_x < N; i_x++)
            {
                map[i_y * N + i_x] = indexMap[(x + i_x) % W, (y + i_y) % H];
            }
        }

        return new Tile(map, N, gameObjectCount);
    }

    private void CountWeights(Tile tile)
    {
        if (tilesDictionary.ContainsKey(tile._index))
        {
            tilesDictionary[tile._index]._weight++;
            ae++;
        }
        else
        {
            tilesDictionary.Add(tile._index, tile);
            tilesDictionary[tile._index]._weight = 1;
        }
    }

    public override void GenerateOutput()
    {
        // instantiae GOs

        Vector3 offset = new Vector3(50f, 0f, 00f);

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (grid[y * gridWidth + x].GetTile() != null)
                {
                    GameObject go = Instantiate(gameObjects[grid[y * gridWidth + x].GetTile()._tileValues[0]], new Vector3(x, 0f, y) * tileSize + offset, grid[y * gridWidth + x].GetTile()._transform.rotation * gameObjects[grid[y * gridWidth + x].GetTile()._tileValues[0]].transform.rotation);
                    go.transform.localScale = new Vector3(1f, 1f, grid[y * gridWidth + x].GetTile()._transform.lossyScale.z * gameObjects[grid[y * gridWidth + x].GetTile()._tileValues[0]].transform.localScale.z);
                }

            }
        }

        Debug.Log("output generation: overlapping");
    }

    public override bool OnBorder(int x, int y)
    {
        return (x < 0 || y < 0 || x > gridWidth - N || y > gridHeight - N);
    }

    public Tile RotateTile(Tile tile)
    {
        byte[] newValues = new byte[tile._tileValues.Length];
        for (int x = 0; x < N; x++)
        {
            for (int y = 0; y < N; y++)
            {
                newValues[y * N + x] = tile._tileValues[N - y - 1 + x * N];     // 90 clockwise
            }
        }
        return new Tile(newValues, 90f, 1f);
    }

    public Tile ReflectTile(Tile tile)
    {
        byte[] newValues = new byte[tile._tileValues.Length];
        for (int x = 0; x < N; x++)
        {
            for (int y = 0; y < N; y++)
            {
                newValues[y * N + x] = tile._tileValues[N - x - 1 + y * N];
            }
        }
        return new Tile(newValues, 0f, -1f);
    }

    public bool CheckAdjacencies(Tile tileA, Tile tileB, int d)
    {
        int dirX = dir[d, 0];
        int dirY = dir[d, 1];

        int minX = (dirX < 0) ? 0 : dirX;
        int minY = (dirY < 0) ? 0 : dirY;
        int maxX = Mathf.Min(dirX + N, N);
        int maxY = Mathf.Min(dirY + N, N);

        //Debug.Log("tileA._tileValues.Length: " + N);

        for (int y = minY; y < maxY; y++)
        {
            for (int x = minX; x < maxX; x++)
            {
                if (tileA._tileValues[y * N + x] != tileB._tileValues[(y - dirY) * N + (x - dirX)])
                    return false;
            }
        }

        return true;
    }

}