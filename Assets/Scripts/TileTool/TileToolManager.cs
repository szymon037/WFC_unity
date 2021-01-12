using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Linq;

public class TileToolManager : MonoBehaviour
{
    public int tileSize;
    public string tilesetName;
    [HideInInspector] public Tile[] tiles;

    private GameObject highlightCube = null;
    private GameObject highlightPlane = null;
    private GameObject colliderPrefab = null;
    private GameObject currentTileGO = null;
    private Vector3 invisiblePos = new Vector3(-999f, 0f, 0f);
    private Vector3 spawnPos = Vector3.zero;

    

    
    private void Init()
    {
        DestroyImmediate(currentTileGO);

        DestroyImmediate(highlightCube);
        highlightCube = Instantiate(Resources.Load<GameObject>("BuilderPrefabs\\HighlightCube"), spawnPos, Quaternion.identity, transform);
        highlightCube.transform.localScale *= (tileSize + 0.05f);

        DestroyImmediate(highlightPlane);
        highlightPlane = Instantiate(Resources.Load<GameObject>("BuilderPrefabs\\HighlightPlane"), invisiblePos, Quaternion.identity, transform);
        highlightPlane.transform.localScale *= tileSize;

        DestroyImmediate(colliderPrefab);
        colliderPrefab = Instantiate(Resources.Load<GameObject>("BuilderPrefabs\\colliderPrefab"), spawnPos, Quaternion.identity, transform);
        colliderPrefab.transform.localScale *= tileSize;
    }

    public void ChooseFace(RaycastHit rHit)
    {
        if (highlightCube == null || highlightPlane == null)
            return;
        if (rHit.transform == null)
        {
            highlightPlane.transform.position = invisiblePos;
            return;
        }

        Vector3 cubePos = rHit.transform.position;
        highlightPlane.transform.position = cubePos + rHit.normal * tileSize * 0.51f;
        highlightPlane.transform.LookAt(highlightPlane.transform.position + rHit.normal);

        // OnFaceChange
    }

    public void OnTilePrefabChange(int index)
    {
        DestroyImmediate(currentTileGO);
        currentTileGO = Instantiate(tiles[index]._tileGameObject, spawnPos, Quaternion.identity);
    }
    public void LoadTiles()
    {
        Init();
        TilesManager.LoadTilesTiled(tilesetName, false);
        tiles = new Tile[TilesManager.tilesTiled.Length];

        for (int i = 0; i < TilesManager.tilesTiled.Length; i++)
            tiles[i] = new Tile(TilesManager.tilesTiled[i]);
    }

    public void CreateRulesXML()
    {
        XElement[] tilesXML = new XElement[6];
        for (int i = 0; i < this.tiles.Length; i++)
            tilesXML[i] = this.tiles[i].ToXML();

        XElement set =
            new XElement("set",
                new XElement("tiles", new XAttribute("tileSize", tileSize),
                    tilesXML[0], tilesXML[1], tilesXML[2], tilesXML[3], tilesXML[4], tilesXML[5]));

        set.Save("Assets\\Resources\\Tiles\\" + tilesetName + "\\rules2.xml");
    }
}
