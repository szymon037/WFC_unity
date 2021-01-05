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

    public Cell(int index, float entropy, int[][] compatible)
    {
        _index = index;
        _entropy = entropy;
        _possibilities = Model.tiles.Length;

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
    public void ChooseTile()
    {
        float[] frequencyHints = new float[Model.tiles.Length];
        float sum = 0;

        for (int i = 0; i < Model.tiles.Length; i++)
        {
            frequencyHints[i] = (_coefficients[i]) ? Model.tiles[i]._weight : 0f;
            sum += frequencyHints[i];
        }

        float r = Random.value;

        int index = 0;
        float x = 0f;

        for (int i = 0; i < Model.tiles.Length; i++)
        {
            x += frequencyHints[i] / sum;
            if (x >= r)
            {
                index = i;
                break;
            }
        }

        _tile = Model.tiles[index];
        _tileIndex = index;

        for (int i = 0; i < _coefficients.Length; i++)
        {
            if (i != index)
                RemoveTile(i);
        }
    }

    public void UpdatePossibilities(int tileIndex, int dir)
    {
        int[] n = Model.tiles[tileIndex]._adjacencies[dir];
        for (int i = 0; i < n.Length; i++)
        {
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
}