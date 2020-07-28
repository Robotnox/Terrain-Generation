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
    [Tooltip("Currently matching terrain layer indexes")]
    public int index;
    [Tooltip("Must be between 0 to 1")]
    public float threshold;
    public Color color;
    public Texture texture;
}

public class TerrainGeneration : MonoBehaviour
{
    [SerializeField]
    NoiseMapGeneration noiseMapGeneration;

    [SerializeField]
    [Tooltip("Seed allow create different world")]
    private int seed;

    [SerializeField]
    [Tooltip("Waves influence how the noisemap is created")]
    private Wave[] waves;

    [SerializeField]
    [Tooltip("Threshold must be in order starting from lowest to highest")]
    private TerrainType[] heightTerrainTypes;

    [SerializeField]
    [Tooltip("Game mode for offical play while others mode are for debugging")]
    private VisualizationMode visualizationMode;
    enum VisualizationMode { Height, Heat, Moisture, Game }

    /*
     * Generate a terrain by getting a noisemap and adjust the height.
     * Then create and apply texture to the terrain layer depends on the visualization mode.
     * TODO : Add heat and moisture maps
     */
    public TileData GenerateTile()
    {
        var terrainTile = this.GetComponent<Terrain>();
        var terrainData = new TerrainData();
        var terrainCollider = terrainTile.GetComponent<TerrainCollider>();

        int size = terrainTile.terrainData.heightmapResolution;
        terrainData.heightmapResolution = size;
        terrainData.baseMapResolution = 100;
        terrainData.SetDetailResolution(100, 32);
        terrainData.size = new Vector3(size, 100, size);

        // generate height noisemap and add hills to the terrain
        float offsetX = this.gameObject.transform.position.x;
        float offsetZ = this.gameObject.transform.position.z;
        var heightMap = this.noiseMapGeneration.GeneratePerlinNoiseMap(size, offsetX, offsetZ, seed, waves);
        terrainData.SetHeights(0, 0, heightMap);

        // Setup textures and trees setting
        terrainData.terrainLayers = terrainTile.terrainData.terrainLayers;
        terrainData.alphamapResolution = size * 4;
        terrainData.treePrototypes = terrainTile.terrainData.treePrototypes;

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

        return new TileData(heightMap, null, null, chosenHeightTerrainTypes, null, null, null, null, terrainTile);
    }

    /*
     * Generate a Texture2D to apply on the terrain layer.
     * <param name="noiseMap">Noise map that will be used to get data of the terrain</param>
     * <param name="terrainTypes">Data to compare against the noise map</param>
     * <param name="chosenTerrainTypes">Infomation of each vertice in the terrain</param>
     */
    private TerrainLayer BuildTexture(float[,] noiseMap, TerrainType[] terrainTypes, TerrainType[,] chosenTerrainTypes)
    {
        var terrainLayer = new TerrainLayer();

        int tileDepth = noiseMap.GetLength(0);
        int tileWidth = noiseMap.GetLength(1);

        terrainLayer.tileSize = new Vector2(tileDepth, tileWidth);

        var colorMap = new Color[tileDepth * tileWidth];
        for (int zIndex = 0; zIndex < tileDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < tileWidth; xIndex++)
            {
                int colorIndex = zIndex * tileWidth + xIndex;
                float noise = noiseMap[zIndex, xIndex];
                var terrainType = ChooseTerrainType(noise, terrainTypes);
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

    /*
     * Generate texture to apply on the terrain alphamap.
     * TODO : Rewrite so muliple textures blend into each other
     */
    private float[,,] BuildGameTexture(float[,] noiseMap, TerrainType[] terrainTypes, TerrainType[,] chosenTerrainTypes, TerrainData terrainData)
    {
        var map = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);

        for (int zIndex = 0; zIndex < terrainData.alphamapHeight; zIndex++)
        {
            for (int xIndex = 0; xIndex < terrainData.alphamapWidth; xIndex++)
            {
                float noise = noiseMap[(int)Mathf.Floor(zIndex / 4), (int)Mathf.Floor(xIndex / 4)];
                var terrainType = ChooseTerrainType(noise, terrainTypes);
                for (int i = 0; i < 4; i++)
                {
                    // i is a texture index in terrain layers that will be applied on the terrain coords 
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

    /*
     * Compare the noise to the terrain types and return the terrain type that is the closest to the threshold
     */
    private TerrainType ChooseTerrainType(float noise, TerrainType[] terrainTypes)
    {
        foreach (TerrainType terrainType in terrainTypes)
        {
            if (noise < terrainType.threshold)
                return terrainType;
        }
        return terrainTypes[terrainTypes.Length - 1];
    }

    /*
     * Snitch up terrains so the border are joined up
     * TODO : Need rewriting to snitch up current terrain to any existing neighbour terrains that haven't been snitched up and smooth it out
     */
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
