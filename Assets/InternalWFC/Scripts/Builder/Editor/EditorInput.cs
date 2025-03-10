﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EditorBuilder))]
public class EditorInput : Editor
{
    private EditorBuilder editorBuilder;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        editorBuilder = (EditorBuilder)target;

        if (GUILayout.Button("Init Builder"))
            editorBuilder.Init();
        if (GUILayout.Button("Load tileset"))
            editorBuilder.LoadTiles();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Overlapping Generation"))
            editorBuilder.GenerateOverlapping();
        if (GUILayout.Button("Tiled Generation (Autofill)"))
            editorBuilder.GenerateTiled();
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Advanced Options"))
            EditorBuilderWindow.Init(editorBuilder);


        if (editorBuilder.tiles == null || editorBuilder.tiles.Length == 0)
            return;

        GUIStyle style = new GUIStyle(GUI.skin.button);
        for (int i = 0; i < Mathf.CeilToInt(editorBuilder.tiles.Length / 3f); i++)
        {
            GUILayout.BeginHorizontal();
            for (int j = 0; j < 3; j++)
            {
                int index = i * 3 + j;
                if (index >= editorBuilder.tiles.Length)
                    break;
                
                Texture t = AssetPreview.GetAssetPreview(editorBuilder.tiles[index]._tileGameObject);
                GUIContent content = new GUIContent(editorBuilder.tiles[index]._tileGameObject.name, t);
                style.imagePosition = ImagePosition.ImageAbove;
                if (GUILayout.Button(content, style, GetButtonOptions()))
                {
                    editorBuilder.OnTilePrefabChange(index);
                }

            }
            GUILayout.EndHorizontal();
        }
        
    }

    protected virtual void OnSceneGUI()
    {
        if (editorBuilder == null)
            return;

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        var e = Event.current;
        if ((e.type == EventType.MouseDown || e.type == EventType.MouseMove) && e.type != EventType.MouseDrag)
        {
            Vector2 mousePos = e.mousePosition;
            var ray = HandleUtility.GUIPointToWorldRay(mousePos);
            RaycastHit rHit;
            bool hit = Physics.Raycast(ray, out rHit, Mathf.Infinity, LayerMask.GetMask("BuilderBox"));
            if (e.type == EventType.MouseMove)
                editorBuilder.HighlightPrefabsManagement(rHit);
            if (e.type == EventType.MouseDown && e.button == 0)
                editorBuilder.CreateTile(rHit);
            else if (e.type == EventType.MouseDown && e.button == 1)
                editorBuilder.DestroyTile(rHit);
        }
        else if (e.type == EventType.KeyUp && e.keyCode == KeyCode.A)
            editorBuilder.RotateTile();
    }

    private GUILayoutOption[] GetButtonOptions()
    {
        GUILayoutOption[] options = new GUILayoutOption[6];
        options[0] = GUILayout.Width(100f);
        options[1] = GUILayout.Height(100f);
        options[2] = GUILayout.MinWidth(50f);
        options[3] = GUILayout.MaxWidth(100f);
        options[4] = GUILayout.MinHeight(50f);
        options[5] = GUILayout.MaxHeight(100f);

        return options;
    }
}
