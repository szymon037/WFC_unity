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
        editorBuilder.tileSize = EditorGUILayout.IntField(tileSizeContent, editorBuilder.tileSize);

        GUIContent seamlessContent = new GUIContent("Seamless", "Generated levels will be seamless on edges");
        editorBuilder.seamless = EditorGUILayout.Toggle(seamlessContent, editorBuilder.seamless);

        GUIContent processTilesContent = new GUIContent("Process tiles", "Generated levels will be seamless on edges");
        editorBuilder.processTiles = EditorGUILayout.Toggle(processTilesContent, editorBuilder.processTiles);

        GUIContent offsetContent = new GUIContent("Offset", "Offset of the generated level from the point (0, 0, 0) of the scene");
        editorBuilder.offset = EditorGUILayout.Vector3Field(offsetContent, editorBuilder.offset);

        EditorGUILayout.Space();
        GUILayout.Label("Overlap Model Settings:", EditorStyles.boldLabel);

        GUIContent NContent = new GUIContent("N", "Width and length of generated overlap modules");
        editorBuilder.N = EditorGUILayout.IntField(NContent, editorBuilder.N);

        GUIContent N_depthContent = new GUIContent("N vertical", "Depth of generated overlap modules");
        editorBuilder.N_depth= EditorGUILayout.IntField(N_depthContent, editorBuilder.N_depth);

        GUIContent outputSizeContent = new GUIContent("Output size", "Size of the generated level");
        editorBuilder.outputSize = EditorGUILayout.Vector3IntField(outputSizeContent, editorBuilder.outputSize);

        GUIContent overlapTileCreationContent = new GUIContent("Overlap Creation", "Creation mode of ");
        editorBuilder.overlapTileCreation = EditorGUILayout.Toggle(overlapTileCreationContent, editorBuilder.overlapTileCreation);
    }
}
