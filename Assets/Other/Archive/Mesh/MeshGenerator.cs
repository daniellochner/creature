using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public abstract class MeshGenerator : MonoBehaviour
{
    #region Fields
    protected Mesh mesh;
    protected Vector3[] vertices;
    protected int[] triangles;

    private List<Vector3> tempVertices;
    #endregion

    #region Methods
    private void OnValidate()
    {
        if (mesh == null)
        {
            MeshFilter segmentMeshFilter = GetComponent<MeshFilter>();
            GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
            mesh = segmentMeshFilter.sharedMesh = new Mesh();
            mesh.name = gameObject.name;
        }

        ConstructMesh();
    }
    private void OnDrawGizmos()
    {
        if (tempVertices == null) return;

        foreach (Vector3 vertex in tempVertices)
        {
            Gizmos.DrawSphere(vertex + transform.position, 0.0025f);
        }
    }

    public abstract void ConstructMesh();

    [ContextMenu("Display Vertices")]
    public void DisplayVertices()
    {
        tempVertices = new List<Vector3>();

        StartCoroutine(DisplayVerticesRoutine(0.05f));
    }
    private IEnumerator DisplayVerticesRoutine(float time)
    {
        foreach (Vector3 vertex in vertices)
        {
            tempVertices.Add(vertex);
            yield return new WaitForSeconds(time);
        }
    }
    #endregion
}
