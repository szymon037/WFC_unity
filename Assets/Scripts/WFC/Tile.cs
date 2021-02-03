using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class Tile
{
    // L - 0, R - 1, U - 2, D - 3, F - 4, B - 5
    public float _weight;
    public int[][] _tileAdjacencies = new int[6][];  // tile neighbours of this tile
    public int[][] _edgeAdjacencies = new int[6][];  // edge indices used for adjacency check
    public byte[] _tileValues = new byte[6];
    public long _index;
    public GameObject _tileGameObject = null;
    public Quaternion _rotation = Quaternion.identity;
    public Vector3 _scale = Vector3.one;
    public string _tileName;  // original name, not changed with rotation or (Clone)
    public bool _ground;
    public bool _ceiling;
    public Tile() { }

    public Tile(byte[] values, float rotation, float scale/*, bool ground = false*/)
    {
        _tileValues = values;
        //_ground = ground;
        _ceiling = true;
        scale = Mathf.Sign(scale);
        _rotation *= Quaternion.Euler(Vector3.up * rotation);

        float x = 1f, z = 1f;
        rotation = Mathf.Round(rotation);
        if (rotation == 90f || rotation == 270f)
            x = scale;
        else
            z = scale;

        _scale = new Vector3(x, 1f, z);
    }

    // tiled
    public Tile(Tile tile)
    {
        _weight = tile._weight;
        _tileAdjacencies = tile._tileAdjacencies;
        _edgeAdjacencies = tile._edgeAdjacencies;
        _tileGameObject = tile._tileGameObject;
        _tileValues = tile._tileValues;
        _tileName = tile._tileName;
        _rotation = tile._rotation;
        _scale = tile._scale;
        _ground = tile._ground;
        _ceiling = tile._ceiling;
    }

    public Tile(float frequencyHint, GameObject tileGameObject, byte[] tileValues, string tileGameObjectName, float rotation, float scale, bool ground, bool ceiling)
    {
        _weight = frequencyHint;
        _tileGameObject = tileGameObject;
        _tileValues = tileValues;
        _tileName = tileGameObjectName;
        _ground = ground;
        _ceiling = ceiling;

        scale = Mathf.Sign(scale);
        _rotation *= Quaternion.Euler(Vector3.up * rotation);

        float x = 1f, z = 1f;
        rotation = Mathf.Round(rotation);
        if (rotation == 90f || rotation == 270f)
            z = scale;
        else
            x = scale;

        _scale = new Vector3(x, 1f, z);
    }

    public Tile(GameObject tileGameObject)
    {
        _tileGameObject = tileGameObject;
        _tileName = tileGameObject.name;
        _weight = 1;

        for (int i = 0; i < 6; i++)
            _edgeAdjacencies[i] = new int[0];
    }

    public string GetName()
    {
        return _tileName + _rotation.eulerAngles.ToString() + " " + _scale.ToString();
    }

    public void CalculateBitValue()
    {
        bool isEmpty = true;
        for (int i = 0; i < 6; i++)
        {
            int bitValue = 0;
            for (int j = 0; j < _edgeAdjacencies[i].Length; j++)
            {
                bitValue |= 1 << _edgeAdjacencies[i][j];
                Debug.Log("bitValue: " + (byte)bitValue);
                isEmpty &= (_edgeAdjacencies[i][j] == 0);
                _ceiling |= (i == 2) & (_edgeAdjacencies[i][j] == 0);
                _ground |= (i == 3) & (_edgeAdjacencies[i][j] == 0);
            }
            _tileValues[i] = (byte)bitValue;
        }
        _ground &= !isEmpty;

    }

    public void SetAdjacencies(int d, List<int> list)
    {
        /*if (_ground)
            list.Add(Model.groundIndex);*/

        _tileAdjacencies[d] = list.ToArray();
    }

    public XElement ToXML()
    {
        string[] adj = new string[6];

        for (int i = 0; i < 6; i++)
        {
            adj[i] = string.Empty;
            if (_edgeAdjacencies[i].Length == 0)
                return null;

            for (int j = 0; j < _edgeAdjacencies[i].Length; j++)
            {
                adj[i] += _edgeAdjacencies[i][j].ToString();
                if (j < _edgeAdjacencies[i].Length - 1)
                    adj[i] += ';';
            }
        }

        XElement tile = new XElement("tile", new XAttribute("name", _tileName), new XAttribute("frequency", _weight),
            new XAttribute("L", adj[0]), new XAttribute("R", adj[1]), new XAttribute("U", adj[2]),
            new XAttribute("D", adj[3]), new XAttribute("F", adj[4]), new XAttribute("B", adj[5]));

        return tile;
    }

    public void CalculateIndex(int tilesNumber)
    {
        long power = 1;
        for (int i = 0; i < _tileValues.Length; i++)
        {
            _index += _tileValues[_tileValues.Length - 1 - i] * power;
            power *= tilesNumber;
        }
    }

    public void PrintTileValues()
    {
        string s = string.Empty;
        for (int i = 0; i < _tileValues.Length; i++)
            s += _tileValues[i].ToString() + " ";

        Debug.Log(s);
    }
}
