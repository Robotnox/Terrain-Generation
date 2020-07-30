using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NoiseMapGeneration;

public class TreeGeneration : MonoBehaviour
{
    [SerializeField]
    private NoiseMapGeneration noiseMapGeneration;

    [SerializeField]
    [Tooltip("Seed allow create different trees location")]
    private int seed;

    [SerializeField]
    [Tooltip("Waves influence how the noisemap is created")]
    private Wave[] waves;

    [SerializeField]
    private float levelScale;

    [SerializeField]
    private float neighborRadius;

    [SerializeField]
    private GameObject treePrefab;

    /*
     * Generate trees for the terrain based on the noisemap and neighbour trees
     * TODO : Need rewriting to properly generate trees
     */
    public void GenerateTrees(TileData terrainTile)
    {
        var terrain = terrainTile.terrain;
        var terrainPosition = terrain.GetPosition();
        var tileSize = (int)terrain.terrainData.size.x;
        var treeMap = this.noiseMapGeneration.GeneratePerlinNoiseMap(tileSize, terrainPosition.x, terrainPosition.z, seed, waves);

        var tree = new TreeInstance();
        tree.prototypeIndex = 0;
        tree.heightScale = 0.7f;
        tree.widthScale = 1;
        for (int zIndex = 0; zIndex < tileSize; zIndex++)
        {
            for (int xIndex = 0; xIndex < tileSize; xIndex++)
            {
                if (terrainTile.chosenHeightTerrainTypes[zIndex,xIndex].name.Equals("ground"))
                {
                    float treeValue = treeMap[zIndex, xIndex];

                    int neighborZBeing = (int)Mathf.Max(0, zIndex - this.neighborRadius);
                    int neighborZEnd = (int)Mathf.Min(tileSize - 1, zIndex + this.neighborRadius);
                    int neighborXBeing = (int)Mathf.Max(0, xIndex - this.neighborRadius);
                    int neighborXEnd = (int)Mathf.Min(tileSize - 1, xIndex + this.neighborRadius);
                    float maxValue = 0f;

                    for (int neighborZ = neighborZBeing; neighborZ <= neighborZEnd; neighborZ++)
                    {
                        for (int neighborX = neighborXBeing; neighborX <= neighborXEnd; neighborX++)
                        {
                            float neighborValue = treeMap[neighborZ, neighborX];
                            if (neighborValue >= maxValue)
                                maxValue = neighborValue;
                        }
                    }

                    if (treeValue == maxValue)
                    {
                        tree.position = new Vector3((float)xIndex / (float)tileSize, -0.4f, (float)zIndex / (float)tileSize);
                        terrain.AddTreeInstance(tree);
                    }
                }
            }
        }
        terrainTile.terrain.Flush();
    }

    public void GenerateRocks(TileData terrainTile)
    {
        var terrain = terrainTile.terrain;
        var terrainPosition = terrain.GetPosition();
        var tileSize = (int)terrain.terrainData.size.x;
        var rocksList = Resources.LoadAll<GameObject>("TerrainAssets/TerrainObjects/RockPackage/Prefabs");

        for (int zIndex = 0; zIndex < tileSize; zIndex++)
        {
            for (int xIndex = 0; xIndex < tileSize; xIndex++)
            {
                if (terrainTile.chosenHeightTerrainTypes[zIndex, xIndex].name.Equals("mountain"))
                {
                    if (UnityEngine.Random.Range(0, 100) == 0)
                    {
                        float x = (float)xIndex + terrainPosition.x;
                        float z = (float)zIndex + terrainPosition.z;
                        float y = terrain.terrainData.GetHeight((int)xIndex, (int)zIndex);
                        var rockPosition = new Vector3(x, y, z);
                        Instantiate(rocksList[UnityEngine.Random.Range(0, rocksList.Length)], 
                            rockPosition, UnityEngine.Random.rotation);
                    }
                }
            }
        }
        terrainTile.terrain.Flush();
    }
}
