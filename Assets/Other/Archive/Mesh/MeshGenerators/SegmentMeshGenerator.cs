using UnityEngine;

public class SegmentMeshGenerator : MeshGenerator
{
    #region Fields
    [SerializeField] private float radius = 0.25f;
    [SerializeField] private float height = 0.5f;
    [SerializeField] [Range(3, 100)] private int segments = 10;
    [SerializeField] [Range(2, 100)] private int rings = 10;
    #endregion

    #region Methods
    public override void ConstructMesh()
    {
        vertices = new Vector3[segments * rings];
        triangles = new int[segments * (rings - 1) * 2 * 3];

        int vertIndex = 0, triIndex = 0;

        for (int ringIndex = 0; ringIndex < rings; ringIndex++)
        {
            for (int i = 0; i < segments; i++, vertIndex++)
            {
                #region Vertices
                float angle = i * 360f / segments;

                float x = radius * Mathf.Cos(angle * Mathf.Deg2Rad);
                float y = (ringIndex * height / (rings - 1)) - height / 2f;
                float z = radius * Mathf.Sin(angle * Mathf.Deg2Rad);

                vertices[vertIndex] = new Vector3(x, y, z);
                #endregion

                #region Triangles
                if (ringIndex < rings - 1)
                {
                    triangles[triIndex + 0] = ringIndex * segments + i + 1 - (i != segments - 1 ? 0 : segments);
                    triangles[triIndex + 1] = ringIndex * segments + i;
                    triangles[triIndex + 2] = ringIndex * segments + i + segments;

                    triangles[triIndex + 3] = ringIndex * segments + i + segments;
                    triangles[triIndex + 4] = ringIndex * segments + i + segments + 1 - (i != segments - 1 ? 0 : segments);
                    triangles[triIndex + 5] = ringIndex * segments + i + 1 - (i != segments - 1 ? 0 : segments);

                    triIndex += 6;
                }
                #endregion
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
    #endregion
}
