﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WFC_Generator : MonoBehaviour
{
    void Start()
    {
        TiledModel tm = new TiledModel(50, 1, 50, 2, true, true, "Knots");
        tm.Solve();

        /*OverlappingModel om = new OverlappingModel(5, 1, 5, 2, false, 3, 1, false, tm.output);
        om.offset = new Vector3(20f, 0f, 0f);
        om.Solve();*/

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            ReloadScene();
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// TODO: rest of constructor's arguments
    public static void GenerateOverlapping(GameObject[][][] input, Vector3 offset, int tileSize)
    {
        OverlappingModel om = new OverlappingModel(10, 1, 10, tileSize, false, 3, 1, false, input);
        om.offset = offset;
        om.Solve();
    }

    public static void AutoFillTiled(GameObject[][][] inputMap, int tileSize, string setName)
    {
        int x = inputMap.Length;        
        int y = inputMap[0].Length;        
        int z = inputMap[0][0].Length;

        TiledModel tm = new TiledModel(x, y, z, tileSize, false, true, setName, inputMap);
        tm.Solve();
    }

}
