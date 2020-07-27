using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static NoiseMapGeneration;

//[Serializable]
//public class TerrainType
//{
//    public string name;
//    public int index;
//    public float threshold;
//    public Color color;
//}

[Serializable]
public class Biome
{
    public string name;
    public Color color;
    public Texture texture;
}

[Serializable]
public class BiomeRow
{
    public Biome[] biomes;
}

public class TileGeneration : MonoBehaviour
{

    [SerializeField]
    NoiseMapGeneration noiseMapGeneration;

    [SerializeField]
    private MeshRenderer tileRenderer;
    [SerializeField]
    private MeshFilter meshFilter;
    [SerializeField]
    private MeshCollider meshCollider;

    [SerializeField]
    private Wave[] waves;

    [SerializeField]
    private float mapScale;

    [SerializeField]
    private float heightMultiplier;

    [SerializeField]
    private BiomeRow[] biomes;

    [SerializeField]
    private AnimationCurve heightCurve;
    [SerializeField]
    private AnimationCurve heatCurve;
    [SerializeField]
    private AnimationCurve moistureCurve;

    [SerializeField]
    private TerrainType[] heightTerrainTypes;
    [SerializeField]
    private TerrainType[] heatTerrainTypes;
    [SerializeField]
    private TerrainType[] moistureTerrainTypes;

    [SerializeField]
    private Shader myShader;

    [SerializeField]
    private VisualizationMode visualizationMode;
    enum VisualizationMode { Height, Heat, Moisture, Biome, Game }

    void Start()
    {
        //GenerateTile(10, 1000);
    }

    //public void GenerateTile()
    //{
    //    var meshVertices = this.meshFilter.mesh.vertices;
    //    int tileDepth = (int)Mathf.Sqrt(meshVertices.Length);
    //    int tileWidth = tileDepth;

    //    float offsetX = -this.gameObject.transform.position.x;
    //    float offsetZ = -this.gameObject.transform.position.z;

    //    var str = GameObject.Find("SeedInputField").GetComponent<TMPro.TMP_InputField>().text;
    //    int seed; 
    //    int.TryParse(str, out seed);
    //    var heightMap = this.noiseMapGeneration.GeneratePerlinNoiseMap(tileDepth, tileWidth, this.mapScale, offsetX, offsetZ, seed, waves);

    //    var tileTexture = BuildTexture(heightMap, this.heightTerrainTypes);
    //    this.tileRenderer.material.mainTexture = tileTexture;
    //    UpdateMeshVertices(heightMap);
    //}

    public TileData GenerateTile(float centerVertexZ, float maxDistanceZ)
    {
        var meshVertices = this.meshFilter.mesh.vertices;
        int tileDepth = (int)Mathf.Sqrt(meshVertices.Length);
        int tileWidth = tileDepth;

        float offsetX = -this.gameObject.transform.position.x;
        float offsetZ = -this.gameObject.transform.position.z;

        var str = GameObject.Find("SeedInputField").GetComponent<TMPro.TMP_InputField>().text;
        int seed;
        int.TryParse(str, out seed);
        var heightMap = this.noiseMapGeneration.GeneratePerlinNoiseMap(tileDepth, tileWidth, this.mapScale, offsetX, offsetZ, seed, waves);

        var tileDimesions = this.meshFilter.mesh.bounds.size;
        float distanceBetweenVertices = tileDimesions.z / (float)tileDepth;
        float vertexOffsetZ = this.gameObject.transform.position.z / distanceBetweenVertices;

        var uniformHeatMap = this.noiseMapGeneration.GenerateUniformNoiseMap(tileDepth, tileWidth, centerVertexZ, maxDistanceZ, vertexOffsetZ);
        var randomHeatMap = this.noiseMapGeneration.GeneratePerlinNoiseMap(tileDepth, tileWidth, this.mapScale, offsetX, offsetZ, seed, waves);
        var heatMap = new float[tileDepth, tileWidth];
        for (int zIndex = 0; zIndex < tileDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < tileWidth; xIndex++)
            {
                heatMap[zIndex, xIndex] = uniformHeatMap[zIndex, xIndex] * randomHeatMap[zIndex, xIndex];
                heatMap[zIndex, xIndex] += this.heatCurve.Evaluate(heightMap[zIndex, xIndex]) * heightMap[zIndex, xIndex];
            }
        }

        var moistureMap = this.noiseMapGeneration.GeneratePerlinNoiseMap(tileDepth, tileWidth, this.mapScale, offsetX, offsetZ, seed, waves);
        for (int zIndex = 0; zIndex < tileDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < tileWidth; xIndex++)
            {
                moistureMap[zIndex, xIndex] -= this.moistureCurve.Evaluate(heightMap[zIndex, xIndex]) * heightMap[zIndex, xIndex];
            }
        }

        var chosenHeightTerrainTypes = new TerrainType[tileDepth, tileWidth];
        var heightTexture = BuildTexture(heightMap, this.heightTerrainTypes, chosenHeightTerrainTypes);
        var chosenHeatTerrainTypes = new TerrainType[tileDepth, tileWidth];
        var heatTexture = BuildTexture(heatMap, this.heatTerrainTypes, chosenHeatTerrainTypes);
        var chosenMoistureTerrainTypes = new TerrainType[tileDepth, tileWidth];
        var moistureTexture = BuildTexture(moistureMap, this.moistureTerrainTypes, chosenMoistureTerrainTypes);
        var chosenBiomes = new Biome[tileDepth, tileWidth];
        var biomeTexture = BuildBiomeTexture(chosenHeightTerrainTypes, chosenHeatTerrainTypes, chosenMoistureTerrainTypes, chosenBiomes);
        var chosenGameTerrainTypes = new Biome[tileDepth, tileWidth];
        var gameTexture = BuildGameTexture(chosenHeightTerrainTypes, chosenHeatTerrainTypes, chosenMoistureTerrainTypes, chosenGameTerrainTypes);

        switch (this.visualizationMode)
        {
            case VisualizationMode.Height:
                this.tileRenderer.material.mainTexture = heightTexture;
                break;
            case VisualizationMode.Heat:
                this.tileRenderer.material.mainTexture = heatTexture;
                break;
            case VisualizationMode.Moisture:
                this.tileRenderer.material.mainTexture = moistureTexture;
                break;
            case VisualizationMode.Biome:
                this.tileRenderer.material.mainTexture = biomeTexture;
                break;
            case VisualizationMode.Game:
                //this.tileRenderer.material.shader = myShader;
                this.tileRenderer.material.mainTexture = gameTexture;
                break;
        }
        UpdateMeshVertices(heightMap);

        var tileData = new TileData(heightMap, heatMap, moistureMap, chosenHeatTerrainTypes, chosenHeatTerrainTypes, chosenMoistureTerrainTypes, chosenBiomes, this.meshFilter.mesh, null);

        return tileData;
    }

    private Texture2D BuildTexture(float[,] heightMap, TerrainType[] terrainTypes, TerrainType[,] chosenTerrainTypes)
    {
        int tileDepth = heightMap.GetLength(0);
        int tileWidth = heightMap.GetLength(1);

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

        return tileTexture;
    }

    private Texture2D BuildBiomeTexture(TerrainType[,] heightTerrainTypes, TerrainType[,] heatTerrainTypes, TerrainType[,] moistureTerrainTypes, Biome[,] chosenBiomes)
    {
        int tileDepth = heatTerrainTypes.GetLength(0);
        int tileWidth = heatTerrainTypes.GetLength(1);

        Color[] colorMap = new Color[tileDepth * tileWidth];
        for (int zIndex = 0; zIndex < tileDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < tileWidth; xIndex++)
            {
                int colorIndex = zIndex * tileWidth + xIndex;
                var heightTerrainType = heightTerrainTypes[zIndex, xIndex];
                if (heightTerrainType.name != "water")
                {
                    var heatTerrainType = heatTerrainTypes[zIndex, xIndex];
                    var moistureTerrainType = moistureTerrainTypes[zIndex, xIndex];

                    Biome biome = this.biomes[moistureTerrainType.index].biomes[heatTerrainType.index];
                    colorMap[colorIndex] = biome.color;
                    chosenBiomes[zIndex, xIndex] = biome;
                }
                else
                {
                    colorMap[colorIndex] = Color.blue;
                }
            }
        }

        var tileTexture = new Texture2D(tileWidth, tileDepth);
        tileTexture.filterMode = FilterMode.Point;
        tileTexture.wrapMode = TextureWrapMode.Clamp;
        tileTexture.SetPixels(colorMap);
        tileTexture.Apply();

        return tileTexture;
    }

    private Texture BuildGameTexture(TerrainType[,] heightTerrainTypes, TerrainType[,] heatTerrainTypes, TerrainType[,] moistureTerrainTypes, Biome[,] chosenBiomes)
    {
        int tileDepth = heatTerrainTypes.GetLength(0);
        int tileWidth = heatTerrainTypes.GetLength(1);

        Color[] colorMap = new Color[tileDepth * tileWidth];
        //for (int zIndex = 0; zIndex < tileDepth; zIndex++)
        //{
        //    for (int xIndex = 0; xIndex < tileWidth; xIndex++)
        //    {
                var heightTerrainType = heightTerrainTypes[0, 0];
                var tileTexture = new Texture2D(tileWidth, tileDepth);
                if (heightTerrainType.name != "water")
                {
                    var heatTerrainType = heatTerrainTypes[0, 0];
                    var moistureTerrainType = moistureTerrainTypes[0, 0];

                    Biome biome = this.biomes[moistureTerrainType.index].biomes[heatTerrainType.index];
                    chosenBiomes[0, 0] = biome;
                    return biome.texture;
                }
                else
                {
                    colorMap[0] = Color.blue;
                }
        //    }
        //}

        //var tileTexture = new Texture2D(tileWidth, tileDepth);
        tileTexture.filterMode = FilterMode.Point;
        tileTexture.wrapMode = TextureWrapMode.Clamp;
        tileTexture.SetPixels(colorMap);
        tileTexture.Apply();

        return tileTexture;
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

    private void UpdateMeshVertices(float[,] heightMap)
    {
        int tileDepth = heightMap.GetLength(0);
        int tileWidth = heightMap.GetLength(1);

        var meshVertices = this.meshFilter.mesh.vertices;

        int vertexIndex = 0;
        for (int zIndex = 0; zIndex < tileDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < tileWidth; xIndex++)
            {
                float height = heightMap[zIndex, xIndex];
                var vertex = meshVertices[vertexIndex];
                meshVertices[vertexIndex] = new Vector3(vertex.x, this.heightCurve.Evaluate(height) * this.heightMultiplier, vertex.z);
                vertexIndex++;
            }
        }

        this.meshFilter.mesh.vertices = meshVertices;
        this.meshFilter.mesh.RecalculateBounds();
        this.meshFilter.mesh.RecalculateNormals();
        this.meshCollider.sharedMesh = this.meshFilter.mesh;
    }
}

public class TileData
{
    public float[,] heightMap;
    public float[,] heatMap;
    public float[,] moistureMap;
    public TerrainType[,] chosenHeightTerrainTypes;
    public TerrainType[,] chosenHeatTerrainTypes;
    public TerrainType[,] chosenMoistureTerrainTypes;
    public Biome[,] chosenBiomes;
    public Mesh mesh;
    public Terrain terrain; 

    public TileData(float[,] heightMap, float[,] heatMap, float[,] moistureMap,
        TerrainType[,] chosenHeightTerrainTypes, TerrainType[,] chosenHeatTerrainTypes, TerrainType[,] chosenMoistureTerrainTypes, 
        Biome[,] chosenBiomes, Mesh mesh, Terrain terrain)
    {
        this.heightMap = heightMap;
        this.heatMap = heatMap;
        this.moistureMap = moistureMap;
        this.chosenHeightTerrainTypes = chosenHeightTerrainTypes;
        this.chosenHeatTerrainTypes = chosenHeatTerrainTypes;
        this.chosenMoistureTerrainTypes = chosenMoistureTerrainTypes;
        this.chosenBiomes = chosenBiomes;
        this.mesh = mesh;
        this.terrain = terrain;
    }
}

public class LevelData
{
    private int tileDepthInVertices, tileWidthInVertices;

    public TileData[,] tilesData;

    public LevelData(int tileDepthInVertices, int tileWidthInVertices, int levelDepthInTiles, int levelWidthInTiles)
    {
        tilesData = new TileData[tileDepthInVertices * levelDepthInTiles, tileWidthInVertices * levelWidthInTiles];

        this.tileDepthInVertices = tileDepthInVertices;
        this.tileWidthInVertices = tileWidthInVertices;
    }

    public void AddTileData(TileData tileData, int tileZIndex, int tileXIndex)
    {
        tilesData[tileZIndex, tileXIndex] = tileData;
    }
}
