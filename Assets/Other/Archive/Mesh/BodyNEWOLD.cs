using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class BodyNEWOLD : MonoBehaviour
{
    #region Fields
    [SerializeField] private GameObject capPrefab;
    [SerializeField] private GameObject segmentPrefab;

    [Space]

    [SerializeField] private Transform segments;
    [SerializeField] private Transform caps;
    [SerializeField] private Transform front;
    [SerializeField] private Transform back;

    private GameObject segmentGO, frontCapGO, backCapGO;
    #endregion

    #region Methods
    private void Start()
    {
        Initialize();
    }
    private void Initialize()
    {
        AddSegmentToFront();
        AddSegmentToBack();

        CombineSegments();
    }

    // Add.
    [ContextMenu("Add Segment to Front")]
    private void AddSegmentToFront()
    {
        segmentGO = Instantiate(segmentPrefab, segments, false);
        segmentGO.transform.position = front.position;

        if (frontCapGO != null) { Destroy(frontCapGO); }
        frontCapGO = Instantiate(capPrefab, caps, false);
        frontCapGO.transform.localPosition = Vector3.up * 0.25f;

        front.position = segmentGO.transform.position + Vector3.up * 0.5f;
    }
    [ContextMenu("Add Segment to Back")]
    private void AddSegmentToBack()
    {
        segmentGO = Instantiate(segmentPrefab, segments, false);
        segmentGO.transform.position = back.position;

        if (backCapGO != null) { Destroy(backCapGO); }
        backCapGO = Instantiate(capPrefab, caps, false);

        backCapGO.transform.localPosition = Vector3.up * -0.25f;
        backCapGO.transform.eulerAngles = new Vector3(180, 0, 0);

        back.position = segmentGO.transform.position - Vector3.up * 0.5f;
    }

    // Remove.
    private void RemoveSegment(int dir)
    {

    }

    [ContextMenu("Combine Segments")]
    private void CombineSegments()
    {
        CombineInstance[] combineSegments = new CombineInstance[segments.childCount];
        for (int i = 0; i < combineSegments.Length; i++)
        {
            MeshFilter meshFilter = segments.GetChild(i).GetComponent<MeshFilter>();

            combineSegments[i].mesh = meshFilter.sharedMesh;
            combineSegments[i].transform = meshFilter.transform.localToWorldMatrix;
            meshFilter.gameObject.SetActive(false);
        }

        Mesh mesh = GetComponent<MeshFilter>().sharedMesh = new Mesh();
        mesh.name = "Body";
        mesh.CombineMeshes(combineSegments);

        //AutoWeld(mesh, 0.0001f, 1);
    }
    private void AutoWeld(Mesh mesh, float threshold, float bucketStep)
    {
        Vector3[] oldVertices = mesh.vertices;
        Vector3[] newVertices = new Vector3[oldVertices.Length];
        int[] old2new = new int[oldVertices.Length];
        int newSize = 0;

        // Find AABB
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        for (int i = 0; i < oldVertices.Length; i++)
        {
            if (oldVertices[i].x < min.x) min.x = oldVertices[i].x;
            if (oldVertices[i].y < min.y) min.y = oldVertices[i].y;
            if (oldVertices[i].z < min.z) min.z = oldVertices[i].z;
            if (oldVertices[i].x > max.x) max.x = oldVertices[i].x;
            if (oldVertices[i].y > max.y) max.y = oldVertices[i].y;
            if (oldVertices[i].z > max.z) max.z = oldVertices[i].z;
        }

        // Make cubic buckets, each with dimensions "bucketStep"
        int bucketSizeX = Mathf.FloorToInt((max.x - min.x) / bucketStep) + 1;
        int bucketSizeY = Mathf.FloorToInt((max.y - min.y) / bucketStep) + 1;
        int bucketSizeZ = Mathf.FloorToInt((max.z - min.z) / bucketStep) + 1;
        List<int>[,,] buckets = new List<int>[bucketSizeX, bucketSizeY, bucketSizeZ];

        // Make new vertices
        for (int i = 0; i < oldVertices.Length; i++)
        {
            // Determine which bucket it belongs to
            int x = Mathf.FloorToInt((oldVertices[i].x - min.x) / bucketStep);
            int y = Mathf.FloorToInt((oldVertices[i].y - min.y) / bucketStep);
            int z = Mathf.FloorToInt((oldVertices[i].z - min.z) / bucketStep);

            // Check to see if it's already been added
            if (buckets[x, y, z] == null)
                buckets[x, y, z] = new List<int>(); // Make buckets lazily

            for (int j = 0; j < buckets[x, y, z].Count; j++)
            {
                Vector3 to = newVertices[buckets[x, y, z][j]] - oldVertices[i];
                if (Vector3.SqrMagnitude(to) < threshold)
                {
                    old2new[i] = buckets[x, y, z][j];
                    goto skip; // Skip to next old vertex if this one is already there
                }
            }

            // Add new vertex
            newVertices[newSize] = oldVertices[i];
            buckets[x, y, z].Add(newSize);
            old2new[i] = newSize;
            newSize++;

            skip:;
        }

        // Make new triangles
        int[] oldTris = mesh.triangles;
        int[] newTris = new int[oldTris.Length];
        for (int i = 0; i < oldTris.Length; i++)
        {
            newTris[i] = old2new[oldTris[i]];
        }

        Vector3[] finalVertices = new Vector3[newSize];
        for (int i = 0; i < newSize; i++)
            finalVertices[i] = newVertices[i];

        mesh.Clear();
        mesh.vertices = finalVertices;
        mesh.triangles = newTris;
        mesh.RecalculateNormals();
        mesh.Optimize();
    }
    #endregion
}





































/*
 * 
 * private void ConstructMesh(float radius, float height, int segments, int rings)
    {
        // Calculate the number of vertices for each shape.
        int cylinderVertCount = segments * rings;
        int capVertCount = segments * (segments / 2 - 2) + 1 + segments;

        // Initialize vertices array.
        vertices = new Vector3[cylinderVertCount + 2 * capVertCount];
        int vertIndex = 0;

        // Setup the vertices.
        #region Caps
        foreach (int capDir in (new int[] { -1, 1 }))
        {
            vertices[vertIndex++] = new Vector3(0, capDir * (height * 0.5f + radius), 0);

            for (int ringIndex = segments / 2 - 2; ringIndex >= 0; ringIndex--)
            {
                float percent = ringIndex / (segments / 2 - 1f);
                float ringRadius = Mathf.Cos(90f * percent * Mathf.Deg2Rad) * radius;
                float ringHeight = Mathf.Sin(90f * percent * Mathf.Deg2Rad) * radius;

                for (int i = 1; i < segments + 1; i++, vertIndex++)
                {
                    float angle = i * 360f / segments;

                    float x = ringRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
                    float y = capDir * (ringHeight + height * 0.5f);
                    float z = ringRadius * Mathf.Sin(angle * Mathf.Deg2Rad);

                    vertices[vertIndex] = new Vector3(x, y, z);
                }
            }
        }
        #endregion

        #region Cylinder
        for (int ringIndex = 0; ringIndex < rings; ringIndex++)
        {
            for (int i = 0; i < segments; i++, vertIndex++)
            {
                float angle = i * 360f / segments;

                float x = radius * Mathf.Cos(angle * Mathf.Deg2Rad);
                float y = (ringIndex * height / (rings - 1)) - height / 2f;
                float z = radius * Mathf.Sin(angle * Mathf.Deg2Rad);

                vertices[vertIndex] = new Vector3(x, y, z);
            }
        }
        #endregion






        // Calculate the number of triangles for each shape.
        int cylinderTriCount = segments * (rings - 1) * 2 * 3;
        int capTriCount = (segments * (segments / 2 - 2) * 2 + segments) * 3 + (segments * 3 * 2);

        // Initialize the triangles array.
        triangles = new int[cylinderTriCount + 2 * capTriCount];
        int triIndex = 0;

        // Setup the triangles.

        #region Caps
        foreach (int capDir in (new int[] { -1, 1 }))
        {
            // Top.
            int startTopIndex = (capDir == -1) ? 0 : capVertCount;
            for (int i = 0; i < segments; i++)
            {
                triangles[triIndex + ((capDir == -1) ? 0 : 2)] = startTopIndex;
                triangles[triIndex + 1] = startTopIndex + i + 1;
                triangles[triIndex + ((capDir == -1) ? 2 : 0)] = startTopIndex + i + 2 - (i != segments - 1 ? 0 : segments); // Prevents the need to use a seam.

                triIndex += 3;
            }

            // Main.
            int startMainIndex = startTopIndex + 1;
            for (int ringIndex = 0; ringIndex < segments / 2 - 2; ringIndex++)
            {
                int startRingIndex = startMainIndex + ringIndex * segments;
                for (int i = 0; i < segments; i++)
                {
                    triangles[triIndex + ((capDir == -1) ? 0 : 2)] = startRingIndex + i + 1 - (i != segments - 1 ? 0 : segments); // ''
                    triangles[triIndex + 1] = startRingIndex + i;
                    triangles[triIndex + ((capDir == -1) ? 2 : 0)] = startRingIndex + i + segments;

                    triangles[triIndex + ((capDir == -1) ? 3 : 5)] = startRingIndex + i + segments;
                    triangles[triIndex + 4] = startRingIndex + i + segments + 1 - (i != segments - 1 ? 0 : segments); // ''
                    triangles[triIndex + ((capDir == -1) ? 5 : 3)] = startRingIndex + i + 1 - (i != segments - 1 ? 0 : segments);  // ''

                    triIndex += 6;
                }
            }
        }
        #endregion

        #region Cylinder
        int startCylinderIndex = capVertCount * 2;
        for (int ringIndex = 0; ringIndex < rings - 1; ringIndex++)
        {
            int startRingIndex = startCylinderIndex + ringIndex * segments;
            for (int i = 0; i < segments; i++)
            {
                triangles[triIndex + 0] = startRingIndex + i + 1 - (i != segments - 1 ? 0 : segments); // ''
                triangles[triIndex + 1] = startRingIndex + i;
                triangles[triIndex + 2] = startRingIndex + i + segments;

                triangles[triIndex + 3] = startRingIndex + i + segments;
                triangles[triIndex + 4] = startRingIndex + i + segments + 1 - (i != segments - 1 ? 0 : segments); // ''
                triangles[triIndex + 5] = startRingIndex + i + 1 - (i != segments - 1 ? 0 : segments);  // ''

                triIndex += 6;
            }
        }
        #endregion

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
 * 
*/










//vertices = new Vector3[segments * rings];
//triangles = new int[segments * (rings - 1) * 2 * 3];

//// Cylider
//for (int ringIndex = 0, triIndex = 0; ringIndex < rings; ringIndex++)
//{
//    int startVertIndex = segments * ringIndex;
//    for (int i = 0; i < segments; i++)
//    {
//        vertices[startVertIndex + i] = (Quaternion.AngleAxis(i * 360f / segments, Vector3.up) * Vector3.forward * radius) + (Vector3.up * ((ringIndex * height / (rings - 1)) - height / 2f));

//        if (ringIndex > 0)
//        {
//            triangles[triIndex] = startVertIndex + i - segments;
//            triangles[triIndex + 1] = (startVertIndex - segments) + (i != segments - 1 ? i + 1 : 0);
//            triangles[triIndex + 2] = (startVertIndex) + (i != segments - 1 ? i + 1 : 0);

//            triangles[triIndex + 3] = (startVertIndex) + (i != segments - 1 ? i + 1 : 0);
//            triangles[triIndex + 4] = startVertIndex + i;
//            triangles[triIndex + 5] = startVertIndex + i - segments;

//            triIndex += 6;
//        }
//    }
//}






//2020-07027 02:03
//    using UnityEngine;

//public class Body : MonoBehaviour
//{
//    #region Fields
//    [Header("Body")]
//    [SerializeField] private int maxSegments;

//    [Header("Segment")]
//    [SerializeField] private float radius = 0.25f;
//    [SerializeField] private float height = 0.5f;
//    [SerializeField] [Range(4, 100)] private int segments = 10;
//    [SerializeField] [Range(2, 100)] private int rings = 10;

//    private Mesh mesh;
//    private Vector3[] vertices;
//    private int[] triangles;
//    #endregion

//    private void Awake()
//    {
//        Initialize();
//    }
//    private void OnValidate()
//    {
//        if (mesh != null)
//        {
//            ConstructCapsule(radius, height, segments, rings);
//        }
//    }

//    [ContextMenu("Initialize")]
//    private void Initialize()
//    {
//        if (mesh == null)
//        {
//            gameObject.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));

//            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
//            mesh = meshFilter.sharedMesh = new Mesh();
//            mesh.name = "Segment";
//        }

//        ConstructCapsule(radius, height, segments, rings);
//    }

//    private void ConstructCapsule(float radius, float height, int segments, int rings)
//    {
//        // Calculate the number of vertices and triangles for each shape.
//        int cylinderVertCount = segments * rings;
//        int cylinderTriCount = segments * (rings - 1) * 2 * 3;
//        int capVertCount = segments * (segments / 2 - 2) + 1;
//        int capTriCount = (segments * (segments / 2 - 2) * 2 + segments) * 3;

//        // Initialize vertex and triangle arrays.
//        vertices = new Vector3[cylinderVertCount + 2 * capVertCount];
//        triangles = new int[cylinderTriCount + 2 * capTriCount];

//        int vertIndex = 0, triIndex = 0;

//        #region Caps
//        foreach (int capDir in (new int[] { -1, 1 }))
//        {
//            vertices[vertIndex++] = new Vector3(0, capDir * (height * 0.5f + radius), 0);

//            for (int ringIndex = segments / 2 - 1; ringIndex > 0; ringIndex--)
//            {
//                float percent = ringIndex / (segments / 2 - 1f);
//                float ringRadius = Mathf.Cos(90f * percent * Mathf.Deg2Rad) * radius;
//                float ringHeight = Mathf.Sin(90f * percent * Mathf.Deg2Rad) * radius;

//                for (int i = 1; i < segments + 1; i++, vertIndex++)
//                {
//                    float angle = i * 360f / segments;

//                    float x = ringRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
//                    float y = capDir * (ringHeight + height * 0.5f);
//                    float z = ringRadius * Mathf.Sin(angle * Mathf.Deg2Rad);

//                    vertices[vertIndex] = new Vector3(x, y, z);
//                }
//            }
//        }
//        for (int i = 0; i < segments; i++)
//        {
//            triangles[triIndex] = 0;
//            triangles[triIndex + 1] = i + 1;
//            triangles[triIndex + 2] = i + 2;

//            triIndex += 3;
//        }
//        #endregion

//        #region Cylinder
//        for (int ringIndex = 0; ringIndex < rings; ringIndex++)
//        {
//            for (int i = 0; i < segments; i++, vertIndex++)
//            {
//                float angle = i * 360f / segments;

//                float x = radius * Mathf.Cos(angle * Mathf.Deg2Rad);
//                float y = (ringIndex * height / (rings - 1)) - height / 2f;
//                float z = radius * Mathf.Sin(angle * Mathf.Deg2Rad);

//                vertices[vertIndex] = new Vector3(x, y, z);
//            }
//        }
//        #endregion


//        //if (ringIndex > 0)
//        //{
//        //    triangles[triIndex] = vertIndex - segments;
//        //    triangles[triIndex + 1] = (vertIndex - segments + 1) - (i != segments - 1 ? 0 : segments);
//        //    triangles[triIndex + 2] = (vertIndex + 1) - (i != segments - 1 ? 0 : segments);

//        //    triangles[triIndex + 3] = (vertIndex + 1) - (i != segments - 1 ? 0 : segments);
//        //    triangles[triIndex + 4] = vertIndex;
//        //    triangles[triIndex + 5] = vertIndex - segments;

//        //    triIndex += 6;
//        //}

//        mesh.Clear();
//        mesh.vertices = vertices;
//        mesh.triangles = triangles;

//        mesh.RecalculateNormals();
//    }

//    [ContextMenu("Add Segment")]
//    public void AddSegment(int dir)
//    {

//    }

//    private void OnDrawGizmos()
//    {
//        foreach (Vector3 vertex in vertices)
//        {
//            Gizmos.DrawSphere(vertex + transform.position, 0.005f);
//        }
//    }
//}









//vertices = new Vector3[segments * rings];
//triangles = new int[segments * (rings - 1) * 2 * 3];

//// Cylider
//for (int ringIndex = 0, triIndex = 0; ringIndex < rings; ringIndex++)
//{
//    int startVertIndex = segments * ringIndex;
//    for (int i = 0; i < segments; i++)
//    {
//        vertices[startVertIndex + i] = (Quaternion.AngleAxis(i * 360f / segments, Vector3.up) * Vector3.forward * radius) + (Vector3.up * ((ringIndex * height / (rings - 1)) - height / 2f));

//        if (ringIndex > 0)
//        {
//            triangles[triIndex] = startVertIndex + i - segments;
//            triangles[triIndex + 1] = (startVertIndex - segments) + (i != segments - 1 ? i + 1 : 0);
//            triangles[triIndex + 2] = (startVertIndex) + (i != segments - 1 ? i + 1 : 0);

//            triangles[triIndex + 3] = (startVertIndex) + (i != segments - 1 ? i + 1 : 0);
//            triangles[triIndex + 4] = startVertIndex + i;
//            triangles[triIndex + 5] = startVertIndex + i - segments;

//            triIndex += 6;
//        }
//    }
//}