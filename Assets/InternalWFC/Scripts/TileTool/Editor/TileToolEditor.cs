using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TileToolManager))]

public class TileToolEditor : Editor
{
    private TileToolManager tileToolManager;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        tileToolManager = (TileToolManager)target;

        if (GUILayout.Button("Tile Settings"))
            TileToolWindow.Init(tileToolManager);

        if (GUILayout.Button("Load Tileset"))
        {
            tileToolManager.LoadTiles();
            TileToolWindow.Init(tileToolManager);
        }

        if (GUILayout.Button("Create Rules"))
            tileToolManager.CreateRulesXML();

        if (tileToolManager.tiles == null || tileToolManager.tiles.Length == 0)
            return;

        GUIStyle style = new GUIStyle(GUI.skin.button);
        for (int i = 0; i < Mathf.CeilToInt(tileToolManager.tiles.Length / 3f); i++)
        {
            GUILayout.BeginHorizontal();
            for (int j = 0; j < 3; j++)
            {
                int index = i * 3 + j;
                if (index >= tileToolManager.tiles.Length)
                    break;

                Texture t = AssetPreview.GetAssetPreview(tileToolManager.tiles[index]._tileGameObject);
                GUIContent content = new GUIContent(tileToolManager.tiles[index]._tileGameObject.name, t);
                style.imagePosition = ImagePosition.ImageAbove;
                if (GUILayout.Button(content, style, GetButtonOptions()))
                {
                    tileToolManager.OnTilePrefabChange(index);
                    TileToolWindow.OnTilePrefabChange(index);
                }

            }
            GUILayout.EndHorizontal();
        }
    }

    protected virtual void OnSceneGUI()
    {
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        var e = Event.current;
        if (e.type == EventType.MouseDown && e.type != EventType.MouseDrag)
        {
            Vector2 mousePos = e.mousePosition;
            var ray = HandleUtility.GUIPointToWorldRay(mousePos);
            RaycastHit rHit;
            bool hit = Physics.Raycast(ray, out rHit, Mathf.Infinity, LayerMask.GetMask("BuilderBox"));
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                tileToolManager.ChooseFace(rHit);
                TileToolWindow.OnFaceChange(rHit);
                SceneView.RepaintAll();
            }
        }
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
