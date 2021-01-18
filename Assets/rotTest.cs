using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotTest : MonoBehaviour
{
    public Vector3 rotation = Vector3.zero;
    public Vector3 scale = Vector3.one;
    [Space]
    public Vector3 finRotation = Vector3.zero;

    void Start()
    {
        
    }

    void Update()
    {
        Matrix4x4 _transform = Matrix4x4.identity;
        Quaternion rot = _transform.rotation * Quaternion.Euler(rotation);
        Matrix4x4 rotMatrix = Matrix4x4.Rotate(rot);
        Matrix4x4 scaleMatrix = Matrix4x4.Scale(scale);
        Matrix4x4 transMatrix = Matrix4x4.Translate(Vector3.zero);
        _transform.SetTRS(Vector3.zero, rot, scale);// = scaleMatrix * rotMatrix * transMatrix;
        Debug.Log(_transform.ValidTRS());
        finRotation = _transform.rotation.eulerAngles;
    }
}
