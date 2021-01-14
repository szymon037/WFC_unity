using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WFC_Generator : MonoBehaviour
{
    public static Transform outputTransform;
    void Start()
    {
        TiledModel tm = new TiledModel(5, 5, 5, 2, true, true, "3DKnots");
        tm.Solve();

        OverlappingModel om = new OverlappingModel(5, 5, 5, 2, false, 3, 3, false, tm.output, true);
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
    public static void GenerateOverlapping(Vector3Int dimensions, int tileSize, int N, int N_depth, bool processTiles, GameObject[][][] input, Vector3 offset, bool overlapTileCreation, Transform parent)
    {
        OverlappingModel om = new OverlappingModel(dimensions.x, dimensions.y, dimensions.z, tileSize, false, N, N_depth, processTiles, input, overlapTileCreation, parent);
        om.offset = offset;
        om.Solve();
        outputTransform = om.outputTransform;
    }

    public static void AutoFillTiled(Vector3Int dimensions, int tileSize, bool seamless, bool processTiles, string setName, GameObject[][][] inputMap)
    {
        TiledModel tm = new TiledModel(dimensions.x, dimensions.y, dimensions.z, tileSize, seamless, processTiles, setName, inputMap);
        tm.Solve();
    }

}
