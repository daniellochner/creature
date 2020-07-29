using UnityEngine;

public class CapMeshGenerator : MeshGenerator
{
    #region Fields
    [SerializeField] private float radius = 0.25f;
    [SerializeField] [Range(4, 100)] private int segments = 10;
    #endregion

    #region Methods
    public override void ConstructMesh()
    {
        vertices = new Vector3[segments * (segments / 2 - 2) + (segments + 1)];
        triangles = new int[3 * segments * (segments - 1)];

        int vertIndex = 0, triIndex = 0;

        #region Vertices
        vertices[vertIndex++] = Vector3.up * radius;
        for (int ringIndex = segments / 2 - 2; ringIndex >= 0; ringIndex--)
        {
            float percent = ringIndex / (segments / 2 - 1f);
            float ringRadius = Mathf.Cos(90f * percent * Mathf.Deg2Rad) * radius;
            float ringHeight = Mathf.Sin(90f * percent * Mathf.Deg2Rad) * radius;

            for (int i = 1; i < segments + 1; i++, vertIndex++)
            {
                float angle = i * 360f / segments;

                float x = ringRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
                float y = ringHeight;
                float z = ringRadius * Mathf.Sin(angle * Mathf.Deg2Rad);

                vertices[vertIndex] = new Vector3(x, y, z);
            }
        }
        #endregion

        #region Triangles
        // Top.
        for (int i = 0; i < segments; i++)
        {
            triangles[triIndex + 0] = 0;
            triangles[triIndex + 1] = i + 2 - (i != segments - 1 ? 0 : segments); // Prevents the need to use a seam.
            triangles[triIndex + 2] = i + 1;

            triIndex += 3;
        }

        // Main.
        for (int ringIndex = 0; ringIndex < segments / 2 - 2; ringIndex++)
        {
            int startRingIndex = 1 + (ringIndex * segments);
            for (int i = 0; i < segments; i++)
            {
                triangles[triIndex + 0] = startRingIndex + i + 1 + segments - (i != segments - 1 ? 0 : segments); // '';
                triangles[triIndex + 1] = startRingIndex + i + segments;
                triangles[triIndex + 2] = startRingIndex + i;

                triangles[triIndex + 3] = startRingIndex + i;
                triangles[triIndex + 4] = startRingIndex + i + 1 - (i != segments - 1 ? 0 : segments); // '';
                triangles[triIndex + 5] = startRingIndex + i + 1 + segments - (i != segments - 1 ? 0 : segments); // '';

                triIndex += 6;
            }
        }
        #endregion

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
    #endregion
}
