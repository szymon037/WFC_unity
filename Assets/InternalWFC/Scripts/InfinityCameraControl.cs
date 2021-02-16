using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfinityCameraControl : MonoBehaviour
{
    public float movementSpeed;
    public float rotationSensitivity;
    private Transform t;

    private float rotationX;
    private float rotationY;

    private Quaternion startRotation;
    void Start()
    {
        t = transform;
        startRotation = t.rotation;
    }

    void Update()
    {
        // Camera Movement
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        t.Translate(t.forward * y * movementSpeed + t.right * x * movementSpeed, Space.World);

        // Camera Rotation
        rotationX += Input.GetAxis("Mouse X") * rotationSensitivity;
        rotationY += Input.GetAxis("Mouse Y") * rotationSensitivity;

        Quaternion xRot = Quaternion.AngleAxis(rotationX, Vector3.up);
        Quaternion yRot = Quaternion.AngleAxis(rotationY, Vector3.left);

        t.rotation = startRotation * xRot * yRot;
    }
}
