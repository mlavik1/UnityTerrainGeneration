using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public Material TerrainMaterial;
    public GameObject CameraObject;

    private const float mCellSize = 1000.0f;
    private const int mCellTextureRes = 256;
    private const float mTerrainHeight = 100.0f;

    private Dictionary<TerrainGridCell, TerrainCell> mTerrainCells = new Dictionary<TerrainGridCell, TerrainCell>();

	void Start ()
    {
        //GenerateObject(transform.position);
        //GenerateObject(transform.position + Vector3.right * mCellSize);
        //GenerateObject(transform.position + Vector3.forward * mCellSize);
    }

	void Update ()
    {
        if(CameraObject != null)
        {
            int camGridPosX = (int)Mathf.Round(CameraObject.transform.position.x / mCellSize);
            int camGridPosY = (int)Mathf.Round(CameraObject.transform.position.z / mCellSize);
            TerrainGridCell[] requiredCells = { new TerrainGridCell(camGridPosX, camGridPosY), new TerrainGridCell(camGridPosX - 1, camGridPosY), new TerrainGridCell(camGridPosX + 1, camGridPosY),
                                                new TerrainGridCell(camGridPosX, camGridPosY + 1), new TerrainGridCell(camGridPosX - 1, camGridPosY + 1), new TerrainGridCell(camGridPosX + 1, camGridPosY + 1),
                                                new TerrainGridCell(camGridPosX, camGridPosY - 1), new TerrainGridCell(camGridPosX - 1, camGridPosY - 1), new TerrainGridCell(camGridPosX + 1, camGridPosY - 1)};
            foreach(TerrainGridCell requiredCell in requiredCells)
            {
                if(!mTerrainCells.ContainsKey(requiredCell))
                {
                    mTerrainCells[requiredCell] = GenerateObject(new Vector3(requiredCell.X, 0.0f, requiredCell.Y) * mCellSize);
                }
            }
        }
    }

    private TerrainCell GenerateObject(Vector3 inPosition)
    {
        GameObject obj = new GameObject();
        obj.transform.position = inPosition;
        TerrainCell terrainCell = obj.AddComponent<TerrainCell>();

        terrainCell.mCellHeights = new float[mCellTextureRes, mCellTextureRes];

        for (int iX = 0; iX < mCellTextureRes; iX++)
        {
            for (int iY = 0; iY < mCellTextureRes; iY++)
            {
                float worldX = inPosition.x + (mCellSize * iX) / (mCellTextureRes - 1);
                float worldY = inPosition.z + (mCellSize * iY) / (mCellTextureRes - 1);
                float pXRidge = worldX * 2.0f / 1000.0f;
                float pYRidge = worldY * 2.0f / 1000.0f;
                float pXNoise = worldX * 10.0f / 1000.0f;
                float pYNoise = worldY * 10.0f / 1000.0f;
                float ridgeNoise = 1.0f - Mathf.Abs(0.5f - Mathf.PerlinNoise(pXRidge, pYRidge)) * 2.0f;
                float noise = Mathf.PerlinNoise(pXNoise, pYNoise);
                float perlin = ridgeNoise + (0.5f - noise) * 0.15f;
                terrainCell.mCellHeights[iX, iY] = perlin;
            }
        }

        MeshRenderer objMeshRenderer = obj.AddComponent<MeshRenderer>();
        objMeshRenderer.material = TerrainMaterial;
        objMeshRenderer.material.SetFloat("_MaxHeight", mTerrainHeight);

        GenerateMesh(terrainCell);
        return terrainCell;
    }

    private void GenerateMesh(TerrainCell inTerrainCell)
    {
        Vector3[] vertices = new Vector3[mCellTextureRes * mCellTextureRes];
        Vector3[] normals = new Vector3[mCellTextureRes * mCellTextureRes];
        Vector2[] texCoords = new Vector2[mCellTextureRes * mCellTextureRes];
        List<int> indices = new List<int>();

        for (int iX = 0; iX < mCellTextureRes; iX++)
        {
            for (int iY = 0; iY < mCellTextureRes; iY++)
            {
                float vpXWorld = ((float)iX / (mCellTextureRes - 1)) * mCellSize;
                float vpYWorld = ((float)iY / (mCellTextureRes - 1)) * mCellSize;

                Vector3 vertexPos = new Vector3(vpXWorld, inTerrainCell.mCellHeights[iX, iY] * mTerrainHeight, vpYWorld);
                vertices[iX + iY * mCellTextureRes] = vertexPos;

                Vector2 vertexUV = new Vector2((float)iX / mCellTextureRes, (float)iY / mCellTextureRes);
                texCoords[iX + iY * mCellTextureRes] = vertexUV;

                float hC = inTerrainCell.mCellHeights[iX, iY];
                float hF = inTerrainCell.mCellHeights[iX, System.Math.Min(iY + 1, mCellTextureRes - 1)];
                float hB = inTerrainCell.mCellHeights[iX, System.Math.Max(iY - 1, 0)];
                float hR = inTerrainCell.mCellHeights[System.Math.Min(iX + 1, mCellTextureRes - 1), iY];
                float hL = inTerrainCell.mCellHeights[System.Math.Max(iX - 1, 0), iY];
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

        MeshFilter meshFilter = inTerrainCell.gameObject.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        mesh.SetVertices(new List<Vector3>(vertices));
        mesh.SetNormals(new List<Vector3>(normals));
        mesh.SetUVs(0, new List<Vector2>(texCoords));
        mesh.SetTriangles(indices, 0);
        meshFilter.mesh = mesh;

        Rigidbody rb = inTerrainCell.gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;

        MeshCollider meshCollider = inTerrainCell.gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
    }
}
