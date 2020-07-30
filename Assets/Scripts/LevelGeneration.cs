using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGeneration : MonoBehaviour
{
    [SerializeField]
    private int mapWidthInTiles, mapDepthInTiles;

    [SerializeField]
    private GameObject tilePrefab;

    [SerializeField]
    private ObjectGeneration objectGeneration;

    [SerializeField]
    private CreatureGeneration creatureGeneration;

    private new Camera camera;
    private Bounds currentTile;

    private GameObject world;

    private LevelData levelData;
    private Terrain[,] terrains;

    /* Setup world terrains */
    void Start()
    {
        world = new GameObject("WorldTiles");
        camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        terrains = new Terrain[512, 512];

        UpdateMap();
    }

    /*Update map if the camera moved to another tile*/
    private void Update()
    {
        var cameraCurrentPosition = camera.transform.position;
        if (!currentTile.Contains(cameraCurrentPosition))
            UpdateMap();
    }

    public void GenerateMap()
    {
        foreach (Transform child in world.transform)
            Destroy(child.gameObject);
        terrains = new Terrain[512, 512];
        var cameraPosition = camera.transform;
        Camera.main.transform.parent.transform.position = new Vector3(cameraPosition.position.x, 100, cameraPosition.position.z);
        UpdateMap();
    }

    /*
     * Get the position of the camera and then generate terrains around the camera
     * TODO : Move Generate Trees to TerrainGeneration script
     * TODO : Make water plane height adjustable
     */
    public void UpdateMap()
    {
        var tileSize = tilePrefab.GetComponent<Terrain>().terrainData.size;
        int tileWidth = (int)tileSize.x;
        int tileDepth = (int)tileSize.z;

        var currentCameraPosition = camera.transform.position;
        var x = Mathf.Floor(currentCameraPosition.x / tileWidth) * tileWidth;
        var z = Mathf.Floor(currentCameraPosition.z / tileDepth) * tileDepth;

        int minMax = (int)Mathf.Floor(mapWidthInTiles / 2);
        for (int zIndex = -minMax; zIndex <= minMax; zIndex++)
        {
            for (int xIndex = -minMax; xIndex <= minMax; xIndex++)
            {
                var tilePosition = new Vector3(x + xIndex * tileWidth, 0, z + zIndex * tileDepth);
                int tilePositionIndexX = (int)(tilePosition.x / tileSize.x);
                int tilePositionIndexZ = (int)(tilePosition.z / tileSize.z);
                if (tilePosition.x >= 0 && tilePosition.z >= 0 && terrains[tilePositionIndexZ, tilePositionIndexX] == null)
                {
                    var tile = Instantiate(Resources.Load("TerrainAssets/TerrainChunk") as GameObject, tilePosition, Quaternion.identity);
                    
                    tile.transform.parent = world.transform;
                    var terrainTile = tile.GetComponent<TerrainGeneration>().GenerateTile();
                    var terrain = terrainTile.terrain;

                    terrains[tilePositionIndexZ, tilePositionIndexX] = terrain;

                    TerrainGeneration.Fix(terrain, terrains[tilePositionIndexZ, tilePositionIndexX - 1], terrains[tilePositionIndexZ - 1, tilePositionIndexX]);

                    // Create water plane, move and scale it at the center of the terrain 
                    var waterTilePosition = new Vector3(tilePosition.x + (tileSize.x / 2), 39f, tilePosition.z + (tileSize.z / 2));
                    var waterTile = Instantiate(Resources.Load("TerrainAssets/WaterBasicDaytime") as GameObject, waterTilePosition, Quaternion.identity);
                    waterTile.transform.localScale = new Vector3(13, 1, 13);
                    waterTile.transform.parent = terrain.transform;

                    objectGeneration.GenerateTrees(terrainTile);
                    objectGeneration.GenerateGrass(terrainTile);
                    objectGeneration.GenerateRocks(terrainTile);
                    creatureGeneration.GenerationCreatures(terrainTile);
                }
                // Set currentTile to be where camera new position is
                currentTile = new Bounds(new Vector3(x + (tileWidth / 2), 0, z + (tileDepth / 2)), new Vector3(tileWidth, 1000, tileDepth));
            }
        }
    }
}
