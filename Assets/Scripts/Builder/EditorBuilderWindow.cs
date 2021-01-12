using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditorBuilderWindow : EditorWindow
{
    private static EditorBuilderWindow window;
    private static EditorBuilder editorBuilder;
    int test;

    [MenuItem("Window/Builder Advanced Settings")]
    public static void Init(EditorBuilder builder)
    {
        window = (EditorBuilderWindow)EditorWindow.GetWindow(typeof(EditorBuilderWindow), false, "Advanced Settings");
        window.Show();
        editorBuilder = builder;
    }

    void OnGUI()
    {
        GUILayout.Label("WFC settings:", EditorStyles.boldLabel);

        GUIContent tileSizeContent = new GUIContent("Tile size", "Size of tile in world space");
        EditorBuilder.tileSize = EditorGUILayout.IntField(tileSizeContent, EditorBuilder.tileSize);

        GUIContent seamlessContent = new GUIContent("Seamless", "Generated levels will be seamless on edges");
        EditorBuilder.seamless = EditorGUILayout.Toggle(seamlessContent, EditorBuilder.seamless);

        GUIContent processTilesContent = new GUIContent("Process tiles", "Generated levels will be seamless on edges");
        EditorBuilder.processTiles = EditorGUILayout.Toggle(processTilesContent, EditorBuilder.processTiles);

        GUIContent offsetContent = new GUIContent("Offset", "Offset of the generated level from the point (0, 0, 0) of the scene");
        EditorBuilder.offset = EditorGUILayout.Vector3Field(offsetContent, EditorBuilder.offset);

        EditorGUILayout.Space();
        GUILayout.Label("Overlap Model Settings:", EditorStyles.boldLabel);

        GUIContent NContent = new GUIContent("N", "Width and length of generated overlap modules");
        EditorBuilder.N = EditorGUILayout.IntField(NContent, EditorBuilder.N);

        GUIContent N_depthContent = new GUIContent("N vertical", "Depth of generated overlap modules");
        EditorBuilder.N_depth= EditorGUILayout.IntField(N_depthContent, EditorBuilder.N_depth);

        GUIContent outputSizeContent = new GUIContent("Output size", "Size of the generated level");
        EditorBuilder.outputSize = EditorGUILayout.Vector3IntField(outputSizeContent, EditorBuilder.outputSize);

        GUIContent overlapTileCreationContent = new GUIContent("Overlap Creation", "Creation mode of ");
        EditorBuilder.overlapTileCreation = EditorGUILayout.Toggle(overlapTileCreationContent, EditorBuilder.overlapTileCreation);

        // overlap in tile creation

        /*EditorGUILayout.Space();
        GUILayout.Label("Editor Settings:", EditorStyles.boldLabel);
        color?
        */


    }
}
