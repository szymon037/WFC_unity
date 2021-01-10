using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class EditorBuilder : MonoBehaviour
{
    public Vector3Int dimensions = Vector3Int.zero;
    public int tileSize = 2;
    public GameObject[][][] outputMap;
    public Material transparentMat; ///TODO: load material from resources
    [HideInInspector] public GameObject currentTileGO;
    //[HideInInspector] public GameObject[] gameObjects;
    [HideInInspector] public Tile[] tiles;
    public string tilesetName;

    private GameObject collidersParent = null;
    private GameObject tilesParent = null;
    private GameObject plane = null;
    private GameObject highlightCube = null;
    private GameObject highlightPlane = null;
    private GameObject highlightCurrentTileGO = null;
    private Vector3 invisiblePos = new Vector3(-999f, 0f, 0f);
    //private Quaternion tileRotation = Quaternion.identity;
    private int currentTileIndex = 0;
    public void Init()
    {
        // load tiles from file
        // if no .xml file, load tiles without adjacencies
        // COPY tiles
        // instead of choosing gameobject, choose tile
        

        foreach (Transform t in transform)
            DestroyImmediate(t.gameObject);
        //tileRotation = Quaternion.identity;
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
            highlightCube.transform.localScale *= tileSize;
        }
        if (highlightPlane == null)
        {
            highlightPlane = Instantiate(Resources.Load<GameObject>("BuilderPrefabs\\HighlightPlane"), invisiblePos, Quaternion.identity, transform);
            highlightPlane.transform.localScale *= tileSize;
        }
        if (highlightCurrentTileGO == null && currentTileGO != null)
        {
            highlightCurrentTileGO = Instantiate(currentTileGO, invisiblePos, tiles[currentTileIndex]._transform.rotation, transform);
            foreach (Transform child in highlightCurrentTileGO.transform)
                child.GetComponent<Renderer>().material = transparentMat;
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
        outputMap = new GameObject[dimensions.x][][];
        for (int x = 0; x < dimensions.x; x++)
        {
            outputMap[x] = new GameObject[dimensions.y][];
            for (int y = 0; y < dimensions.y; y++)
            {
                outputMap[x][y] = new GameObject[dimensions.z];
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
            highlightCurrentTileGO.transform.position = invisiblePos;
            return;
        }

        Vector3 gridPos = rHit.transform.position;
        highlightCube.transform.position = gridPos;
        highlightPlane.transform.position = gridPos + rHit.normal * tileSize * 0.5f;
        highlightPlane.transform.LookAt(highlightPlane.transform.position + rHit.normal);
        // Tile prefab highlight
        /// TODO: APPLY ROTATION
        Vector3 pos = rHit.transform.position;
        Vector3 normal = rHit.normal;

        Vector3 spawnPos = pos + normal * tileSize;
        Vector3 id = spawnPos / tileSize;

        if (id.x < 0 || id.y < 0 || id.z < 0 ||
            id.x >= dimensions.x || id.y >= dimensions.y || id.z >= dimensions.z || 
            highlightCurrentTileGO == null)
        {
            highlightCurrentTileGO.transform.position = invisiblePos;
            return;
        }

        highlightCurrentTileGO.transform.position = spawnPos;
    }

    public void CreateTile(RaycastHit rHit)
    {
        if (rHit.transform == null || currentTileGO == null)
            return;

        Vector3 pos = rHit.transform.position;
        Vector3 normal = rHit.normal;

        Vector3 spawnPos = pos + normal * tileSize;
        Vector3 id = spawnPos / tileSize;

        if (id.x < 0 || id.y < 0 || id.z < 0 ||
            id.x >= dimensions.x || id.y >= dimensions.y || id.z >= dimensions.z)
            return;

        GameObject tile = Instantiate(currentTileGO, spawnPos, tiles[currentTileIndex]._transform.rotation, tilesParent.transform);
        tile.name = currentTileGO.name + tile.transform.rotation.eulerAngles.ToString() + " " + tile.transform.localScale.ToString();
        GameObject collider = Instantiate(Resources.Load<GameObject>("BuilderPrefabs\\colliderPrefab"), spawnPos, Quaternion.identity, collidersParent.transform);
        collider.transform.localScale *= tileSize;

        outputMap[(int)id.x][(int)id.y][(int)id.z] = tile;
    }

    public void DestroyTile(RaycastHit rHit)
    {
        if (rHit.transform == null || outputMap == null || rHit.transform.position.y < 0f)
            return;

        Vector3 pos = rHit.transform.position;
        Vector3 id = pos / tileSize;

        if (outputMap[(int)id.x][(int)id.y][(int)id.z] == null)
            return;
        
        DestroyImmediate(outputMap[(int)id.x][(int)id.y][(int)id.z]);
        DestroyImmediate(rHit.transform.gameObject);
        outputMap[(int)id.x][(int)id.y][(int)id.z] = null;
    }

    public void OnTilePrefabChange(int index)
    {
        if (tiles[index] == null)
            return;
        currentTileIndex = index;

        currentTileGO = tiles[currentTileIndex]._tileGameObject;
        if (highlightCurrentTileGO != null)
            DestroyImmediate(highlightCurrentTileGO);
        highlightCurrentTileGO = Instantiate(currentTileGO, invisiblePos, tiles[currentTileIndex]._transform.rotation, transform);

        foreach (Transform child in highlightCurrentTileGO.transform)
            child.GetComponent<Renderer>().material = transparentMat;

    }


    public void RotateTile()
    {
        Tile rotationTile = TilesManager.RotateTile(tiles[currentTileIndex]);
        if (rotationTile == null)
        {
            Debug.Log("could not rotate");
            return;
        }

        tiles[currentTileIndex] = rotationTile;
        highlightCurrentTileGO.transform.rotation = tiles[currentTileIndex]._transform.rotation;
        //tileRotation *= Quaternion.Euler(Vector3.up * 90f);
        //highlightCurrentTileGO.transform.rotation = tileRotation;
    }

    public void GenerateOverlapping()
    {
        WFC_Generator.GenerateOverlapping(outputMap, new Vector3(20f, 0f, 0f), tileSize, true);
    }

    public void GenerateTiled() // based on user's input
    {
        WFC_Generator.AutoFillTiled(dimensions, outputMap, tileSize, tilesetName);
    }
    public void LoadTiles()
    {
        TilesManager.LoadTilesTiled(tilesetName, false);
        tiles = new Tile[TilesManager.tilesTiled.Length];

        for (int i = 0; i < TilesManager.tilesTiled.Length; i++)
        {
            tiles[i] = new Tile(TilesManager.tilesTiled[i]);
        }
    }
}
