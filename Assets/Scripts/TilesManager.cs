﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Xml;
using System.Text;
using System.IO;

public static class TilesManager 
{
    public static Tile[] tilesTiled = null;
    public static Tile[] tilesOverlap = null;

    public static void LoadTilesTiled(string setName, bool processTiles)
    {
        if (ReadData(setName))
        {
            if (processTiles)
                ProcessTiles();
            CreateAdjacencies();
        }
        else
            LoadGameObjectTiles(setName);


    }

    private static bool ReadData(string setName)
    {
        tilesTiled = null;
        string rulesPath = Application.dataPath + "\\Resources\\Tiles\\" + setName + "\\rules.xml";
        string tilePath = "Tiles\\" + setName + "\\";

        if (!File.Exists(rulesPath))
        {
            Debug.LogWarning("File with rules does not exist for tileset: " + setName + ". Loading only Prefabs.");
            return false;
        }

        XmlTextReader reader = new XmlTextReader(rulesPath);
        List<Tile> tilesList = new List<Tile>();

        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:

                    if (reader.Name == "tile")
                    {
                        Tile tile = new Tile();
                        byte[] tileValues = new byte[6];
                        while (reader.MoveToNextAttribute())
                        {
                            if (reader.Name == "name")
                            {
                                string name = reader.Value;
                                tile._tileGameObject = LoadTileGameObject("Tiles\\" + setName + "\\" + name);
                                tile._tileName = tile._tileGameObject.name;
                            }

                            if (reader.Name == "frequency")
                                tile._weight = float.Parse(reader.Value);

                            if (reader.Name == "L")
                                tile._edgeAdjacencies[0] = ParseStringAdjacencies(reader.Value);
                            if (reader.Name == "R")
                                tile._edgeAdjacencies[1] = ParseStringAdjacencies(reader.Value);
                            if (reader.Name == "U")
                                tile._edgeAdjacencies[2] = ParseStringAdjacencies(reader.Value);
                            if (reader.Name == "D")
                                tile._edgeAdjacencies[3] = ParseStringAdjacencies(reader.Value);
                            if (reader.Name == "F")
                                tile._edgeAdjacencies[4] = ParseStringAdjacencies(reader.Value);
                            if (reader.Name == "B")
                                tile._edgeAdjacencies[5] = ParseStringAdjacencies(reader.Value);

                            

                            /*for (int i = 0; i < 6; i++)
                            {
                                if (tile._adjacencies[i] == null)
                                {
                                    continue;
                                }
                                for (int j = 0; j < tile._adjacencies[i].Length; j++)
                                {
                                    Debug.Log(tile._adjacencies[i][j]);
                                }
                            }*/

                        }
                        tile.CalculateBitValue();
                        tilesList.Add(tile);
                    }

                    break;
            }
        }

        tilesTiled = tilesList.ToArray();

        return true;
    }

    private static int[] ParseStringAdjacencies(string value)
    {
        string[] neighbours = value.Split(';');
        int[] adjacencies = new int[neighbours.Length];

        for (int i = 0; i < neighbours.Length; i++)
            adjacencies[i] = int.Parse(neighbours[i]);

        return adjacencies;
    }

    private static GameObject LoadTileGameObject(string path)
    {
        return Resources.Load<GameObject>(path);
    }

    private static void ProcessTiles()
    {
        List<Tile> newTiles = new List<Tile>();

        for (int i = 0; i < tilesTiled.Length; i++)
        {
            Tile rotationTile = new Tile(tilesTiled[i]);

            for (int j = 0; j < 3; j++)
            {
                /// TODO: IT DOES NOT WORK? "I" TYPE IS ROTATED TWICE
                // CHECK IF TILE IS "I" TYPE - ROTATE ONLY ONCE
                rotationTile = RotateTile(rotationTile);
                Tile reflectionTile = null;
                if (rotationTile != null)
                    reflectionTile = ReflectTile(rotationTile);

                if (rotationTile != null)
                    newTiles.Add(rotationTile);
                if (reflectionTile != null)
                    newTiles.Add(reflectionTile);
            }
        }

        tilesTiled = tilesTiled.Concat(newTiles).ToArray();
    }

    /// TODO: make rotation possible for tiles without init tileValues (tiles without rules.xml)
    public static Tile RotateTile(Tile tile)
    {
        if (tile == null || (tile._tileValues[0] == tile._tileValues[1] && tile._tileValues[0] == tile._tileValues[4] && tile._tileValues[0] == tile._tileValues[5]))
            return null;

        Quaternion rotation = tile._transform.rotation;
        float rotationY = rotation.eulerAngles.y;

        byte[] tileValues = new byte[6];
        tileValues[0] = tile._tileValues[5];
        tileValues[1] = tile._tileValues[4];
        tileValues[2] = tile._tileValues[2];
        tileValues[3] = tile._tileValues[3];
        tileValues[4] = tile._tileValues[0];
        tileValues[5] = tile._tileValues[1];
        return new Tile(tile._weight, tile._tileGameObject, tileValues, tile._tileName, rotationY + 90f, 1f);
    }

    public static Tile ReflectTile(Tile tile)
    {
        if (tile == null)
            return null;

        bool threeDifferentValues = (tile._tileValues[0] != tile._tileValues[1] && tile._tileValues[0] != tile._tileValues[4] && tile._tileValues[1] != tile._tileValues[4]) || (tile._tileValues[0] != tile._tileValues[5] && tile._tileValues[0] != tile._tileValues[4] && tile._tileValues[5] != tile._tileValues[4]);
        if (!((tile._tileValues[0] != tile._tileValues[1] && tile._tileValues[4] != tile._tileValues[5]) && threeDifferentValues))
            return null;

        //GameObject reflectedTileGameObject = Object.Instantiate(tile._tileGameObject, Vector3.zero, tile._tileGameObject.transform.rotation);
        //reflectedTileGameObject.transform.localScale = new Vector3(1f, 1f, -1f);
        //Object.Destroy(reflectedTileGameObject);

        Quaternion rotation = tile._transform.rotation;
        float rotationY = rotation.eulerAngles.y;

        byte[] tileValues = new byte[6];
        tileValues[0] = tile._tileValues[1];
        tileValues[1] = tile._tileValues[0];
        tileValues[2] = tile._tileValues[2];
        tileValues[3] = tile._tileValues[3];
        tileValues[4] = tile._tileValues[4];
        tileValues[5] = tile._tileValues[5];

        return new Tile(tile._weight, tile._tileGameObject, tileValues, tile._tileName, rotationY, -1f);
    }
    private static void CreateAdjacencies()
    {
        for (int i = 0; i < tilesTiled.Length; i++)
        {
            for (int d = 0; d < 6; d++)
            {
                List<int> l = new List<int>();

                for (int j = 0; j < tilesTiled.Length; j++)
                {
                    if ((tilesTiled[i]._tileValues[d] & tilesTiled[j]._tileValues[Model.opposite[d]]) > 0)
                        l.Add(j);
                }
                tilesTiled[i]._tileAdjacencies[d] = l.ToArray();
            }
        }
    }

    public static void LoadGameObjectTiles(string setName)
    {
        /// TODO: check if tileset exists (simple null check is not enough)
        if (setName == null || setName == "")
            Debug.Log("Tilest name not specified!");
        GameObject[] gameObjects;
        gameObjects = Resources.LoadAll<GameObject>("Tiles\\" + setName);
        tilesTiled = new Tile[gameObjects.Length];

        for (int i = 0; i < gameObjects.Length; i++)
            tilesTiled[i] = new Tile(gameObjects[i]);
    }
}
