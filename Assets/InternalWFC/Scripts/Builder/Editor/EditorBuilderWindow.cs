using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditorBuilderWindow : EditorWindow
{
    private static EditorBuilderWindow window;
    private static EditorBuilder editorBuilder;
    int test;

    [MenuItem("Window/Builder Advanced Options")]
    public static void Init(EditorBuilder builder)
    {
        window = (EditorBuilderWindow)EditorWindow.GetWindow(typeof(EditorBuilderWindow), false, "Advanced Options");
        window.Show();
        editorBuilder = builder;
    }

    void OnGUI()
    {
        if (editorBuilder == null)
            return;

        GUILayout.Label("WFC settings:", EditorStyles.boldLabel);
        GUIContent processTilesContent = new GUIContent("Process tiles", "Generated levels will be seamless on edges");
        editorBuilder.processTiles = EditorGUILayout.Toggle(processTilesContent, editorBuilder.processTiles);

        EditorGUILayout.Space();
        GUILayout.Label("Tiled Model Settings:", EditorStyles.boldLabel);

        GUIContent tileSizeContent = new GUIContent("Tile size", "Size of tile in world space");
        editorBuilder.tileSize = EditorGUILayout.IntField(tileSizeContent, editorBuilder.tileSize);

        GUIContent seamlessContent = new GUIContent("Seamless", "Generated levels will be seamless on edges");
        editorBuilder.seamless = EditorGUILayout.Toggle(seamlessContent, editorBuilder.seamless);

        EditorGUILayout.Space();
        GUILayout.Label("Overlapping Model Settings:", EditorStyles.boldLabel);

        GUIContent offsetContent = new GUIContent("Level Position Offset", "Offset of the generated level from the point (0, 0, 0) of the scene");
        editorBuilder.offset = EditorGUILayout.Vector3Field(offsetContent, editorBuilder.offset);

        GUIContent NContent = new GUIContent("N", "Width and length of generated overlap modules");
        editorBuilder.N = EditorGUILayout.IntField(NContent, editorBuilder.N);

        GUIContent N_depthContent = new GUIContent("N vertical", "Depth of generated overlap modules");
        editorBuilder.N_depth= EditorGUILayout.IntField(N_depthContent, editorBuilder.N_depth);

        GUIContent outputSizeContent = new GUIContent("Generated level size", "Size of the generated level");
        editorBuilder.outputSize = EditorGUILayout.Vector3IntField(outputSizeContent, editorBuilder.outputSize);

        GUIContent overlapTileCreationContent = new GUIContent("Overlap Tile Creation", "Creation mode of tiles for overlapping model");
        editorBuilder.overlapTileCreation = EditorGUILayout.Toggle(overlapTileCreationContent, editorBuilder.overlapTileCreation);

        EditorGUILayout.Space();
        GUILayout.Label("Data save:", EditorStyles.boldLabel);
        GUIContent saveDataContent = new GUIContent("Name of save file", "Name of the save file that will be loaded/saved");
        editorBuilder.saveFileName = EditorGUILayout.TextField(saveDataContent, editorBuilder.saveFileName);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save File"))
            editorBuilder.SaveDataFile();

        if (GUILayout.Button("Load File"))
            editorBuilder.LoadDataFile();
        GUILayout.EndHorizontal();
    }
}
