///TODO: przenieść ten skrypt do folderu Editor
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TileToolWindow : EditorWindow
{
    private static TileToolWindow window;
    private static TileToolManager tileToolManager;
    private static int tileIndex = -1;
    private static string faceName = "None";
    private static string faceIndices = "None";
    private static int dirIndex;
    // L - 0, R - 1, U - 2, D - 3, F - 4, B - 5
    private static Dictionary<Vector3, int> directionsToIndexDictionary = new Dictionary<Vector3, int>{
        { Vector3.left, 0 }, { Vector3.right, 1 }, { Vector3.up, 2 }, { Vector3.down, 3}, { Vector3.forward, 4 }, { Vector3.back, 5 } };
    private static Dictionary<Vector3, string> directionToNameDictionary = new Dictionary<Vector3, string>{
        { Vector3.left, "Left" }, { Vector3.right, "Right" }, { Vector3.up, "Up" }, { Vector3.down, "Down"}, { Vector3.forward, "Forward" }, { Vector3.back, "Back" } };

    private static List<int>[] edgeAdjacencies = new List<int>[6];

    private static float weight;
    int maxIndicesNr = 12;
    int indicesInRow = 4;
    int indexSelection;


    [MenuItem("Window/Tile Tool Window")]
    public static void Init(TileToolManager tileTool)
    {
        tileToolManager = tileTool;
        // Get existing open window or if none, make a new one:
        window = (TileToolWindow)EditorWindow.GetWindow(typeof(TileToolWindow));
        window.Show();
    }

    void OnGUI()
    {
        string tileName = (tileToolManager != null && tileIndex > 0 && tileIndex < tileToolManager.tiles.Length) ? tileToolManager.tiles[tileIndex]._tileGameObject.name : "None";
        EditorGUILayout.LabelField("Prefab name: ", tileName);
        EditorGUILayout.LabelField("Face selected: ", faceName);
        EditorGUILayout.LabelField("Current face indices: ", faceIndices);
        weight = Mathf.Max(0f, EditorGUILayout.FloatField("Weight", weight));
        indexSelection = GUILayout.Toolbar(indexSelection, new string[] { "Add", "Remove" });


        for (int y = 0; y < Mathf.Ceil((float)maxIndicesNr / indicesInRow); y++)
        {
            GUILayout.BeginHorizontal();
            for (int x = 0; x < indicesInRow; x++)
            {
                int buttonIndex = y * indicesInRow + x;
                if (buttonIndex >= maxIndicesNr)
                    break;

                if (GUILayout.Button(buttonIndex.ToString()))
                {
                    if (indexSelection == 0 && !edgeAdjacencies[dirIndex].Exists(i => i == buttonIndex))
                    {
                        edgeAdjacencies[dirIndex].Add(buttonIndex);
                        UpdateFaceIndices();
                    }
                    else if (indexSelection == 1)
                    {
                        edgeAdjacencies[dirIndex].Remove(buttonIndex);
                        UpdateFaceIndices();
                    }

                }

            }
            GUILayout.EndHorizontal();
        }

        if (GUILayout.Button("++"))
        {
            maxIndicesNr++;
            Debug.Log(maxIndicesNr + " " + Mathf.Ceil(maxIndicesNr / indicesInRow));
        }
        if (GUILayout.Button("--"))
            maxIndicesNr--;

        if (GUILayout.Button("Save tile changes"))
            SaveTileChanges();




    }

    public static void OnTilePrefabChange(int index)
    {
        tileIndex = index;
        weight = tileToolManager.tiles[tileIndex]._weight;
        for (int i = 0; i < 6; i++)
            edgeAdjacencies[i] = new List<int>(tileToolManager.tiles[tileIndex]._edgeAdjacencies[i]);
        dirIndex = -1;
        UpdateFaceIndices();
        window.Repaint();
    }

    public static void OnFaceChange(RaycastHit rHit)
    {
        if (rHit.transform == null)
            return;
        faceName = directionToNameDictionary[rHit.normal];

        dirIndex = directionsToIndexDictionary[rHit.normal];
        UpdateFaceIndices();

    }

    private static void UpdateFaceIndices()
    {
        faceIndices = "";

        for (int i = 0; dirIndex > 0 && i < edgeAdjacencies[dirIndex].Count; i++)
            faceIndices += edgeAdjacencies[dirIndex][i].ToString() + " ";

        window.Repaint();
    }

    public static void SaveTileChanges()
    {
        tileToolManager.tiles[tileIndex]._weight = weight;
        for (int i = 0; i < 6; i++)
            tileToolManager.tiles[tileIndex]._edgeAdjacencies[i] = edgeAdjacencies[i].ToArray();
    }

    

}
