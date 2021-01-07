using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationTest : MonoBehaviour
{
    Quaternion rot = Quaternion.identity;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(rot.eulerAngles.ToString());

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rot *=  Quaternion.Euler(Vector3.up * 90f);
            Debug.Log(rot.eulerAngles.ToString());
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            rot *= Quaternion.Euler(Vector3.up * -90f);
            Debug.Log(rot.eulerAngles.ToString());
        }
    }
}
