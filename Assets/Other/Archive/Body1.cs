// Creature Creator
// Version: 1.0.0
// Author: Daniel Lochner

using BasicTools.ButtonInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Body1 : MonoBehaviour
{
    #region Fields
    [SerializeField] private BodySettings bodySettings;

    [Space]

    [SerializeField] [Button("Add to Front", "AddToFront")] private bool addToFront;
    [SerializeField] [Button("Add to Back", "AddToBack")] private bool addToBack;
    [SerializeField] [Button("Remove from Front", "RemoveFromFront")] private bool removeFromFront;
    [SerializeField] [Button("Remove from Back", "RemoveFromBack")] private bool removeFromBack;

    private Mesh mesh;
    private SkinnedMeshRenderer skinnedMeshRenderer;

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<int> segments = new List<int>();
    #endregion

    #region Methods
    private void Awake()
    {
        Initialize();
    }
    private void Initialize()
    {
        if (!mesh)
        {
            GameObject model = new GameObject("Model");
            model.transform.SetParent(transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;

            GameObject root = new GameObject("Root");
            root.transform.SetParent(transform);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;

            skinnedMeshRenderer = model.AddComponent<SkinnedMeshRenderer>();
            skinnedMeshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
            skinnedMeshRenderer.sharedMesh = mesh = model.AddComponent<MeshFilter>().sharedMesh = new Mesh();
            skinnedMeshRenderer.rootBone = root.transform;
            skinnedMeshRenderer.updateWhenOffscreen = true;

            mesh.name = "Body";
        }

        AddEndSegment(-1);
        AddEndSegment(1);

        DisplayVertices(0f);
    }

    private void AddEndSegment(int dir)
    {
        if (segments.Count < bodySettings.MinMaxSegments.y)
        {
            if (segments.Count(x => x == dir) > 0)
            {
                RemoveEnd(dir, CountVertices(dir));
                AddSegment(dir, CountVertices(dir) - 1);
            }
            else
            {
                AddSegment(dir, 0);
            }
            AddEnd(dir, CountVertices(dir) - 1);

            if (segments.Count >= bodySettings.MinMaxSegments.x)
            {
                Setup();
            }
        }
    }
    private void AddSegment(int dir, int startVertex)
    {
        float zOffset = (vertices.Count > 0) ? vertices[startVertex].z : 0;

        for (int ringIndex = 1; ringIndex < bodySettings.Rings; ringIndex++)
        {
            for (int i = 0; i < bodySettings.Segments; i++)
            {
                float angle = dir * i * 360f / bodySettings.Segments;

                float x = bodySettings.Radius * Mathf.Cos(angle * Mathf.Deg2Rad);
                float y = bodySettings.Radius * Mathf.Sin(angle * Mathf.Deg2Rad);
                float z = dir * (ringIndex * bodySettings.Length / (bodySettings.Rings - 1));

                vertices.Add(new Vector3(x, y, z + zOffset));
            }
        }
        segments.Add(dir);
    }
    private void AddEnd(int dir, int startVertex, bool insertAtStartVertex = false)
    {
        float zOffset = (vertices.Count > 0) ? vertices[startVertex].z : 0;

        List<Vector3> tempVertices = new List<Vector3>();
        for (int ringIndex = 1; ringIndex < bodySettings.Segments / 2 - 1; ringIndex++)
        {
            float percent = ringIndex / (bodySettings.Segments / 2 - 1f);
            float ringRadius = Mathf.Cos(90f * percent * Mathf.Deg2Rad) * bodySettings.Radius;
            float ringDistance = Mathf.Sin(90f * percent * Mathf.Deg2Rad) * bodySettings.Radius;

            for (int i = 0; i < bodySettings.Segments; i++)
            {
                float angle = dir * i * 360f / bodySettings.Segments;

                float x = ringRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
                float y = ringRadius * Mathf.Sin(angle * Mathf.Deg2Rad);
                float z = dir * ringDistance;

                tempVertices.Add(new Vector3(x, y, z + zOffset));
            }
        }
        tempVertices.Add(new Vector3(0, 0, (dir * bodySettings.Radius) + zOffset));

        vertices.InsertRange(insertAtStartVertex ? (startVertex + 1) : vertices.Count, tempVertices);
    }

    private void RemoveEndSegment(int dir)
    {
        if (segments.Count > bodySettings.MinMaxSegments.x)
        {
            int dirCount = segments.Count(x => x == dir);

            RemoveEnd(dir, CountVertices(dir));
            RemoveSegment(dir, CountVertices(dir) - bodySettings.SegmentVertCount);

            if (dirCount > 1)
            {
                AddEnd(dir, CountVertices(dir) - 1, true);
            }
            else
            {
                RemoveSegment(-dir, 0);
                AddEndSegment(dir);
            }

            Setup();
        }
    }
    private void RemoveSegment(int dir, int startVertex)
    {
        vertices.RemoveRange(startVertex, bodySettings.SegmentVertCount);

        int lastIndex = 0;
        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i] == dir)
            {
                lastIndex = i;
            }
        }
        segments.RemoveAt(lastIndex);
    }
    private void RemoveEnd(int dir, int startVertex)
    {
        vertices.RemoveRange(startVertex, bodySettings.EndVertCount);
    }

    private void Setup()
    {
        mesh.Clear();

        SetupVertices();
        SetupTriangles();

        mesh.RecalculateNormals();

        // update bounds manually...
    }
    private void SetupVertices()
    {
        mesh.vertices = vertices.ToArray();
    }
    private void SetupTriangles()
    {
        triangles.Clear();

        Dictionary<int, List<int>> dirIndices = new Dictionary<int, List<int>>()
        {
            { -1, new List<int>() },
            { 1, new List<int>() }
        };
        for (int dirIndex = 0; dirIndex < segments.Count; dirIndex++)
        {
            dirIndices[segments[dirIndex]].Add(dirIndex);
        }

        foreach (int currentDir in dirIndices.Keys)
        {
            int vertIndex = 0;
            int lastIndex = dirIndices[currentDir][dirIndices[currentDir].Count - 1];

            for (int j = 0; j < dirIndices[currentDir].Count; j++)
            {
                int currentIndex = dirIndices[currentDir][j];
                vertIndex = CountVerticesToIndex(currentDir, currentIndex);

                for (int ringIndex = 0; ringIndex < bodySettings.Rings - 2; ringIndex++)
                {
                    int vertOffset = 0;
                    if (ringIndex == bodySettings.Rings - 3 && currentIndex < lastIndex)
                    {
                        vertOffset = CountVerticesToIndex(currentDir, dirIndices[currentDir][j + 1]) - vertIndex - 1 * bodySettings.Segments;
                    }

                    for (int i = 0; i < bodySettings.Segments; i++)
                    {
                        int seamOffset = i != bodySettings.Segments - 1 ? 0 : bodySettings.Segments;

                        triangles.Add(vertIndex + i);
                        triangles.Add(vertIndex + i + 1 - seamOffset);
                        triangles.Add(vertIndex + i + (bodySettings.Segments + vertOffset) + 1 - seamOffset);

                        triangles.Add(vertIndex + i + (bodySettings.Segments + vertOffset) + 1 - seamOffset);
                        triangles.Add(vertIndex + i + (bodySettings.Segments + vertOffset));
                        triangles.Add(vertIndex + i);
                    }

                    vertIndex += bodySettings.Segments;
                }
            }

            int topVertIndex = CountVertices(currentDir) + bodySettings.EndVertCount - 1;
            for (int i = 0; i < bodySettings.Segments; i++)
            {
                int seamOffset = i != bodySettings.Segments - 1 ? 0 : bodySettings.Segments;

                triangles.Add(topVertIndex);
                triangles.Add(topVertIndex - i - 2 + seamOffset);
                triangles.Add(topVertIndex - i - 1);
            }
        }

        mesh.triangles = triangles.ToArray();
    }

    private int CountVertices(int dir)
    {
        int lastIndexEnd = 0;
        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i] == dir)
            {
                lastIndexEnd = i;
            }
        }

        return CountVerticesToIndex(dir, lastIndexEnd) + bodySettings.SegmentVertCount;
    }
    private int CountVerticesToIndex(int dir, int index)
    {
        int lastIndexOppEnd = 0;
        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i] == -dir)
            {
                lastIndexOppEnd = i;
            }
        }

        int vertIndex = index * bodySettings.SegmentVertCount;
        if (index > lastIndexOppEnd)
        {
            vertIndex += bodySettings.EndVertCount;
        }

        return vertIndex;
    }

    public void AddToFront()
    {
        AddEndSegment(1);
        DisplayVertices(0f);
    }
    public void AddToBack()
    {
        AddEndSegment(-1);
        DisplayVertices(0f);
    }
    public void RemoveFromFront()
    {
        RemoveEndSegment(1);
        DisplayVertices(0f);
    }
    public void RemoveFromBack()
    {
        RemoveEndSegment(-1);
        DisplayVertices(0f);
    }

    #region Debugging
    private List<Vector3> gizmoVertices = new List<Vector3>();
    private Coroutine displayVerticesCoroutine;

    private void OnDrawGizmos()
    {
        if (gizmoVertices == null || gizmoVertices.Count == 0) { return; }

        foreach (Vector3 vertex in gizmoVertices)
        {
            Gizmos.DrawSphere(vertex + transform.position, 0.005f);
        }
    }

    [ContextMenu("Debug/Display")]
    public void DisplayVertices()
    {
        DisplayVertices(Application.isPlaying ? 0.025f : 0f);
    }
    private void DisplayVertices(float time)
    {
        HideVertices();
        displayVerticesCoroutine = StartCoroutine(DisplayVerticesRoutine(time));

        //Debug.Log(string.Format("[{0}]", string.Join(", ", segments.ToArray())));
    }
    private IEnumerator DisplayVerticesRoutine(float time)
    {
        foreach (Vector3 vertex in vertices)
        {
            gizmoVertices.Add(vertex);

            if (time > 0)
            {
                yield return new WaitForSeconds(time);
            }
        }
    }

    [ContextMenu("Debug/Hide")]
    private void HideVertices()
    {
        if (displayVerticesCoroutine != null)
        {
            StopCoroutine(displayVerticesCoroutine);
        }

        gizmoVertices.Clear();
    }

    [ContextMenu("Debug/Clear")]
    private void Clear()
    {
        vertices.Clear();
        triangles.Clear();

        HideVertices();
    }
    #endregion
    #endregion

    #region Inner Classes
    [Serializable]
    public class BodySettings
    {
        #region Fields
        [Header("General")]
        [SerializeField] private Vector2Int minMaxSegments = new Vector2Int(2, 10);

        [Header("Segment")]
        [SerializeField] private float radius = 0.25f;
        [SerializeField] private float length = 0.5f;
        [SerializeField] [Range(4, 100)] private int segments = 20;
        [SerializeField] [Range(2, 100)] private int rings = 20;
        #endregion

        #region Properties
        public Vector2Int MinMaxSegments { get { return minMaxSegments; } }

        public float Radius { get { return radius; } }
        public float Length { get { return length; } }
        public int Segments { get { return segments; } }
        public int Rings { get { return rings; } }

        public int SegmentVertCount
        {
            get
            {
                return segments * rings - segments;
            }
        }
        public int SegmentTriCount
        {
            get
            {
                return segments * (rings - 1) * 2 * 3;
            }
        }
        public int EndVertCount
        {
            get
            {
                return segments * (segments / 2 - 2) + (segments + 1) - segments;
            }
        }
        public int EndTriCount
        {
            get
            {
                return 3 * segments * (segments - 1);
            }
        }
        #endregion
    }
    #endregion
}