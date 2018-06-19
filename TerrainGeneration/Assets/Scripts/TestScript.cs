using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    private const float mCellSize = 1000.0f;
    private const int mCellTextureRes = 256;
    private const float mTerrainHeight = 100.0f;

    private float[,] mCellHeights;

	// Use this for initialization
	void Start ()
    {
        mCellHeights = new float[mCellTextureRes, mCellTextureRes];
        Texture2D PreviewTexture = new Texture2D(mCellTextureRes, mCellTextureRes, TextureFormat.ARGB32, false);

        for (int iX = 0; iX < mCellTextureRes; iX++)
        {
            for(int iY = 0; iY < mCellTextureRes; iY++)
            {
                float pXRidge = (float)iX / mCellTextureRes * 2.0f;
                float pYRidge = (float)iY / mCellTextureRes * 2.0f;
                float pXNoise = (float)iX / mCellTextureRes * 10.0f;
                float pYNoise = (float)iY / mCellTextureRes * 10.0f;
                float ridgeNoise = 1.0f - Mathf.Abs(0.5f - Mathf.PerlinNoise(pXRidge, pYRidge)) * 2.0f;
                float noise = Mathf.PerlinNoise(pXNoise, pYNoise);
                float perlin = ridgeNoise + (0.5f - noise) * 0.15f;
                mCellHeights[iX, iY] = perlin;
                PreviewTexture.SetPixel(iX, iY, new Color(perlin, perlin, perlin));
                //PreviewTexture.SetPixel(iX, iY, new Color((float)iX / PreviewTexture.width, (float)iY / PreviewTexture.height,0));
            }
        }

        PreviewTexture.Apply();
        GetComponent<MeshRenderer>().materials[0].SetTexture("_MainTex", PreviewTexture);
        GetComponent<MeshRenderer>().materials[0].SetFloat("_MaxHeight", mTerrainHeight);

        GenerateMesh();
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    private void GenerateMesh()
    {
        Vector3[] vertices = new Vector3[mCellTextureRes * mCellTextureRes];
        Vector3[] normals = new Vector3[mCellTextureRes * mCellTextureRes];
        Vector2[] texCoords = new Vector2[mCellTextureRes * mCellTextureRes];
        List<int> indices = new List<int>();

        for (int iX = 0; iX < mCellTextureRes; iX++)
        {
            for (int iY = 0; iY < mCellTextureRes; iY++)
            {
                float vpXWorld = ((float)iX / mCellTextureRes) * mCellSize;
                float vpYWorld = ((float)iY / mCellTextureRes) * mCellSize;

                Vector3 vertexPos = new Vector3(vpXWorld, mCellHeights[iX, iY] * mTerrainHeight, vpYWorld);
                vertices[iX + iY * mCellTextureRes] = vertexPos;

                Vector2 vertexUV = new Vector2((float)iX / mCellTextureRes, (float)iY / mCellTextureRes);
                texCoords[iX + iY * mCellTextureRes] = vertexUV;

                float hC = mCellHeights[iX, iY];
                float hF = mCellHeights[iX, System.Math.Min(iY + 1, mCellTextureRes - 1)];
                float hB = mCellHeights[iX, System.Math.Max(iY - 1, 0)];
                float hR = mCellHeights[System.Math.Min(iX + 1, mCellTextureRes - 1), iY];
                float hL = mCellHeights[System.Math.Max(iX - 1, 0), iY];
                float dX = (hC - hL) + (hR - hC);
                float dY = (hC - hB) + (hF - hC);
                Vector3 xSlope = new Vector3(1.0f, dX * mTerrainHeight, 0.0f);
                Vector3 ySlope = new Vector3(0.0f, dY * mTerrainHeight, 1.0f);
                Vector3 vertexNormal = Vector3.Cross(ySlope, xSlope).normalized;
                normals[iX + iY * mCellTextureRes] = vertexNormal;

                if (iX > 0 && iY > 0)
                {
                    int i0 = (iX - 1) + (iY - 1) * mCellTextureRes;
                    int i1 = (iX - 1) + iY * mCellTextureRes;
                    int i2 = iX + iY * mCellTextureRes;
                    int i3 = iX + (iY - 1) * mCellTextureRes;
                    indices.Add(i0);
                    indices.Add(i1);
                    indices.Add(i2);
                    indices.Add(i0);
                    indices.Add(i2);
                    indices.Add(i3);
                }
            }
        }

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        mesh.SetVertices(new List<Vector3>(vertices));
        mesh.SetNormals(new List<Vector3>(normals));
        mesh.SetUVs(0, new List<Vector2>(texCoords));
        mesh.SetTriangles(indices, 0);
        meshFilter.mesh = mesh;
    }
}
