using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static NoiseMapGeneration;

[Serializable]
public class TerrainType
{
    public string name;
    public int index;
    public float threshold;
    public Color color;
    public Texture texture;
}

public class TerrainGeneration : MonoBehaviour
{
    [SerializeField]
    NoiseMapGeneration noiseMapGeneration;

    [SerializeField]
    private Wave[] waves;

    [SerializeField]
    private TerrainType[] heightTerrainTypes;

    [SerializeField]
    private VisualizationMode visualizationMode;
    enum VisualizationMode { Height, Game }

    public Terrain GenerateTile()
    {
        var terrainTile = this.GetComponent<Terrain>();
        var terrainData = new TerrainData();
        var terrainCollider = terrainTile.GetComponent<TerrainCollider>();

        int size = terrainTile.terrainData.heightmapResolution;
        terrainData.heightmapResolution = size;
        terrainData.baseMapResolution = 100;
        terrainData.SetDetailResolution(100, 32);

        float offsetX = this.gameObject.transform.position.x;
        float offsetZ = this.gameObject.transform.position.z;
        var heightMap = this.noiseMapGeneration.GeneratePerlinNoiseMap(size, size, size, offsetX, offsetZ, 77, waves);
        terrainData.SetHeights(0, 0, heightMap);

        terrainData.size = new Vector3(terrainData.heightmapResolution, 100, terrainData.heightmapResolution);

        terrainData.terrainLayers = terrainTile.terrainData.terrainLayers;
        terrainData.alphamapResolution = size * 4;

        var chosenHeightTerrainTypes = new TerrainType[size, size];
        var heightLayer = BuildTexture(heightMap, this.heightTerrainTypes, chosenHeightTerrainTypes);
        var chosenGameTerrainTypes = new TerrainType[size, size];
        var gameLayer = BuildGameTexture(heightMap, this.heightTerrainTypes, chosenGameTerrainTypes, terrainData);

        var terrainLayers = new TerrainLayer[1];

        switch (this.visualizationMode)
        {
            case VisualizationMode.Height:
                terrainLayers[0] = heightLayer;
                terrainData.terrainLayers = terrainLayers;
                break;
            case VisualizationMode.Game:
                terrainData.SetAlphamaps(0, 0, gameLayer); 
                break;
            default:
                break;
        }

        terrainTile.terrainData = terrainData;
        terrainCollider.terrainData = terrainData;

        return terrainTile;
    }

    private TerrainLayer BuildTexture(float[,] heightMap, TerrainType[] terrainTypes, TerrainType[,] chosenTerrainTypes)
    {
        var terrainLayer = new TerrainLayer();

        int tileDepth = heightMap.GetLength(0);
        int tileWidth = heightMap.GetLength(1);

        terrainLayer.tileSize = new Vector2(tileDepth, tileWidth);

        var colorMap = new Color[tileDepth * tileWidth];
        for (int zIndex = 0; zIndex < tileDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < tileWidth; xIndex++)
            {
                int colorIndex = zIndex * tileWidth + xIndex;
                float height = heightMap[zIndex, xIndex];
                var terrainType = ChooseTerrainType(height, terrainTypes);
                colorMap[colorIndex] = terrainType.color;
                chosenTerrainTypes[zIndex, xIndex] = terrainType;
            }
        }

        var tileTexture = new Texture2D(tileWidth, tileDepth);
        tileTexture.wrapMode = TextureWrapMode.Clamp;
        tileTexture.SetPixels(colorMap);
        tileTexture.Apply();

        terrainLayer.diffuseTexture = tileTexture;

        return terrainLayer;
    }

    private float[,,] BuildGameTexture(float[,] heightMap, TerrainType[] terrainTypes, TerrainType[,] chosenTerrainTypes, TerrainData terrainData)
    {
        int tileDepth = heightMap.GetLength(0);
        int tileWidth = heightMap.GetLength(1);

        var map = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);

        for (int zIndex = 0; zIndex < tileDepth * 4; zIndex++)
        {
            for (int xIndex = 0; xIndex < tileWidth * 4; xIndex++)
            {
                float height = heightMap[(int)Mathf.Floor(zIndex / 4), (int)Mathf.Floor(xIndex / 4)];
                var terrainType = ChooseTerrainType(height, terrainTypes);
                for (int i = 0; i < 3; i++)
                {
                    if (terrainType.index == i)
                        map[zIndex, xIndex, i] = 1.0f;
                    else
                        map[zIndex, xIndex, i] = 0;
                }

                chosenTerrainTypes[(int)Mathf.Floor(zIndex / 4), (int)Mathf.Floor(xIndex / 4)] = terrainType;
            }
        }

        return map;
    }


    private TerrainType ChooseTerrainType(float height, TerrainType[] terrainTypes)
    {
        foreach (TerrainType terrainType in terrainTypes)
        {
            if (height < terrainType.threshold)
                return terrainType;
        }
        return terrainTypes[terrainTypes.Length - 1];
    }

    public static void Fix(Terrain cur, Terrain leftTerrain, Terrain bottomTerrain)
    {
        int resolution = cur.terrainData.heightmapResolution;

        float[,] newHeights = new float[resolution, resolution];
        float[,] leftHeights = new float[resolution, resolution], bottomHeights = new float[resolution, resolution];

        if (leftTerrain != null)
            leftHeights = leftTerrain.terrainData.GetHeights(0, 0, resolution, resolution);
        if (bottomTerrain != null)
            bottomHeights = bottomTerrain.terrainData.GetHeights(0, 0, resolution, resolution);

        if (leftTerrain != null || bottomTerrain != null)
        {
            newHeights = cur.terrainData.GetHeights(0, 0, resolution, resolution);

            for (int i = 0; i < resolution; i++)
            {
                if (leftTerrain != null)
                    newHeights[i, 0] = leftHeights[i, resolution - 1];
                if (bottomTerrain != null)
                    newHeights[0, i] = bottomHeights[resolution - 1, i];
            }
            cur.terrainData.SetHeights(0, 0, newHeights);
        }
    }
}
