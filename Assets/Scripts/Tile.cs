using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    public float _weight;
    public int[][] _adjacencies = new int[4][];  // L - 0, R - 1, U - 2, D - 3
    public byte[] _tileValues;
    public long _index;
    public GameObject _tileGameObject = null;

    public Tile() { }

    public Tile(byte[] values)
    {
        values = _tileValues;
    }

    // overlapping
    public Tile(byte[] values, int N, int gameObjectsCount)
    {
        _tileValues = values;

        long power = 1;
        for (int i = 0; i < N * N; i++)
        {
            _index += _tileValues[i] * power;
            power *= gameObjectsCount;
        }
    }

    // custom
    public Tile(Tile tile)
    {
        _weight = tile._weight;
        _tileGameObject = tile._tileGameObject;
        _tileValues = tile._tileValues;
    }

    public Tile(float frequencyHint, GameObject tileGameObject, byte[] tileValues)
    {
        _weight = frequencyHint;
        _tileGameObject = tileGameObject;
        _tileValues = tileValues;


    }
}
