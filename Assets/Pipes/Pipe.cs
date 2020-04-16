using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pipe : MonoBehaviour
{
    //parameters of the pipe- changeable in Unity editor
    public float torusRadius;
    public float pipeRadius;
    public int torusSegments;
    public int pipeSegments;

    //local fields
    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;

    public PipeVolume pipeVolPrefab;
    PipeVolume pipeVolume;
    public float rotate;

    void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.name = "Pipe";
        GetComponent<MeshCollider>().sharedMesh = mesh;
        pipeVolume = Instantiate(pipeVolPrefab);
        pipeVolume.transform.parent = transform;
    }

    public void AttachAsNewStraightPipe(Pipe originalPipe)
    {
        transform.SetParent(originalPipe.transform, false);
        transform.localPosition = Vector3.zero;

        transform.Translate(0f, -originalPipe.torusRadius / 2, 0f, originalPipe.transform);

        transform.SetParent(originalPipe.transform.parent);
    }

    //algorithm followed to attach one pipe to another, cohering to each other properly
    public void AttachAsNewPipe(Pipe originalPipe)
    {
        transform.SetParent(originalPipe.transform, false);
        //transform.localPosition = Vector3.zero;

        //rotates the pipe randomly but still in such a way that the rotation still allows the pipe to cohere to the other 
        transform.Rotate(0f, Mathf.RoundToInt(Random.Range((pipeSegments - 1) / 2, (pipeSegments - 1))) * (360f / (pipeSegments - 1)), 0f);

        transform.Translate(originalPipe.torusRadius, 0f, 0f, originalPipe.transform);

        transform.Translate(-torusRadius, 0f, 0f);

        transform.Rotate(0f, 0f, -rotate);

        transform.SetParent(originalPipe.transform.parent);
    }

    //used to get a point on a torus with given u/v values (u = angle along the torus, v = angle along the pipe)
    private Vector3 GetPoint(float u, float v)
    {
        Vector3 point;

        point.x = (torusRadius + pipeRadius * Mathf.Cos(v)) * Mathf.Cos(u);
        point.y = (torusRadius + pipeRadius * Mathf.Cos(v)) * Mathf.Sin(u);
        point.z = pipeRadius * Mathf.Sin(v);

        return point;
    }

    //renders the volume which when passed through adds new pipes, deletes old pipes and increments the score by 1
    public void RenderVolume()
    {
        Vector3[] volumeVertices = new Vector3[pipeSegments + 1];

        volumeVertices[0] = (GetPoint(0, 2f * Mathf.PI)).normalized * (GetPoint(0, 2f * Mathf.PI).magnitude);// - pipeRadius);

        float v = 0;

        //used to generate the array of vertices as required 
        for (int i = 0; i < pipeSegments; i++)
        {
            Vector3 point = GetPoint(0, v);

            volumeVertices[i + 1] = point;

            v += (2f * Mathf.PI) / (pipeSegments - 1);
        }

        //sets our mesh's vertex array to equal the array we have just created
        pipeVolume.mesh.vertices = volumeVertices;

        int[] volumeTriangles = new int[3 + (pipeSegments) * 3];

        //used to generate the array of triangles as required 
        for (int n = 1; n < pipeSegments; n++)
        {
            volumeTriangles[3 * (n - 1)] = n;
            volumeTriangles[3 * (n - 1) + 1] = 0;
            volumeTriangles[3 * (n - 1) + 2] = (n + 1) % pipeSegments;
        }

        //sets our mesh's triangles array to equal the array we have just created
        pipeVolume.mesh.triangles = volumeTriangles;

        pipeVolume.mesh.RecalculateBounds();
        pipeVolume.mesh.RecalculateNormals();


        pipeVolume.transform.position = transform.position;
    }

    //renders a straight pipe instead of one which is a section of a torus
    public void RenderStraightPipe()
    {
        rotate = 360;

        mesh.Clear();

        vertices = new Vector3[2 * (pipeSegments - 1)];

        float v = 0;

        //used to generate the array of vertices as required 
        for (int i = 0; i < (pipeSegments - 1); i++)
        {
            Vector3 point1 = GetPoint(0, v);
            Vector3 point2 = GetPoint(0, v) + torusRadius / 2 * transform.up;

            v += (2f * Mathf.PI) / (pipeSegments - 1);
            vertices[2 * i] = point1;
            vertices[2 * i + 1] = point2;
        }

        //sets our mesh's vertex array to equal the array we have just created
        mesh.vertices = vertices;

        int maxVertex = (2 * (pipeSegments - 1)) - 1;

        triangles = new int[(maxVertex + 1) * 3];

        /**
         * (example triangles array)
         * 
         * 012 123 234 345 456 567 670
         *  
         **/

        //used to generate the array of triangles as required 
        for (int i = 0; i < maxVertex + 1; i++)
        {
            if (i % 2 == 1)
            {

                triangles[3 * i] = i;
                triangles[3 * i + 1] = (i + 1) % (maxVertex + 1);
                triangles[3 * i + 2] = (i + 2) % (maxVertex + 1);
            }
            else
            {
                triangles[3 * i] = (i + 2) % (maxVertex + 1);
                triangles[3 * i + 1] = (i + 1) % (maxVertex + 1);
                triangles[3 * i + 2] = i;
            }
        }

      
        //sets our mesh's triangles array to equal the array we have just created
        mesh.triangles = triangles;


        Mesh.MeshDataArray dataArray = Mesh.AcquireReadOnlyMeshData(mesh);
        
        mesh = new Mesh();
        Mesh mesh1 = new Mesh();
        
        Mesh.ApplyAndDisposeWritableMeshData(dataArray, mesh1);

        mesh = mesh1;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    //renders the pipe
    public void RenderPipe(Vector3 startPoint, float percentage)
    {
        mesh.Clear();

        int renderTorusSegments = Mathf.RoundToInt(torusSegments * percentage);

        vertices = new Vector3[renderTorusSegments * pipeSegments * 4];

        //gives a rotation in degrees which lets all segments stay of equal size
        rotate = 360 * ((float)renderTorusSegments / (torusSegments - 1));

        float u = 0;
        float v = 0;

        //used to generate the array of vertices as required 
        for (int i = 0; i < renderTorusSegments; i++) //for each torusSegment, we want to save all vertices in the loop (2 each time for a pipeSegments amount of times)
        {
            for (int j = 0; j < 2 * (pipeSegments - 1); j++)
            {

                Vector3 point1 = startPoint + GetPoint(u + (2f * Mathf.PI / (torusSegments - 1)), v);

                Vector3 point2 = startPoint + GetPoint(u, v);

                vertices[i * (pipeSegments * 2) + j * 2] = point1;
                vertices[i * (pipeSegments * 2) + j * 2 + 1] = point2;

                v += (2f * Mathf.PI) / (pipeSegments - 1);
            }
            u += (2f * Mathf.PI) / (torusSegments - 1);
        }

        //sets our mesh's vertex array to equal the array we have just created
        mesh.vertices = vertices;

        triangles = new int[renderTorusSegments * (pipeSegments) * 6];

        int vertex = 0;
        // used to generate the array of triangles as required 
        for (int j = 0; j < renderTorusSegments; j++)
        {
            for (int i = 0; i < pipeSegments; i++)
            {
                triangles[6 * (pipeSegments) * j + 6 * i + 0] = vertex + 3;    //0  //reverse these for outside view
                triangles[6 * (pipeSegments) * j + 6 * i + 1] = vertex + 2;    //2
                triangles[6 * (pipeSegments) * j + 6 * i + 2] = vertex + 0;    //3

                triangles[6 * (pipeSegments) * j + 6 * i + 3] = vertex + 0;    //3  //reverse these for outside view
                triangles[6 * (pipeSegments) * j + 6 * i + 4] = vertex + 1;    //1
                triangles[6 * (pipeSegments) * j + 6 * i + 5] = vertex + 3;    //0 

                //vertex += 2;

                //triangles[6 * 2 * (pipeSegments) * j + 6 * 2 * i + 6] = vertex + 3;    //0  //reverse these for outside view
                //triangles[6 * 2 * (pipeSegments) * j + 6 * 2 * i + 7] = vertex + 2;    //2
                //triangles[6 * 2 * (pipeSegments) * j + 6 * 2 * i + 8] = vertex + 0;    //3

                //triangles[6 * 2 * (pipeSegments) * j + 6 * 2 * i + 9] = vertex + 0;    //3  //reverse these for outside view
                //triangles[6 * 2 * (pipeSegments) * j + 6 * 2 * i + 10] = vertex + 1;    //1
                //triangles[6 * 2 * (pipeSegments) * j + 6 * 2 * i + 11] = vertex + 3;    //0 

                vertex += 2;
            }
        }

        //sets our mesh's triangles array to equal the array we have just created
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
}