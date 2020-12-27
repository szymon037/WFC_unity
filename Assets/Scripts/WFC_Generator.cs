using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WFC_Generator : MonoBehaviour
{
    void Start()
    {
        TiledModel tm = new TiledModel(4, 4, 2, true, "Knots");
        tm.Solve();

        OverlappingModel om = new OverlappingModel(10, 10, 2, 3, false, tm.output);
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
