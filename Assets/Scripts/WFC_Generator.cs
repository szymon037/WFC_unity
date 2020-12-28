using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WFC_Generator : MonoBehaviour
{
    void Start()
    {
        TiledModel tm = new TiledModel(20, 20, 2, true, true, "Knots");
        tm.Solve();

        OverlappingModel om = new OverlappingModel(10, 10, 2, false, 3, false, tm.output);
        om.Solve();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            ReloadAndGenerate();
    }

    public void ReloadAndGenerate()
    {

        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
