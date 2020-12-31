using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float rotationAngle = 20f;

    private Transform t;
    private float lastMousePosX = 0;

    void Start()
    {
        t = transform;
    }

    void Update()
    {
        if (Input.GetMouseButton(2))
        {
            float currentMousePosX = Input.mousePosition.x;
            float delta = currentMousePosX - lastMousePosX;
            t.Rotate(Vector3.up, rotationAngle * delta);
            lastMousePosX = currentMousePosX;
        }

        lastMousePosX = Input.mousePosition.x;

    }
}
