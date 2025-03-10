﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Xml;
using System.Text;
using System.IO;
using System;

public static class TilesManager 
{
    public static Tile[] tilesTiled = null;
    public static Tile[] tilesOverlap = null;
    public static int tileSize = 0;
    public static bool LoadTilesTiled(string setName, bool processTiles, bool loadGameObjects)
    {
        if (setName == null || setName == "")
        {
            Debug.Log("Tilest name not specified!");
            return false;
        }

        if (!Directory.Exists(Application.dataPath + "\\Resources\\Tiles\\" + setName))
        {
            Debug.LogWarning("There is no such tile set: " + setName + " in Resources folder");
            return false;
        }

        if (ReadData(setName))
        {
            if (loadGameObjects)
                LoadGameObjectTiles(setName);

            if (processTiles)
                ProcessTiles();
            CreateAdjacencies();
        }
        else
            LoadGameObjectTiles(setName);

        return true;
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

                    if (reader.Name == "tiles")
                    {
                        reader.MoveToNextAttribute();
                        if (reader.Name == "tileSize")
                            tileSize = int.Parse(reader.Value);
                    }

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
                        }
                        tile.CalculateBitValue();
                        tilesList.Add(tile);
                    }

                    break;
            }
        }

        tilesTiled = tilesList.ToArray();
        reader.Close();
        return true;
    }

    private static int[] ParseStringAdjacencies(string value)
    {
        string[] neighbours = value.Split(';');
        int[] adjacencies = new int[neighbours.Length];

        for (int i = 0; i < neighbours.Length; i++)
        {
            try
            {
                adjacencies[i] = int.Parse(neighbours[i]);
            }
            catch (FormatException)
            {
                adjacencies = new int[0];
                break;
            }
        }

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
                // CHECK IF TILE IS "I" TYPE - ROTATE ONLY ONCE
                rotationTile = RotateTile(rotationTile);
                Tile reflectionTile = null;
                if (rotationTile != null)
                    reflectionTile = ReflectTile(rotationTile);
                if (rotationTile != null && !newTiles.Exists(t => t.GetName() == rotationTile.GetName()) && tilesTiled[i].GetName() != rotationTile.GetName())
                    newTiles.Add(rotationTile);
                if (reflectionTile != null && !newTiles.Exists(t => t.GetName() == reflectionTile.GetName()) && tilesTiled[i].GetName() != reflectionTile.GetName())
                    newTiles.Add(reflectionTile);
            }
        }

        tilesTiled = tilesTiled.Concat(newTiles).ToArray();
    }

    public static Tile RotateTile(Tile tile)
    {
        if (tile == null || (tile._tileValues[0] == tile._tileValues[1] && tile._tileValues[0] == tile._tileValues[4] && tile._tileValues[0] == tile._tileValues[5]))
            return null;

        Quaternion rotation = tile._rotation;
        float rotationY = 0f;

        if (tile._tileValues[0] == tile._tileValues[1] && tile._tileValues[4] == tile._tileValues[5] && tile._tileValues[0] != tile._tileValues[4]) // if type I
        {
            float modulo = Mathf.Round(rotation.eulerAngles.y % 180f);
            rotationY = (modulo > 0f) ? 0f : 90f;
        }
        else
            rotationY = rotation.eulerAngles.y + 90f;

        int[] tileValues = new int[6];
        tileValues[0] = tile._tileValues[5];
        tileValues[1] = tile._tileValues[4];
        tileValues[2] = tile._tileValues[2];
        tileValues[3] = tile._tileValues[3];
        tileValues[4] = tile._tileValues[0];
        tileValues[5] = tile._tileValues[1];
        return new Tile(tile._weight, tile._tileGameObject, tileValues, tile._tileName, rotationY, 1f, tile._ground, tile._ceiling);
    }

    public static Tile ReflectTile(Tile tile, bool skipValueCheck = false)
    {
        if (tile == null)
            return null;

        if (tile._tileValues[0] == tile._tileValues[1] && tile._tileValues[4] == tile._tileValues[5] && tile._tileValues[0] != tile._tileValues[4]) // if type I
            return null;

        Quaternion rotation = tile._rotation;
        float rotationY = Mathf.Round(rotation.eulerAngles.y);

        bool threeDifferentValues = (tile._tileValues[0] != tile._tileValues[1] && tile._tileValues[0] != tile._tileValues[4] && tile._tileValues[1] != tile._tileValues[4]) || (tile._tileValues[0] != tile._tileValues[5] && tile._tileValues[0] != tile._tileValues[4] && tile._tileValues[5] != tile._tileValues[4]);
        if (!skipValueCheck && !((tile._tileValues[0] != tile._tileValues[1] && tile._tileValues[4] != tile._tileValues[5]) && threeDifferentValues))
            return null;


        else if (skipValueCheck)
        {
            float modulo = Mathf.Round(rotation.eulerAngles.y % 180f);
            if (modulo == 90f)
                return RotateTile(RotateTile(RotateTile(tile)));
            return RotateTile(tile);
        }

        int[] tileValues = new int[6];
        tileValues[0] = tile._tileValues[1];
        tileValues[1] = tile._tileValues[0];
        tileValues[2] = tile._tileValues[2];
        tileValues[3] = tile._tileValues[3];
        tileValues[4] = tile._tileValues[4];
        tileValues[5] = tile._tileValues[5];

        return new Tile(tile._weight, tile._tileGameObject, tileValues, tile._tileName, rotationY, -1f, tile._ground, tile._ceiling);
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
        GameObject[] gameObjects;
        gameObjects = Resources.LoadAll<GameObject>("Tiles\\" + setName);
        List<Tile> tiles = new List<Tile>();
        if (tilesTiled != null)
            tiles = new List<Tile>(tilesTiled);

        for (int i = 0; i < gameObjects.Length; i++)
            if (!tiles.Exists(t => t._tileGameObject.name == gameObjects[i].name))
                tiles.Add(new Tile(gameObjects[i]));

        tilesTiled = tiles.ToArray();
    }
}
