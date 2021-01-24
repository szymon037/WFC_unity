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
    private static int faceIndex;

    // L - 0, R - 1, U - 2, D - 3, F - 4, B - 5
    private static Dictionary<Vector3, int> directionsToIndexDictionary = new Dictionary<Vector3, int>{
        { Vector3.left, 0 }, { Vector3.right, 1 }, { Vector3.up, 2 }, { Vector3.down, 3}, { Vector3.forward, 4 }, { Vector3.back, 5 } };
    private static Dictionary<Vector3, string> directionToNameDictionary = new Dictionary<Vector3, string>{
        { Vector3.left, "Left" }, { Vector3.right, "Right" }, { Vector3.up, "Up" }, { Vector3.down, "Down"}, { Vector3.forward, "Forward" }, { Vector3.back, "Back" } };

    private static List<int>[] edgeAdjacencies = new List<int>[6];

    private static float weight;
    int maxIndicesNr = 12;
    int indicesInRow = 4;

    [MenuItem("Window/Tile Tool Window")]
    public static void Init(TileToolManager tileTool)
    {
        tileToolManager = tileTool;
        window = (TileToolWindow)EditorWindow.GetWindow(typeof(TileToolWindow));
        window.Show();
    }

    void OnGUI()
    {
        string tileName = (tileToolManager != null && tileToolManager.tiles != null && tileIndex > -1 && tileIndex < tileToolManager.tiles.Length) ? tileToolManager.tiles[tileIndex]._tileGameObject.name : "None";

        GUIContent prefabNameContent1 = new GUIContent("Prefab name: ", "Name of the loaded prefab");
        GUIContent prefabNameContent2 = new GUIContent(tileName);
        EditorGUILayout.LabelField(prefabNameContent1, prefabNameContent2);

        GUIContent selectedFaceContent1 = new GUIContent("Selected face: ", "Currently selected face - chosen indices will be assigned to this face");
        GUIContent selectedFaceContent2 = new GUIContent(faceName);
        EditorGUILayout.LabelField(selectedFaceContent1, selectedFaceContent2);

        GUIContent currentFaceIndicesContent1 = new GUIContent("Current face indices: ", "Indices assigned to selected face; selected face will be adjacent to other faces with at least one same index");
        GUIContent currentFaceIndicesContent2 = new GUIContent(faceIndices);
        EditorGUILayout.LabelField(currentFaceIndicesContent1, currentFaceIndicesContent2);

        GUIContent weightContent = new GUIContent("Frequency", "Frequency/weight of current tile: it decides how often will this tile appear");
        weight = Mathf.Max(0f, EditorGUILayout.FloatField(weightContent, weight));

        //indexSelection = GUILayout.Toolbar(indexSelection, new string[] { "Add", "Remove" });

        for (int y = 0; y < Mathf.Ceil((float)maxIndicesNr / indicesInRow); y++)
        {
            GUILayout.BeginHorizontal();
            for (int x = 0; x < indicesInRow; x++)
            {
                int buttonIndex = y * indicesInRow + x;
                if (buttonIndex >= maxIndicesNr)
                    break;

                if (GUILayout.Button(buttonIndex.ToString(), GetButtonOptions()))
                {
                    if (!edgeAdjacencies[faceIndex].Exists(i => i == buttonIndex))
                    {
                        edgeAdjacencies[faceIndex].Add(buttonIndex);
                        UpdateFaceIndices();
                    }
                    else
                    {
                        edgeAdjacencies[faceIndex].Remove(buttonIndex);
                        UpdateFaceIndices();
                    }

                }

            }
            GUILayout.EndHorizontal();
        }

        if (GUILayout.Button("++"))
            maxIndicesNr++;

        if (GUILayout.Button("--"))
            maxIndicesNr--;
    }

    public static void OnTilePrefabChange(int index)
    {
        SaveTileChanges();

        tileIndex = index;
        weight = tileToolManager.tiles[tileIndex]._weight;
        for (int i = 0; i < 6; i++)
        {
            edgeAdjacencies[i] = new List<int>(tileToolManager.tiles[tileIndex]._edgeAdjacencies[i]);
        }

        for (int i = 0; i < 6; i++)
        {
            if (TileToolManager.textFields[i] == null)
                TileToolManager.textFields[i].text = string.Empty;
            else
                TileToolManager.textFields[i].text = GetFaceIndices(i);

        }

        faceIndex = 0;
        UpdateFaceIndices();
        window.Repaint();
    }

    public static void OnFaceChange(RaycastHit rHit)
    {
        if (rHit.transform == null)
            return;
        faceName = directionToNameDictionary[rHit.normal];

        faceIndex = directionsToIndexDictionary[rHit.normal];
        UpdateFaceIndices();
    }

    private static void UpdateFaceIndices()
    {
        faceIndices = GetFaceIndices(faceIndex);
        TileToolManager.textFields[faceIndex].text = faceIndices;
        EditorApplication.QueuePlayerLoopUpdate();
        window.Repaint();
    }

    public static void SaveTileChanges()
    {
        if (tileIndex < 0f)
            return;

        tileToolManager.tiles[tileIndex]._weight = weight;
        for (int i = 0; i < 6; i++)
            tileToolManager.tiles[tileIndex]._edgeAdjacencies[i] = edgeAdjacencies[i].ToArray();
    }

    private GUILayoutOption[] GetButtonOptions()
    {
        GUILayoutOption[] options = new GUILayoutOption[6];
        options[0] = GUILayout.Width(25f);
        options[1] = GUILayout.Height(25f);
        options[2] = GUILayout.MinWidth(5f);
        options[3] = GUILayout.MaxWidth(100f);
        options[4] = GUILayout.MinHeight(5f);
        options[5] = GUILayout.MaxHeight(25f);

        return options;
    }

    private static string GetFaceIndices(int index)
    {
        if (edgeAdjacencies[index] == null)
            return string.Empty;

        string indices = string.Empty;

        for (int i = 0; i < edgeAdjacencies[index].Count; i++)
            indices += edgeAdjacencies[index][i].ToString() + " ";

        return indices;
    }
}
