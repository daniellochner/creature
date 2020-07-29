//------------------------------//
//  ProceduralCapsule.cs        //
//  Written by Jay Kay          //
//  2016/05/27                  //
//------------------------------//


using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent( typeof(MeshFilter), typeof(MeshRenderer) )]
public class ProceduralCapsule : MonoBehaviour 
{
	#if UNITY_EDITOR
	public void OnValidate()
	{
		GenerateMesh();
	}
    #endif

    Vector3[] vertices;

    public float height = 2f;
	public float radius = 0.5f;
	
	public int segments = 24;



    int points;
    float[] pX;
    float[] pZ;
    float[] pY;
    float[] pR;


    void GenerateMesh() 
	{
        // make segments an even number
        if (segments % 2 != 0)
            segments++;

        // extra vertex on the seam
        points = segments + 1;
		
		// calculate points around a circle
		float[] pX = new float[ points ];
		float[] pZ = new float[ points ];
		float[] pY = new float[ points ];
		float[] pR = new float[ points ];
		
		float calcH = 0f;
		float calcV = 0f;
		
		for ( int i = 0; i < points; i ++ )
		{
			pX[ i ] = Mathf.Sin( calcH * Mathf.Deg2Rad ); 
			pZ[ i ] = Mathf.Cos( calcH * Mathf.Deg2Rad );
			pY[ i ] = Mathf.Cos( calcV * Mathf.Deg2Rad ); 
			pR[ i ] = Mathf.Sin( calcV * Mathf.Deg2Rad ); 
			
			calcH += 360f / (float)segments;
			calcV += 180f / (float)segments;
		}
		
		
		// - Vertices and UVs -
		//
		vertices = new Vector3[ points * ( points + 1 ) ];
		Vector2[] uvs = new Vector2[ vertices.Length ];
		int ind = 0;
		
		// Y-offset is half the height minus the diameter
		float yOff = ( height - ( radius * 2f ) ) * 0.5f;
		if ( yOff < 0 )
			yOff = 0;
		
		// uv calculations
		float stepX = 1f / ( (float)(points - 1) );
		float uvX, uvY;
		
		// Top Hemisphere
		int top = Mathf.CeilToInt( (float)points * 0.5f );
		
		for ( int y = 0; y < top; y ++ ) 
		{
			for ( int x = 0; x < points; x ++ ) 
			{
				vertices[ ind ] = new Vector3( pX[ x ] * pR[ y ], pY[ y ], pZ[ x ] * pR[ y ] ) * radius;
				vertices[ ind ].y = yOff + vertices[ ind ].y;
				
				uvX = 1f - ( stepX * (float)x );
				uvY = ( vertices[ ind ].y + ( height * 0.5f ) ) / height;
				uvs[ ind ] = new Vector2( uvX, uvY );
				
				ind ++;
			}
		}

        // Bottom Hemisphere
        int btm = Mathf.FloorToInt((float)points * 0.5f);

        for (int y = btm; y < points; y++)
        {
            for (int x = 0; x < points; x++)
            {
                vertices[ind] = new Vector3(pX[x] * pR[y], pY[y], pZ[x] * pR[y]) * radius;
                vertices[ind].y = -yOff + vertices[ind].y;

                uvX = 1f - (stepX * (float)x);
                uvY = (vertices[ind].y + (height * 0.5f)) / height;
                uvs[ind] = new Vector2(uvX, uvY);

                ind++;
            }
        }


        // - Triangles -

        int[] triangles = new int[ ( segments * (segments + 1) * 2 * 3 ) ];
		
		for ( int y = 0, t = 0; y < segments + 1; y ++ ) 
		{
			for ( int x = 0; x < segments; x ++, t += 6 ) 
			{
				triangles[ t + 0 ] = ( (y + 0) * ( segments + 1 ) ) + x + 0;
				triangles[ t + 1 ] = ( (y + 1) * ( segments + 1 ) ) + x + 0;
				triangles[ t + 2 ] = ( (y + 1) * ( segments + 1 ) ) + x + 1;
				
				triangles[ t + 3 ] = ( (y + 0) * ( segments + 1 ) ) + x + 1;
				triangles[ t + 4 ] = ( (y + 0) * ( segments + 1 ) ) + x + 0;
				triangles[ t + 5 ] = ( (y + 1) * ( segments + 1 ) ) + x + 1;
			}
		}


        // - Assign Mesh -

        MeshFilter mf = gameObject.GetComponent<MeshFilter>();
        Mesh mesh = mf.sharedMesh;
        if (!mesh)
        {
            mesh = new Mesh();
            mf.sharedMesh = mesh;
        }
        mesh.Clear();

        mesh.name = "ProceduralCapsule";

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        //mesh.Optimize();
    }

    private void OnDrawGizmos()
    {
        if (gizmoVertices == null) { return; }

        foreach (Vector3 vertex in gizmoVertices)
        {
            Gizmos.DrawSphere(vertex + transform.position, 0.0025f);
        }
    }

    [ContextMenu("Debug/Display Vertices")]
    public void DisplayVertices()
    {
        HideVertices();
        StartCoroutine(DisplayVerticesRoutine(Application.isPlaying ? 0.05f : 0f));
    }
    private IEnumerator DisplayVerticesRoutine(float time)
    {
        foreach (Vector3 vertex in vertices)
        {
            gizmoVertices.Add(vertex);

            if (time > 0)
                yield return new WaitForSeconds(time);
        }
    }

    [ContextMenu("Debug/Hide Vertices")]
    private void HideVertices()
    {
        gizmoVertices.Clear();
    }
    private List<Vector3> gizmoVertices = new List<Vector3>();

}
