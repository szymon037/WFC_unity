using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Builder : MonoBehaviour
{
    public int width, depth, length;
    public GameObject colliderPrefab;
    public GameObject planePrefab;
    public GameObject[] tiles;
    public string setName;
    public int tileSize = 2;
    public GameObject[][][] outputMap;
    public GameObject highlightCube;
    public GameObject highlightPlane;
    public Material transparentMat;
    private int currentTileIndex = -1;
    [SerializeField]
    private GameObject currentTileGO;
    private Quaternion rotation = Quaternion.identity;

    void Start()
    {
        InitGround();
        LoadTiles(setName);

        outputMap = new GameObject[width][][];
        for (int x = 0; x < width; x++)
        {
            outputMap[x] = new GameObject[depth][];
            for (int y = 0; y < depth; y++)
            {
                outputMap[x][y] = new GameObject[length];
            }
        }
        GameObject.Find("CameraControl").transform.position = new Vector3((width - 1) * 0.5f, -0.5f, (length - 1) * 0.5f) * tileSize;
        
        highlightCube = Instantiate(highlightCube, Vector3.zero, Quaternion.identity);
        highlightCube.transform.localScale *= tileSize;

        highlightPlane = Instantiate(highlightPlane, Vector3.zero, Quaternion.identity);
        highlightPlane.transform.localScale *= tileSize;

        TileChange(0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
            TileChange(currentTileIndex + 1);

        if (Input.GetKeyDown(KeyCode.E))
            ChangeRotation();

        RaycastHit rHit = new RaycastHit();
        bool hit = RaycastMouse(ref rHit);

        TeleportHighlight(rHit);
        TeleportCurrentTileGO(rHit);

        if (Input.GetMouseButtonDown(0) && hit)
            CreateTile(rHit);

        if (Input.GetMouseButtonDown(1) && hit)
            DestroyTile(rHit);
    }

    private void InitGround()
    {
        for (int z = 0; z < length; z++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject go = Instantiate(colliderPrefab, new Vector3(x, -1f, z) * tileSize, Quaternion.identity);
                go.transform.localScale *= tileSize;
            }
        }
        GameObject plane = Instantiate(planePrefab, new Vector3((width - 1) * 0.5f, -0.5f, (length - 1) * 0.5f) * tileSize, Quaternion.identity);
        plane.transform.localScale = new Vector3(width, 1f, length) * tileSize * 0.1f;
    }

    private void LoadTiles(string setName)
    {
        string rulesPath = Application.dataPath + "\\Resources\\Tiles\\" + setName + "\\rules.xml";
        string tilePath = "Tiles\\" + setName + "\\";
        XmlTextReader reader = new XmlTextReader(rulesPath);
        List<Tile> tilesList = new List<Tile>();

        List<GameObject> goTiles = new List<GameObject>();

        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    if (reader.Name == "tile")
                    {
                        while (reader.MoveToNextAttribute())
                        {
                            if (reader.Name == "name")
                            {
                                string name = reader.Value;
                                goTiles.Add(Resources.Load<GameObject>("Tiles\\" + setName + "\\" + name));
                            }
                        }
                    }

                    break;
            }
        }

        tiles = goTiles.ToArray();
    }

    private bool RaycastMouse(ref RaycastHit rHit)
    {
        Vector3 point = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
        return Physics.Raycast(point, Camera.main.transform.forward, out rHit, Mathf.Infinity, LayerMask.GetMask("BuilderBox"));
    }

    private void TileChange(int index)
    {
        index = index % tiles.Length;
        
        if (currentTileIndex == index)
            return;

        currentTileIndex = index;
        Destroy(currentTileGO);
        currentTileGO = Instantiate(tiles[currentTileIndex], Vector3.zero, Quaternion.identity);

        foreach (Transform child in currentTileGO.transform)
        {
            child.GetComponent<Renderer>().material = transparentMat;
        }
    }

    private void ChangeRotation()
    {
        rotation *= Quaternion.Euler(Vector3.up * 90f);
        currentTileGO.transform.rotation = rotation;
    }

    private void TeleportHighlight(RaycastHit rHit)
    {
        if (rHit.transform == null)
        {
            highlightCube.transform.position = new Vector3(-999f, -999f, -999f);
            highlightPlane.transform.position = new Vector3(-999f, -999f, -999f);
            return;
        }
        Vector3 pos = rHit.transform.position;
        highlightCube.transform.position = pos;
        highlightPlane.transform.position = pos + rHit.normal * tileSize * 0.5f;
        highlightPlane.transform.LookAt(highlightPlane.transform.position + rHit.normal);
    }

    private void TeleportCurrentTileGO(RaycastHit rHit)
    {
        if (rHit.transform == null)
        {
            currentTileGO.transform.position = new Vector3(-999f, -999f, -999f);
            return;
        }

        Vector3 pos = rHit.transform.position;
        Vector3 normal = rHit.normal;

        Vector3 spawnPos = pos + normal * tileSize;
        Vector3 id = spawnPos / tileSize;

        if (id.x < 0 || id.y < 0 || id.z < 0 ||
            id.x >= width || id.y >= depth || id.z >= length)
            return;

        currentTileGO.transform.position = spawnPos;
    }

    private void CreateTile(RaycastHit rHit)
    {
        Vector3 pos = rHit.transform.position;
        Vector3 normal = rHit.normal;

        Vector3 spawnPos = pos + normal * tileSize;
        Vector3 id = spawnPos / tileSize;

        if (id.x < 0 || id.y < 0 || id.z < 0 ||
            id.x >= width || id.y >= depth || id.z >= length)
            return;

        GameObject tile = Instantiate(tiles[currentTileIndex], spawnPos, rotation);
        GameObject collider = Instantiate(colliderPrefab, spawnPos, Quaternion.identity);
        collider.transform.localScale *= tileSize;

        outputMap[(int)id.x][(int)id.y][(int)id.z] = tile;
    }

    private void DestroyTile(RaycastHit rHit)
    {
        Vector3 pos = rHit.transform.position;
        Vector3 id = pos / tileSize;

        Destroy(outputMap[(int)id.x][(int)id.y][(int)id.z]);
        Destroy(rHit.transform.gameObject);
        outputMap[(int)id.x][(int)id.y][(int)id.z] = null;
    }
}
