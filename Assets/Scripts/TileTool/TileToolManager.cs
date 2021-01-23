using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Linq;
using TMPro;

[ExecuteInEditMode]
public class TileToolManager : MonoBehaviour
{
    public int tileSize;
    public string tilesetName;
    [HideInInspector] public Tile[] tiles;

    private GameObject highlightCube = null;
    private GameObject highlightPlane = null;
    private GameObject colliderPrefab = null;
    [HideInInspector] public GameObject currentTileGO = null;
    private Vector3 invisiblePos = new Vector3(-999f, 0f, 0f);
    private Vector3 spawnPos = Vector3.zero;
    public static TextMeshProUGUI[] textFields = new TextMeshProUGUI[6];

    private void Init()
    {
        DestroyImmediate(currentTileGO);

        while (transform.childCount > 0)
            DestroyImmediate(transform.GetChild(0).gameObject);

        highlightCube = Instantiate(Resources.Load<GameObject>("BuilderPrefabs\\HighlightCube"), spawnPos, Quaternion.identity, transform);
        highlightCube.transform.localScale *= (tileSize + 0.05f);

        highlightPlane = Instantiate(Resources.Load<GameObject>("BuilderPrefabs\\HighlightPlane"), invisiblePos, Quaternion.identity, transform);
        highlightPlane.transform.localScale *= tileSize;

        colliderPrefab = Instantiate(Resources.Load<GameObject>("BuilderPrefabs\\colliderPrefab"), spawnPos, Quaternion.identity, transform);
        colliderPrefab.transform.localScale *= tileSize;

        SetTextFields();
    }

    private void SetTextFields()
    {
        // L - 0, R - 1, U - 2, D - 3, F - 4, B - 5
        Transform canvas = GameObject.Find("Canvas").transform;
        foreach (Transform child in canvas)
        {
            if (child.gameObject.name == "L")
            {
                textFields[0] = child.GetComponent<TextMeshProUGUI>();
                textFields[0].transform.position = Vector3.left * tileSize * 0.51f;
                textFields[0].transform.LookAt(textFields[0].transform.position + Vector3.right);
            }
            if (child.gameObject.name == "R")
            {
                textFields[1] = child.GetComponent<TextMeshProUGUI>();
                textFields[1].transform.position = Vector3.right * tileSize * 0.51f;
                textFields[1].transform.LookAt(textFields[1].transform.position + Vector3.left);
            }
            if (child.gameObject.name == "U")
            {
                textFields[2] = child.GetComponent<TextMeshProUGUI>();
                textFields[2].transform.position = Vector3.up * tileSize * 0.51f;
                textFields[2].transform.LookAt(textFields[2].transform.position + Vector3.down);
            }
            if (child.gameObject.name == "D")
            {
                textFields[3] = child.GetComponent<TextMeshProUGUI>();
                textFields[3].transform.position = Vector3.down * tileSize * 0.51f;
                textFields[3].transform.LookAt(textFields[3].transform.position + Vector3.up);
            }
            if (child.gameObject.name == "F")
            {
                textFields[4] = child.GetComponent<TextMeshProUGUI>();
                textFields[4].transform.position = Vector3.forward * tileSize * 0.51f;
                textFields[4].transform.LookAt(textFields[4].transform.position + Vector3.back);
            }
            if (child.gameObject.name == "B")
            {
                textFields[5] = child.GetComponent<TextMeshProUGUI>();
                textFields[5].transform.position = Vector3.back * tileSize * 0.51f;
                textFields[5].transform.LookAt(textFields[5].transform.position + Vector3.forward);
            }
        }
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
    }

    public void OnTilePrefabChange(int index)
    {
        DestroyImmediate(currentTileGO);
        currentTileGO = Instantiate(tiles[index]._tileGameObject, spawnPos, Quaternion.identity);
    }
    public void LoadTiles()
    {
        TilesManager.LoadTilesTiled(tilesetName, false);
        tiles = new Tile[TilesManager.tilesTiled.Length];
        tileSize = TilesManager.tileSize;

        for (int i = 0; i < TilesManager.tilesTiled.Length; i++)
            tiles[i] = new Tile(TilesManager.tilesTiled[i]);
        Init();
    }

    public void CreateRulesXML()
    {
        List<XElement> tilesXML = new List<XElement>();
        for (int i = 0; i < tiles.Length; i++)
        {
            XElement xelement = tiles[i].ToXML();
            if (xelement != null)
                tilesXML.Add(tiles[i].ToXML());
        }

        XElement set =
            new XElement("set",
                new XElement("tiles", new XAttribute("tileSize", tileSize)));

        set.Element("tiles").Add(tilesXML);
        set.Save("Assets\\Resources\\Tiles\\" + tilesetName + "\\rules.xml");
    }
}
