using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Xml;
using System.Text;

class TiledModel : Model
{

    public TiledModel(int gridWidth, int gridHeight, int tileSize, bool seamless, bool processTiles, string setName) : base(gridWidth, gridHeight, tileSize, seamless)
    {
        this.tileSize = tileSize;
        ReadData(setName);
        if (processTiles)
            ProcessTiles();
        CreateAdjacencies();
        InitGrid();
        Init();
    }

    private void CreateAdjacencies()
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            for (int d = 0; d < 4; d++)
            {
                List<int> l = new List<int>();

                for (int j = 0; j < tiles.Length; j++)
                {
                    if ((tiles[i]._tileValues[d] & tiles[j]._tileValues[opposite[d]]) > 0)
                        l.Add(j);
                }
                tiles[i]._adjacencies[d] = l.ToArray();
            }
        }
    }

    private void ReadData(string setName)
    {
        string rulesPath = Application.dataPath + "\\Resources\\Tiles\\" + setName + "\\rules.xml";
        string tilePath = "Tiles\\" + setName + "\\";
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
                        byte[] tileValues = new byte[4];
                        while (reader.MoveToNextAttribute())
                        {
                            if (reader.Name == "name")
                            {
                                string name = reader.Value;
                                tile._tileGameObject = LoadTileGameObject("Tiles\\Knots\\" + name);
                            }

                            if (reader.Name == "frequency")
                                tile._weight = float.Parse(reader.Value);

                            if (reader.Name == "L")
                                tileValues[0] = CalculateBitValue(reader.Value);
                            if (reader.Name == "R")
                                tileValues[1] = CalculateBitValue(reader.Value);
                            if (reader.Name == "U")
                                tileValues[2] = CalculateBitValue(reader.Value);
                            if (reader.Name == "D")
                                tileValues[3] = CalculateBitValue(reader.Value);

                        }
                        tile._tileValues = tileValues;
                        tilesList.Add(tile);
                    }

                    break;
            }
        }

        tiles = tilesList.ToArray();

    }

    private byte CalculateBitValue(string value)
    {
        string[] neigbours = value.Split(';');
        int bitValue = 0;
        for (int i = 0; i < neigbours.Length; i++)
        {
            bitValue |= 1 << int.Parse(neigbours[i]);
        }
        return (byte)bitValue;
    }

    private GameObject LoadTileGameObject(string path)
    {
        return Resources.Load<GameObject>(path);
    }

    private void ProcessTiles()
    {
        List<Tile> newTiles = new List<Tile>();

        for (int i = 0; i < tiles.Length; i++)
        {
            Tile rotationTile = new Tile(tiles[i]);

            for (int j = 0; j < 3; j++)
            {
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

        tiles = tiles.Concat(newTiles).ToArray();
    }
    public override void GenerateOutput()
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                output[x][y] = Instantiate(grid[y * gridWidth + x]._tile._tileGameObject, new Vector3(x, 0f, /*gridHeight - */y) * tileSize, grid[y * gridWidth + x]._tile._tileGameObject.transform.rotation);
            }
        }

        /*for (int y = 0; y < gridHeight; y++)
        {
            StringBuilder sb = new StringBuilder();
            for (int x = 0; x < gridWidth; x++)
            {
                sb.Append((grid[y * gridWidth + x]._tileIndex + " "));
            }
            Debug.Log(sb.ToString());
        }*/

    }

    public override bool OnBorder(int x, int y)
    {
        return (x < 0 || y < 0 || x >= gridWidth || y >= gridHeight);
    }

    public Tile RotateTile(Tile tile)
    {
        if (tile == null || (tile._tileValues[0] == tile._tileValues[1] && tile._tileValues[0] == tile._tileValues[2] && tile._tileValues[0] == tile._tileValues[3]))
            return null;

        GameObject rotatedTileGameObject = Object.Instantiate(tile._tileGameObject, Vector3.zero, tile._tileGameObject.transform.rotation);
        rotatedTileGameObject.transform.Rotate(new Vector3(0f, 90f, 0f));
        //rotatedTileGameObject.name = "90";
        Destroy(rotatedTileGameObject);
        byte[] tileValues = new byte[4];
        tileValues[0] = tile._tileValues[3];
        tileValues[1] = tile._tileValues[2];
        tileValues[2] = tile._tileValues[0];
        tileValues[3] = tile._tileValues[1];
        return new Tile(tile._weight, rotatedTileGameObject, tileValues);
    }

    public Tile ReflectTile(Tile tile)
    {
        if (tile == null)
            return null;

        bool threeDifferentValues = (tile._tileValues[0] != tile._tileValues[1] && tile._tileValues[0] != tile._tileValues[2] && tile._tileValues[1] != tile._tileValues[2]) || (tile._tileValues[0] != tile._tileValues[3] && tile._tileValues[0] != tile._tileValues[2] && tile._tileValues[3] != tile._tileValues[2]);
        if (!((tile._tileValues[0] != tile._tileValues[1] && tile._tileValues[2] != tile._tileValues[3]) && threeDifferentValues))
            return null;

        GameObject reflectedTileGameObject = Object.Instantiate(tile._tileGameObject, Vector3.zero, tile._tileGameObject.transform.rotation);
        reflectedTileGameObject.transform.localScale = new Vector3(1f, 1f, -1f);

        Destroy(reflectedTileGameObject);
        byte[] tileValues = new byte[4];
        tileValues[0] = tile._tileValues[1];
        tileValues[1] = tile._tileValues[0];
        tileValues[2] = tile._tileValues[2];
        tileValues[3] = tile._tileValues[3];

        return new Tile(tile._weight, reflectedTileGameObject, tileValues);
    }
}
