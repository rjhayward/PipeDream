using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeVolume : MonoBehaviour
{
    public Mesh mesh;

    // Start is called before the first frame update
    void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.name = "PipeVol";
        GetComponent<MeshCollider>().sharedMesh = mesh;

    }
}
