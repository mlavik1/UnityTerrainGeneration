﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public Material TerrainMaterial;
    public GameObject CameraObject;
    public GameObject TreeAsset;

    private const float mCellSize = 1000.0f;
    private const int mCellTextureRes = 15;
    private const float mTerrainHeight = 3500.0f;

    private Dictionary<TerrainGridCell, TerrainCell> mTerrainCells = new Dictionary<TerrainGridCell, TerrainCell>();

	void Start ()
    {
    }

	void Update ()
    {
        if(CameraObject != null)
        {
            const int viewRange = 20;
            int camGridPosX = (int)Mathf.Round(CameraObject.transform.position.x / mCellSize);
            int camGridPosY = (int)Mathf.Round(CameraObject.transform.position.z / mCellSize);
            List<TerrainGridCell> requiredCells = new List<TerrainGridCell>();
            for(int iX = -viewRange; iX <= viewRange; iX ++)
            {
                for (int iY = -viewRange; iY <= viewRange; iY++)
                {
                    requiredCells.Add(new TerrainGridCell(camGridPosX + iX, camGridPosY + iY));
                }
            }
            foreach (TerrainGridCell requiredCell in requiredCells)
            {
                if(!mTerrainCells.ContainsKey(requiredCell))
                {
                    mTerrainCells[requiredCell] = GenerateObject(new Vector3(requiredCell.X, 0.0f, requiredCell.Y) * mCellSize);
                }
            }
        }
    }

    private float fbm(float inX, float inY, float seed, int octaves, float lacunarity, float gain)
    {
        float sum = 0;
        float freq = 1.0f;
        float amp = 1.0f;
        for(int i = 0; i < octaves; i++)
        {
            float n = Mathf.PerlinNoise(inX * freq, inY * freq);
            sum += n * amp;
            freq *= lacunarity;
            amp *= gain;
        }
        return sum;
    }

    private TerrainCell GenerateObject(Vector3 inPosition)
    {
        GameObject obj = new GameObject();
        obj.transform.position = inPosition;
        TerrainCell terrainCell = obj.AddComponent<TerrainCell>();

        terrainCell.mCellHeights = new float[mCellTextureRes + 2, mCellTextureRes + 2];
        //terrainCell.mVegetation= new int[mCellTextureRes, mCellTextureRes];

        for (int iX = -1; iX < mCellTextureRes + 1; iX++)
        {
            for (int iY = -1; iY < mCellTextureRes + 1; iY++)
            {
                float worldX = inPosition.x + (mCellSize * iX) / (mCellTextureRes - 1);
                float worldY = inPosition.z + (mCellSize * iY) / (mCellTextureRes - 1);
                float pXArea = worldX * 0.00002f;
                float pYArea = worldY * 0.00002f;

                float areaNoise = Mathf.PerlinNoise(pXArea, pYArea);

                float valleyNoise = GenerateTurbulentWave(4.0f, worldX, worldY, 0.0003f, 1400.0f);         
                float smallValleyNoise = GenerateTurbulentWave(4.0f, worldX, worldY, 000.1f, 200.0f);
                float fbmNoise = fbm(worldX * 0.0001f, worldY * 0.0001f, 0.0f, 8, 1.92f, 0.4f) * 2.0f - 1.0f;
                float fbmMountainNoise = fbm(worldX * 0.00003f, worldY * 0.00003f, 0.0f, 4, 1.92f, 0.5f) * 2.0f - 1.0f;

                float elevationRateNoise = GenerateTurbulentWave(4.0f, worldX, worldY, 0.00007f, 7000.0f);
                float elevationRate = Mathf.Clamp((0.5f - elevationRateNoise) * 2.0f + 0.8f, 0.0f, 1.0f);

                const float valleyHeight = 950.0f;
                const float areaHeight = 6000.0f;
                const float fbmHeight = 900.0f;
                const float fbmMountainHeight = 2500.0f;

                float perlin = fbmNoise * (fbmHeight / mTerrainHeight);
                perlin += valleyNoise * (valleyHeight / mTerrainHeight) * elevationRateNoise;
                perlin += (0.8f - fbmMountainNoise) * (fbmMountainHeight / mTerrainHeight);
                //perlin *= elevationRate;
                perlin += (0.6f - areaNoise) * (areaHeight / mTerrainHeight);

                terrainCell.mCellHeights[iX + 1, iY + 1] = perlin;          

                //terrainCell.mVegetation[iX, iY] = Mathf.PerlinNoise(pXArea, pYArea) > 0.6f ? 1 : 0;
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

                float hC = inTerrainCell.mCellHeights[1 + iX, 1 + iY];
                float hF = inTerrainCell.mCellHeights[1 + iX, 1 + iY + 1];
                float hB = inTerrainCell.mCellHeights[1 + iX, 1 + iY - 1];
                float hR = inTerrainCell.mCellHeights[1 + iX + 1, 1 + iY];
                float hL = inTerrainCell.mCellHeights[1 + iX - 1, 1 + iY];
                float dX = (hC - hL) + (hR - hC);
                float dY = (hC - hB) + (hF - hC);
                Vector3 xSlope = new Vector3(2.0f * mCellSize / (mCellTextureRes - 1), dX * mTerrainHeight, 0.0f);
                Vector3 ySlope = new Vector3(0.0f, dY * mTerrainHeight, 2.0f *mCellSize / (mCellTextureRes - 1));
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

    private float GenerateTurbulentWave(float inTurbPower, float inX, float inY, float inNoseFactor, float inPeriod)
    {
        float xyValue = Mathf.PerlinNoise(inX * inNoseFactor, inY * inNoseFactor) * inTurbPower + (inX + inY) / inPeriod;
        return (Mathf.Sin(xyValue) + 1.0f) / 2.0f;
    }
}
