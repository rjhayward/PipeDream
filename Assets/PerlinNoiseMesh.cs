using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoiseMesh : MonoBehaviour
{
    //local fields
    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;


    public PerlinNoiseMesh meshGenerator;

    void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.name = "Perlin";
        mesh.Clear();

        meshGenerator.RenderPerlinSquares(Random.Range(0.1f, 0.9f));
        //meshGenerator.RenderPerlin(Random.Range(0.1f, 0.9f));


        //GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    public Vector3 GetPointOnPerlinNoise(float x, float y)
    {
        float z = Mathf.PerlinNoise(x, y);
        //Debug.Log(z);
        return new Vector3(x, z, y);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            meshGenerator.RenderPerlinSquares(Random.Range(0.1f, 0.9f));
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
        }

    }

    //void OnDrawGizmosSelected()
    //{
    //    int columns = 8;
    //    int rows = 8;
    //    int levels = 4;
    //    float heightMod = 3f;
    //    float squareSize = 4f;

    //    Vector3 perlinNoise;
    //    float noiseVal = 0.4762f;   
    //    vertices = new Vector3[(2 * columns * 2 * rows)];


    //    int index = 0;
    //    for (int x = 0; x < 2 * columns; x++)
    //    {
    //        for (int y = 0; y < rows; y++)
    //        {
    //            //if (index <= rows * columns)
    //            //{
    //            //if ((index-2 % (2*columns) != 0) && (index-4 % (2 * columns) != 0) && (index-6 % (2 * columns) != 0))
    //            //{
    //            if (x % 2 == 0)//&&(x % 2 != 0))
    //            {
    //                perlinNoise = GetPointOnPerlinNoise(x + noiseVal, y * 2 + noiseVal);
    //                perlinNoise = new Vector3(Mathf.RoundToInt(perlinNoise.x * levels), Mathf.RoundToInt(perlinNoise.y * levels), Mathf.RoundToInt(perlinNoise.z * levels));
    //                perlinNoise.y *= heightMod;
    //                //Debug.Log(index + ": " + perlinNoise);
    //                //vertices[index] = perlinNoise;
    //                Gizmos.DrawSphere(transform.position + perlinNoise, 0.5f);

    //                perlinNoise = GetPointOnPerlinNoise(x + noiseVal, y * 2 + noiseVal);
    //                perlinNoise = new Vector3(Mathf.RoundToInt(perlinNoise.x * levels), Mathf.RoundToInt(perlinNoise.y * levels), Mathf.RoundToInt(perlinNoise.z * levels));
    //                perlinNoise.y *= heightMod;
    //                perlinNoise.z += squareSize;
    //                //Debug.Log(index + 1 + ": " + perlinNoise);
    //                //vertices[index + 1] = perlinNoise;
    //                Gizmos.DrawSphere(transform.position + perlinNoise, 0.5f);

    //                perlinNoise = GetPointOnPerlinNoise(x + noiseVal, y * 2 + noiseVal);
    //                perlinNoise = new Vector3(Mathf.RoundToInt(perlinNoise.x * levels), Mathf.RoundToInt(perlinNoise.y * levels), Mathf.RoundToInt(perlinNoise.z * levels));
    //                perlinNoise.y *= heightMod;
    //                perlinNoise.x += squareSize;
    //                //Debug.Log(index + 6 + ": " + perlinNoise);
    //                //vertices[index + 6] = perlinNoise;
    //                Gizmos.DrawSphere(transform.position + perlinNoise, 0.5f);

    //                perlinNoise = GetPointOnPerlinNoise(x + noiseVal, y * 2 + noiseVal);
    //                perlinNoise = new Vector3(Mathf.RoundToInt(perlinNoise.x * levels), Mathf.RoundToInt(perlinNoise.y * levels), Mathf.RoundToInt(perlinNoise.z * levels));
    //                perlinNoise.y *= heightMod;
    //                perlinNoise.x += squareSize;
    //                perlinNoise.z += squareSize;
    //                //Debug.Log(index + 7 + ": " + perlinNoise);
    //                //vertices[index + 7] = perlinNoise;
    //                Gizmos.DrawSphere(transform.position + perlinNoise, 0.5f);
    //            }
    //            //}
    //            //}
    //            index += 2;
    //        }
    //    }
    //}

    public void RenderPerlin(float noiseVal)
    {

        // fill vertices[] with stuff
        int columns = 8;
        int rows = 8;
        int index = 0;
        int levels = 40;

        Vector3 perlinNoise = GetPointOnPerlinNoise(noiseVal, noiseVal);

        vertices = new Vector3[(columns * rows) + 2];

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {

                perlinNoise = GetPointOnPerlinNoise(x + noiseVal, y + noiseVal);

                perlinNoise = new Vector3(Mathf.RoundToInt(perlinNoise.x * levels), Mathf.RoundToInt(perlinNoise.y * levels), Mathf.RoundToInt(perlinNoise.z * levels));
                perlinNoise.y *= 1.5f;

                //Debug.Log(index + ": " + perlinNoise);
                vertices[index++] = perlinNoise;
                //Gizmos.DrawSphere(transform.position + perlinNoise, 0.1f);
            }
        }

        mesh.vertices = vertices;

        vertices = new Vector3[6 * columns * rows];

        List<int> tris = new List<int>();

        for (int i = 0; ((i + rows) < (columns * rows)); i++)
        {
            if ((i + 1) % rows != 0)
            {
                //if ((i + rows) < (columns * rows))
                //{
                tris.Add(i);
                tris.Add(i + 1);
                tris.Add(i + rows);

                tris.Add(i + 1);
                tris.Add(i + 1 + rows);
                tris.Add(i + rows);
                //}
            }
        }

        for (int i = 0; i < tris.Count; i++)
        {
            //Debug.Log(tris[i]);
        }

        // fill triangles[] with stuff

        mesh.triangles = tris.ToArray();

    }

    public void RenderPerlinSquares(float noiseVal)
    {
        mesh.Clear();

        // fill vertices[] with stuff
        int columns = 30;
        int rows = 30;
        int levels = 100;
        float heightMod = 9f;
        float squareSize = 100f;

        Vector3 perlinNoise;

        vertices = new Vector3[(2 * columns * 2 * rows)];


        int index = 0;
        for (int x = 0; x < 2 * columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                //if (index <= rows * columns)
                //{
                //if ((index-2 % (2*columns) != 0) && (index-4 % (2 * columns) != 0) && (index-6 % (2 * columns) != 0))
                //{
                if (x % 2 == 0)//&&(x % 2 != 0))
                {
                    perlinNoise = GetPointOnPerlinNoise(x + noiseVal, y * 2 + noiseVal);
                    perlinNoise = new Vector3(Mathf.RoundToInt(perlinNoise.x * levels), Mathf.RoundToInt(perlinNoise.y * levels), Mathf.RoundToInt(perlinNoise.z * levels));
                    perlinNoise.y *= heightMod;
                    //Debug.Log(index + ": " + perlinNoise);
                    vertices[index] = perlinNoise;

                    perlinNoise = GetPointOnPerlinNoise(x + noiseVal, y * 2 + noiseVal);
                    perlinNoise = new Vector3(Mathf.RoundToInt(perlinNoise.x * levels), Mathf.RoundToInt(perlinNoise.y * levels), Mathf.RoundToInt(perlinNoise.z * levels));
                    perlinNoise.y *= heightMod;
                    perlinNoise.z += squareSize;
                    //Debug.Log(index + 1 + ": " + perlinNoise);
                    vertices[index + 1] = perlinNoise;

                    perlinNoise = GetPointOnPerlinNoise(x + noiseVal, y * 2 + noiseVal);
                    perlinNoise = new Vector3(Mathf.RoundToInt(perlinNoise.x * levels), Mathf.RoundToInt(perlinNoise.y * levels), Mathf.RoundToInt(perlinNoise.z * levels));
                    perlinNoise.y *= heightMod;
                    perlinNoise.x += squareSize;
                    //Debug.Log(index + rows * 2 + ": " + perlinNoise);
                    vertices[index + rows * 2] = perlinNoise;

                    perlinNoise = GetPointOnPerlinNoise(x + noiseVal, y * 2 + noiseVal);
                    perlinNoise = new Vector3(Mathf.RoundToInt(perlinNoise.x * levels), Mathf.RoundToInt(perlinNoise.y * levels), Mathf.RoundToInt(perlinNoise.z * levels));
                    perlinNoise.y *= heightMod;
                    perlinNoise.x += squareSize;
                    perlinNoise.z += squareSize;
                    //Debug.Log(index + rows * 2 + 1 + ": " + perlinNoise);
                    vertices[index + rows * 2 + 1] = perlinNoise;
                }
                //}
                //}
                index += 2;
            }
        }


        //for (int x = 0; x < columns; x++)
        //{
        //    for (int y = 0; y < rows; y++)
        //    {

        //        perlinNoise = GetPointOnPerlinNoise(x + 0.323465f, y + 0.323465f);
        //        perlinNoise = new Vector3(Mathf.RoundToInt(perlinNoise.x * levels), Mathf.RoundToInt(perlinNoise.y * levels), Mathf.RoundToInt(perlinNoise.z * levels));
        //        perlinNoise.y *= 1.5f;

        //        Debug.Log(index + ": " + perlinNoise);
        //        vertices[index++] = perlinNoise;
        //        //Gizmos.DrawSphere(transform.position + perlinNoise, 0.1f);
        //    }
        //}

        mesh.vertices = vertices;

        vertices = new Vector3[6 * 2 * columns * 2 * rows];

        List<int> tris = new List<int>();

        for (int i = 0; i < (2 * columns * 2 * rows); i++)
        {
            if ((i + 1) % (rows * 2) != 0)
            {
                if ((i + 1 + 2 * rows) < (2 * columns * 2 * rows))
                {
                    tris.Add(i);
                    tris.Add(i + 1);
                    tris.Add(i + 2 * rows);

                    tris.Add(i + 1);
                    tris.Add(i + 1 + 2 * rows);
                    tris.Add(i + 2 * rows);
                }
            }
        }

        for (int i = 0; i < tris.Count; i++)
        {
            //Debug.Log(tris[i]);
        }

        // fill triangles[] with stuff

        mesh.triangles = tris.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

    }
}
