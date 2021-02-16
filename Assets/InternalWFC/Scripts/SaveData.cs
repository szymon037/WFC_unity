using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

[System.Serializable]
public class SaveData
{
    // General data
    public string tilesetName;
    public Vector3Json dimensions;

    // WFC general settings
    public bool seamless;
    public bool processTiles;

    // Overlap settings
    public Vector3Json offset;
    public int N;
    public int N_depth;
    public Vector3Json outputSize;
    public bool overlapTileCreation;

    public int[] builderMap;
}

[System.Serializable]
public struct Vector3Json
{

    public float x;

    public float y;

    public float z;

    public Vector3Json(float rX, float rY, float rZ)
    {
        x = rX;
        y = rY;
        z = rZ;
    }
    
    public static implicit operator Vector3(Vector3Json rValue)
    {
        return new Vector3(rValue.x, rValue.y, rValue.z);
    }

    public static implicit operator Vector3Json(Vector3 rValue)
    {
        return new Vector3Json(rValue.x, rValue.y, rValue.z);
    }

    public static implicit operator Vector3Int(Vector3Json rValue)
    {
        return new Vector3Int((int)rValue.x, (int)rValue.y, (int)rValue.z);
    }


}
sealed class Vector3SerializationSurrogate : ISerializationSurrogate
{

    // Method called to serialize a Vector3 object
    public void GetObjectData(System.Object obj,
                              SerializationInfo info, StreamingContext context)
    {

        Vector3 v3 = (Vector3)obj;
        info.AddValue("x", v3.x);
        info.AddValue("y", v3.y);
        info.AddValue("z", v3.z);
    }

    // Method called to deserialize a Vector3 object
    public System.Object SetObjectData(System.Object obj,
                                       SerializationInfo info, StreamingContext context,
                                       ISurrogateSelector selector)
    {

        Vector3 v3 = (Vector3)obj;
        v3.x = (float)info.GetValue("x", typeof(float));
        v3.y = (float)info.GetValue("y", typeof(float));
        v3.z = (float)info.GetValue("z", typeof(float));
        obj = v3;
        return obj;   // Formatters ignore this return value //Seems to have been fixed!
    }
}