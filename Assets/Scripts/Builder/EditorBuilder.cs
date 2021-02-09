using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

public class EditorBuilder : MonoBehaviour
{
    public Vector3Int dimensions = Vector3Int.zero;
    public Tile[][][] outputMap;
    string[] tilesNames;

    private Material transparentMat;
    [HideInInspector] public Transform WFC_output;
    [HideInInspector] public GameObject currentTileGO;
    [HideInInspector] public Tile[] tiles;
    public string tilesetName;
    private GameObject collidersParent = null;
    private GameObject tilesParent = null;
    private GameObject plane = null;
    private GameObject highlightCube = null;
    private GameObject highlightPlane = null;
    private GameObject highlightCurrentTileGO = null;
    private Vector3 invisiblePos = new Vector3(-999f, 0f, 0f);
    private int currentTileIndex = 0;
    [HideInInspector] public string saveFileName = string.Empty;

    /// WFC SETTINGS
    [HideInInspector] public bool seamless = false;
    [HideInInspector] public bool processTiles = true;
    [HideInInspector] public int tileSize = 4;
    [HideInInspector] public Vector3 offset;
    // OVERLAPPING
    [HideInInspector] public int N = 3;
    [HideInInspector] public int N_depth = 2;
    [HideInInspector] public Vector3Int outputSize = new Vector3Int(10, 1, 10);
    [HideInInspector] public bool overlapTileCreation = true;

    public void Init()
    {
        currentTileIndex = 0;
        if (tiles != null && tiles.Length > 0)
            currentTileGO = tiles[currentTileIndex]._tileGameObject;

        while (transform.childCount > 0)
            DestroyImmediate(transform.GetChild(0).gameObject);

        transparentMat = Resources.Load<Material>("TransparentMaterial");

        // Start objects check
        if (collidersParent == null)
        {
            collidersParent = new GameObject("Colliders");
            collidersParent.transform.parent = transform;
        }
        else
        {
            DestroyImmediate(collidersParent);
            collidersParent = new GameObject("Colliders");
            collidersParent.transform.parent = transform;
        }

        if (tilesParent == null)
        {
            tilesParent = new GameObject("Tiles");
            tilesParent.transform.parent = transform;
        }
        else
        {
            DestroyImmediate(tilesParent);
            tilesParent = new GameObject("Tiles");
            tilesParent.transform.parent = transform;
        }

        if (plane == null)
        {
            plane = Instantiate(Resources.Load<GameObject>("BuilderPrefabs\\Plane"), new Vector3((dimensions.x - 1) * 0.5f, -0.55f, (dimensions.z - 1) * 0.5f) * tileSize, Quaternion.identity, transform);
            plane.transform.localScale = new Vector3(dimensions.x, 1f, dimensions.z) * tileSize * 0.1f;
        }
        else
        {
            DestroyImmediate(plane);
            plane = Instantiate(Resources.Load<GameObject>("BuilderPrefabs\\Plane"), new Vector3((dimensions.x - 1) * 0.5f, -0.55f, (dimensions.z - 1) * 0.5f) * tileSize, Quaternion.identity, transform);
            plane.transform.localScale = new Vector3(dimensions.x, 1f, dimensions.z) * tileSize * 0.1f;
        }

        if (highlightCube == null)
        {
            highlightCube = Instantiate(Resources.Load<GameObject>("BuilderPrefabs\\HighlightCube"), invisiblePos, Quaternion.identity, transform);
            highlightCube.transform.localScale *= (tileSize + 0.05f);
        }
        if (highlightPlane == null)
        {
            highlightPlane = Instantiate(Resources.Load<GameObject>("BuilderPrefabs\\HighlightPlane"), invisiblePos, Quaternion.identity, transform);
            highlightPlane.transform.localScale *= tileSize;
        }
        if (highlightCurrentTileGO == null && currentTileGO != null && tiles != null)
        {
            highlightCurrentTileGO = Instantiate(currentTileGO, invisiblePos, tiles[currentTileIndex]._rotation, transform);
            Renderer[] renderers = highlightCurrentTileGO.transform.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
                renderers[i].material = transparentMat;
        }


        // Colliders instantiate
        for (int z = 0; z < dimensions.z; z++)
        {
            for (int x = 0; x < dimensions.x; x++)
            {
                GameObject go = Instantiate(Resources.Load<GameObject>("BuilderPrefabs\\colliderPrefab"), new Vector3(x, -1f, z) * tileSize, Quaternion.identity, collidersParent.transform);
                go.transform.localScale *= tileSize;
                go.name = "collider x" + x.ToString() + " z" + z.ToString();
            }
        }

        // Output map init
        outputMap = new Tile[dimensions.x][][];
        tilesNames = new string[dimensions.x  * dimensions.y * dimensions.z];
        for (int x = 0; x < dimensions.x; x++)
        {
            outputMap[x] = new Tile[dimensions.y][];
            for (int y = 0; y < dimensions.y; y++)
            {
                outputMap[x][y] = new Tile[dimensions.z];
                for (int z = 0; z < dimensions.z; z++)
                    tilesNames[ID(x, y, z)] = string.Empty;
            }
        }

    }

    

    public void HighlightPrefabsManagement(RaycastHit rHit)
    {
        if (highlightCube == null || highlightPlane == null)
            return;
        if (rHit.transform == null)
        {
            highlightCube.transform.position = invisiblePos;
            highlightPlane.transform.position = invisiblePos;
            if (highlightCurrentTileGO != null)
                highlightCurrentTileGO.transform.position = invisiblePos;
            return;
        }

        Vector3 gridPos = rHit.transform.position;
        highlightCube.transform.position = gridPos;
        highlightPlane.transform.position = gridPos + rHit.normal * tileSize * 0.51f;
        highlightPlane.transform.LookAt(highlightPlane.transform.position + rHit.normal);
        // Tile prefab highlight
        Vector3 pos = rHit.transform.position;
        Vector3 normal = rHit.normal;

        Vector3 spawnPos = pos + normal * tileSize;
        Vector3 id = spawnPos / tileSize;

        if (highlightCurrentTileGO == null)
            return;

        if (id.x < 0 || id.y < 0 || id.z < 0 ||
            id.x >= dimensions.x || id.y >= dimensions.y || id.z >= dimensions.z)
        {
            highlightCurrentTileGO.transform.position = invisiblePos;
            return;
        }

        highlightCurrentTileGO.transform.position = spawnPos;
    }

    public void CreateTile(RaycastHit rHit)
    {
        if (rHit.transform == null || currentTileGO == null || tiles == null)
            return;

        Vector3 pos = rHit.transform.position;
        Vector3 normal = rHit.normal;

        Vector3 spawnPos = pos + normal * tileSize;
        Vector3 idFloat = spawnPos / tileSize;
        Vector3Int id = new Vector3Int(Mathf.RoundToInt(idFloat.x), Mathf.RoundToInt(idFloat.y), Mathf.RoundToInt(idFloat.z));

        if (id.x < 0 || id.y < 0 || id.z < 0 ||
            id.x >= dimensions.x || id.y >= dimensions.y || id.z >= dimensions.z)
            return;

        GameObject tile = Instantiate(currentTileGO, spawnPos, tiles[currentTileIndex]._rotation, tilesParent.transform);
        tile.name = currentTileGO.name + tile.transform.rotation.eulerAngles.ToString() + " " + tile.transform.localScale.ToString();
        GameObject collider = Instantiate(Resources.Load<GameObject>("BuilderPrefabs\\colliderPrefab"), spawnPos, Quaternion.identity, tile.transform);
        collider.transform.localScale *= tileSize;
        outputMap[id.x][id.y][id.z] = tiles[currentTileIndex];
        tilesNames[ID(id.x, id.y, id.z)] = tiles[currentTileIndex].GetName();
    }

    public void CreateTile(Vector3Int id, int tileID)
    {
        if (tileID < 0 || tiles == null || id.x < 0 || id.y < 0 || id.z < 0 ||
            id.x >= dimensions.x || id.y >= dimensions.y || id.z >= dimensions.z)
            return;

        Vector3 spawnPos = id * tileSize;

        currentTileGO = TilesManager.tilesTiled[tileID]._tileGameObject;
        GameObject tile = Instantiate(currentTileGO, spawnPos, TilesManager.tilesTiled[tileID]._rotation, tilesParent.transform);
        tile.name = currentTileGO.name + tile.transform.rotation.eulerAngles.ToString() + " " + tile.transform.localScale.ToString();
        GameObject collider = Instantiate(Resources.Load<GameObject>("BuilderPrefabs\\colliderPrefab"), spawnPos, Quaternion.identity, tile.transform);
        collider.transform.localScale *= tileSize;
        outputMap[id.x][id.y][id.z] = TilesManager.tilesTiled[tileID];
        tilesNames[ID(id.x, id.y, id.z)] = TilesManager.tilesTiled[tileID].GetName();
    }

    public void DestroyTile(RaycastHit rHit)
    {
        if (rHit.transform == null || outputMap == null || rHit.transform.position.y < 0f)
            return;

        Vector3 pos = rHit.transform.position;
        Vector3 idFloat = pos / tileSize;
        Vector3Int id = new Vector3Int(Mathf.RoundToInt(idFloat.x), Mathf.RoundToInt(idFloat.y), Mathf.RoundToInt(idFloat.z));

        if (outputMap[id.x][id.y][id.z] == null)
            return;
        
        outputMap[id.x][id.y][id.z] = null;
        tilesNames[ID(id.x, id.y, id.z)] = string.Empty;
        DestroyImmediate(rHit.transform.parent.gameObject);
    }

    public void OnTilePrefabChange(int index)
    {
        if (tiles[index] == null)
            return;
        currentTileIndex = index;

        currentTileGO = tiles[currentTileIndex]._tileGameObject;
        if (highlightCurrentTileGO != null)
            DestroyImmediate(highlightCurrentTileGO);
        highlightCurrentTileGO = Instantiate(currentTileGO, invisiblePos, tiles[currentTileIndex]._rotation, transform);

        Renderer[] renderers = highlightCurrentTileGO.transform.GetComponentsInChildren<Renderer>();
        transparentMat.SetTexture("_MainTex", renderers[0].sharedMaterial.GetTexture("_MainTex"));
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].material = transparentMat;


    }


    public void RotateTile()
    {
        if (tiles == null || highlightCurrentTileGO == null)
            return;

        Tile rotationTile = TilesManager.RotateTile(tiles[currentTileIndex]);
        if (rotationTile == null)
            return;

        tiles[currentTileIndex] = rotationTile;
        highlightCurrentTileGO.transform.rotation = tiles[currentTileIndex]._rotation;
    }

    public void GenerateOverlapping()
    {
        if (IsOutputEmpty())
            return;
        
        if (WFC_output != null)
            DestroyImmediate(WFC_output.gameObject);

        if (outputSize.x < outputMap.Length || outputSize.y < outputMap[0].Length || outputSize.z < outputMap[0][0].Length)
        {
            Debug.Log("Output size is smaller than input! Modyfing output size.");
            outputSize = new Vector3Int(Math.Max(outputSize.x, outputMap.Length), Math.Max(outputSize.y, outputMap[0].Length), Math.Max(outputSize.z, outputMap[0][0].Length));
        }
        WFC_Generator.GenerateOverlapping(outputSize, tileSize, N, N_depth, processTiles, outputMap, offset, overlapTileCreation, transform);
        WFC_output = WFC_Generator.outputTransform;
    }

    public void GenerateTiled() // based on user's input
    {
        if (WFC_output != null)
            DestroyImmediate(WFC_output.gameObject);

        WFC_Generator.AutoFillTiled(dimensions, tileSize, seamless, processTiles, tilesetName, outputMap, transform);
        WFC_output = WFC_Generator.outputTransform;
        
    }
    public void LoadTiles()
    {
        if (!TilesManager.LoadTilesTiled(tilesetName, false, false))
            return;

        tiles = new Tile[TilesManager.tilesTiled.Length];
        tileSize = (TilesManager.tileSize <= 0) ? 2 : TilesManager.tileSize;

        for (int i = 0; i < TilesManager.tilesTiled.Length; i++)
        {
            tiles[i] = new Tile(TilesManager.tilesTiled[i]);
        }
    }

    public void SaveDataFile()
    {
        SaveData saveData = new SaveData();
        saveData.tilesetName = tilesetName;
        saveData.dimensions = new Vector3Json(dimensions.x, dimensions.y, dimensions.z);
        saveData.seamless = seamless;
        saveData.processTiles = processTiles;
        saveData.offset = new Vector3Json(offset.x, offset.y, offset.z);
        saveData.N = N;
        saveData.N_depth = N_depth;
        saveData.outputSize = new Vector3Json(outputSize.x, outputSize.y, outputSize.z);
        saveData.overlapTileCreation = overlapTileCreation;

        TilesManager.LoadTilesTiled(tilesetName, true, false);

        int[] builderMap = new int[tilesNames.Length];
        for (int i = 0; i < tilesNames.Length; i++)
        {
            builderMap[i] = -1;
            if (tilesNames[i] == string.Empty)
                continue;

            for (int j = 0; j < TilesManager.tilesTiled.Length; j++)
            {
                if (TilesManager.tilesTiled[j].GetName() == tilesNames[i])
                {
                    builderMap[i] = j;
                    break;
                }
            }

        }

        saveData.builderMap = builderMap;

        string json = JsonUtility.ToJson(saveData);

        string path = Application.dataPath + "\\Resources\\SaveFiles\\" + saveFileName + ".json";
        
        using (StreamWriter sw = File.CreateText(path))
            sw.WriteLine(json);
        AssetDatabase.Refresh();
    }

    public void LoadDataFile()
    {
        string path = Application.dataPath + "\\Resources\\SaveFiles\\" + saveFileName + ".json";

        if (!File.Exists(path))
        {
            Debug.LogWarning("File does not exist");
            return;
        }

        string json = Resources.Load<TextAsset>("SaveFiles\\" + saveFileName).text;

        SaveData saveData = JsonUtility.FromJson<SaveData>(json);
        tilesetName = saveData.tilesetName;
        dimensions = saveData.dimensions;
        seamless = saveData.seamless;
        processTiles = saveData.processTiles;
        offset = saveData.offset;
        N = saveData.N;
        N_depth = saveData.N_depth;
        outputSize = saveData.outputSize;
        overlapTileCreation = saveData.overlapTileCreation;
        int[] newBuilderMap = saveData.builderMap;

        LoadOutputMap(newBuilderMap);
    }

    public void LoadOutputMap(int[] newBuilderMap)
    {
        Init();
        TilesManager.LoadTilesTiled(tilesetName, true, false);

        for (int i = 0; i < newBuilderMap.Length; i++)
        {
            int[] pos = FromID(i);
            CreateTile(new Vector3Int(pos[0], pos[1], pos[2]), newBuilderMap[i]);
        }

        LoadTiles();
    }

    protected int ID(int x, int y, int z)
    {
        return x + y * dimensions.x * dimensions.z + z * dimensions.x;
    }

    public int[] FromID(int ID)
    {
        int y = ID / (dimensions.x * dimensions.z);
        ID -= y * (dimensions.x * dimensions.z);
        int x = ID % dimensions.x;
        int z = ID / dimensions.x;

        return new int[] { x, y, z };
    }

    private void PrintInputMap()
    {
        int W = outputMap.Length;        // x
        int D = outputMap[0].Length;     // y   
        int L = outputMap[0][0].Length;  // z

        string map = string.Empty;
        for (int z = 0; z < L; z++)
        {
            for (int x = 0; x < W; x++)
            {
                map += outputMap[x][0][z].ToString() + " ";
            }

        }
        Debug.Log(map);
    }

    private bool IsOutputEmpty()
    {
        if (outputMap == null)
            return true;

        bool empty = true;

        int W = outputMap.Length;        // x
        int D = outputMap[0].Length;     // y   
        int L = outputMap[0][0].Length;  // z

        string map = string.Empty;
        for (int z = 0; z < L; z++)
            for (int y = 0; y < D; y++)
                for (int x = 0; x < W; x++)
                {
                    if (outputMap[x][y][z] != null)
                    {
                        empty = false;
                        break;
                    }
                }
        return empty;
    }
}
