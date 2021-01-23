using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public int _index = -1;
    public int _tileIndex = -1;

    public bool[] _coefficients = new bool[Model.tiles.Length];   // one bool for one tile
    public Tile _tile = null;

    public float _entropy;
    public int _possibilities;
    public int[][] _compatible = new int[Model.tiles.Length][];
    private bool _ceiling;
    private int _x;
    private int _y;
    private int _z;
    public Cell(int index, float entropy, int[][] compatible, bool ceiling, int x, int z, int y)
    {
        _index = index;
        _entropy = entropy;
        _possibilities = Model.tiles.Length;
        _ceiling = ceiling;
        _x = x;
        _z = z;
        _y = y;

        for (int i = 0; i < _coefficients.Length; i++)
        {
            _coefficients[i] = true;
        }

        for (int i = 0; i < compatible.Length; i++)
        {
            _compatible[i] = new int[6];
            for (int d = 0; d < 6; d++)
            {
                _compatible[i][d] = compatible[i][d];
            }
        }
    }

    public Cell(int index, int tileIndex) // Used for autofill
    {
        _index = index;
        //_tileIndex = tileIndex;
        //_tile = Model.tiles[_tileIndex];

        for (int i = 0; i < Model.tiles.Length; i++)
            _coefficients[i] = (_tileIndex) == i;

        _possibilities = 1;
        _entropy = 0f;


        for (int i = 0; i < Model.tiles.Length; i++)
        {
            _compatible[i] = new int[6];
            for (int j = 0; j < 6; j++)
            {
                _compatible[i][j] = Model.tiles[i]._tileAdjacencies[j].Length;
            }
        }
        ChooseTile(tileIndex);
        Debug.Log("name of tgo: " + _tile._tileGameObject.name);
        /*
        for (int i = 0; i < _coefficients.Length; i++)
        {
            if (i != _tileIndex)
                RemoveTile(i);
        }*/
    }

    public void ChooseTile(int tileIndex = -1)
    {
        ChooseCeiling();
            BanGroundTiles();


        if (tileIndex == -1)
        {
            tileIndex = 0;
            float[] frequencyHints = new float[Model.tiles.Length];
            float sum = 0;

            for (int i = 0; i < Model.tiles.Length; i++)
            {
                frequencyHints[i] = (_coefficients[i]) ? Model.tiles[i]._weight : 0f;
                sum += frequencyHints[i];
            }

            float r = Random.value;

            float x = 0f;

            for (int i = 0; i < Model.tiles.Length; i++)
            {
                x += frequencyHints[i] / sum;
                if (x >= r)
                {
                    tileIndex = i;
                    break;
                }
            }
        }

        _tile = Model.tiles[tileIndex];
        _tileIndex = tileIndex;

        if (_tile._ground)
        {
            Model.floorCheck[_x, _z] = true;
            //Debug.Log("Floor set: " + _x + " " + _z);
        }

        for (int i = 0; i < _coefficients.Length; i++)
        {
            if (i != tileIndex)
                RemoveTile(i);
        }
    }

    public void UpdatePossibilities(int tileIndex, int dir)
    {
        if (tileIndex == -1)
            return;

        int[] n = Model.tiles[tileIndex]._tileAdjacencies[dir];
        for (int i = 0; i < n.Length; i++)
        {
            if (n[i] == -1)
                continue;

            _compatible[n[i]][Model.opposite[dir]]--;
            if (_compatible[n[i]][Model.opposite[dir]] == 0)
                RemoveTile(n[i]);
        }
        if (_possibilities == 1)
            ChooseTile();
    }

    public int[] GetRemovedTiles()
    {
        List<int> removed = new List<int>();
        for (int i = 0; i < _coefficients.Length; i++)
        {
            if (!_coefficients[i])
                removed.Add(i);
        }

        return removed.ToArray();
    }


    public Tile GetTile()
    {
        return _tile;
    }

    private void RemoveTile(int tileIndex)
    {
        if (!_coefficients[tileIndex])
            return;

        _entropy -= Model.tiles[tileIndex]._weight;
        _possibilities -= 1;
        _coefficients[tileIndex] = false;

        if (_possibilities == 1)
        {
            _entropy = 0;
        }

        for (int i = 0; i < 6; i++)
        {
            _compatible[tileIndex][i] = 0;
        }

        Model.stack[Model.stackSize] = (_index, tileIndex);
        Model.stackSize++;
    }

    private void ChooseCeiling()
    {
        if (!_ceiling)
            return;

        for (int i = 0; i < _coefficients.Length; i++)
            if (!Model.tiles[i]._ceiling)
                RemoveTile(i);
    }

    private void BanGroundTiles()
    {
        if (!Model.floorCheck[_x, _z])
            return;

        for (int i = 0; i < _coefficients.Length; i++)
            if (Model.tiles[i]._ground && i != _tileIndex)
                RemoveTile(i);
    }
}