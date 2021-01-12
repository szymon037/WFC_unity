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
    public Matrix4x4 _transform = Matrix4x4.identity;
    public string _tileName;  // original name, not changed with rotation or (Clone)
    //public string[] _stringAdjacencies;
    public Tile() { }

    public Tile(byte[] values, int N, int N_depth, int gameObjectsCount, float rotation, float scale)
    {
        _tileValues = values;

        long power = 1;
        for (int i = 0; i < _tileValues.Length; i++)
        {
            _index += _tileValues[_tileValues.Length - 1 - i] * power;
            power *= gameObjectsCount;
        }

        scale = Mathf.Sign(scale);

        Quaternion rot = _transform.rotation * Quaternion.Euler(Vector3.up * rotation);
        Matrix4x4 rotMatrix = Matrix4x4.Rotate(rot);
        Matrix4x4 scaleMatrix = Matrix4x4.Scale(new Vector3(1f, 1f, scale));
        _transform = rotMatrix * scaleMatrix;
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
        _transform = tile._transform;
        /*Debug.Log(_tileGameObjectName);
        for (int i = 0; i < _adjacencies[0].Length; i++)
        {
            Debug.Log(tile._adjacencies[0][i]);
        }*/
        //_stringAdjacencies = tile._stringAdjacencies;
    }

    public Tile(float frequencyHint, GameObject tileGameObject, byte[] tileValues, string tileGameObjectName, float rotation, float scale)
    {
        _weight = frequencyHint;
        _tileGameObject = tileGameObject;
        _tileValues = tileValues;
        _tileName = tileGameObjectName;

        Quaternion rot = _transform.rotation * Quaternion.Euler(Vector3.up * rotation);
        Matrix4x4 rotMatrix = Matrix4x4.Rotate(rot);
        Matrix4x4 scaleMatrix = Matrix4x4.Scale(new Vector3(1f, 1f, scale));
        _transform = rotMatrix * scaleMatrix;
    }

    public Tile(GameObject tileGameObject)
    {
        _tileGameObject = tileGameObject;
    }

    public string GetName()
    {
        return _tileName + _transform.rotation.eulerAngles.ToString() + " " + _transform.lossyScale.ToString();
    }

    public void CalculateBitValue()
    {
        for (int i = 0; i < 6; i++)
        {
            int bitValue = 0;
            for (int j = 0; j < _edgeAdjacencies[i].Length; j++)
            {
                bitValue |= 1 << _edgeAdjacencies[i][j];
                //Debug.Log(_adjacencies[i][j]);
            }
            _tileValues[i] = (byte)bitValue;
        }
        
    }

    public XElement ToXML()
    {
        string[] adj = new string[6];

        // split test: 1;

        for (int i = 0; i < 6; i++)
            for (int j = 0; j < _edgeAdjacencies[i].Length; j++)
            {
                adj[i] += _edgeAdjacencies[i][j].ToString();
                if (j < _edgeAdjacencies[i].Length - 1)
                    adj[i] += ';';
            }

        XElement tile = new XElement("tile", new XAttribute("name", _tileName), new XAttribute("frequency", _weight),
            new XAttribute("L", adj[0]), new XAttribute("R", adj[1]), new XAttribute("U", adj[2]),
            new XAttribute("D", adj[3]), new XAttribute("F", adj[4]), new XAttribute("B", adj[5]));

        return tile;
    }
}
