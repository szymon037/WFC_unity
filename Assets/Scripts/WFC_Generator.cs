using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WFC_Generator : MonoBehaviour
{
    void Start()
    {
        TiledModel tm = new TiledModel(5, 5, 5, 2, true, true, "3DKnots");
        tm.Solve();

        OverlappingModel om = new OverlappingModel(5, 5, 5, 2, false, 3, 3, false, tm.output);
        om.offset = new Vector3(20f, 0f, 0f);
        om.Solve();

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
    public static void GenerateOverlapping(GameObject[][][] input, Vector3 offset, int tileSize, bool processTiles)
    {
        OverlappingModel om = new OverlappingModel(100, 1, 100, tileSize, false, 3, 1, processTiles, input);
        om.offset = offset;
        om.Solve();
    }

    public static void AutoFillTiled(Vector3Int dimensions, GameObject[][][] inputMap, int tileSize, string setName)
    {
        TiledModel tm = new TiledModel(dimensions.x, dimensions.y, dimensions.z, tileSize, false, true, setName, inputMap);
        tm.Solve();
    }

}
